using GYSTCorpus.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace GYSTCorpus;
public static class TranscriptDownloader
{
	public const string DefaultLang = "de";
	public const string YouTubeVideoUrl = "http://www.youtube.com/watch?v=";

	private const string CaptionListStart = "{\"playerCaptionsTracklistRenderer\"";

	static TranscriptDownloader()
	{

	}

	public static Task<Transcript?> DownloadTranscriptAsync(string videoId) => DownloadTranscriptAsync(videoId, DefaultLang);

	public async static Task<Transcript?> DownloadTranscriptAsync(string videoId, string langCode)
	{
		using HttpClientHandler handler = new();

		handler.AllowAutoRedirect = true;

		using HttpClient client = new(handler);

		//client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:134.0) Gecko/20100101 Firefox/134.0");

		string text = string.Empty;

		bool retry = false;
		int remainingRetries = 5;
		do
		{
			try
			{
				retry = false;
				text = await client.GetStringAsync($"{YouTubeVideoUrl}{videoId}");
			}
			catch (HttpRequestException hre)
			{
				if (hre.StatusCode == HttpStatusCode.TooManyRequests)
				{
					retry = --remainingRetries > 0;
					await Task.Delay(10000);
				}
				else if (hre.HttpRequestError == HttpRequestError.ProxyTunnelError)
					retry = --remainingRetries > 0;
				else if (hre.StatusCode == null)
				{
					//Sock5Proxies.RemoveAt(proxyIndex);
				}
				else throw;
			}
		} while (retry);

		if (remainingRetries <= 0)
		{
			throw new Exception("Too many failures");
		}

		if (string.IsNullOrEmpty(text))
			throw new InvalidOperationException("No data was received");

		int start = text.IndexOf(CaptionListStart);
		if (start < 0)
			return null;

		ReadOnlySpan<char> json = ExtractJson(text.AsSpan(start));
		if (json.Length == 0)
			throw new InvalidOperationException("No captions found");

		TracklistRenderer renderer = JsonSerializer.Deserialize<TracklistRenderer>(json) ?? throw new InvalidOperationException("Failed to retrieve caption information");

		var tracks = renderer.TrackList.CaptionTracks;
		if (!tracks.Any(t => t.LanguageCode == langCode))
			return null;

		var track = tracks.First(t => t.LanguageCode == langCode);

		Transcript transcript = new Transcript
		{
			VideoId = videoId,
			LangCode = langCode,
			LanguageName = track.Name.SimpleText,
			Url = track.BaseUrl,
			IsGenerated = track.Name.SimpleText.EndsWith("(auto-generated)"),
			Text = string.Empty
		};

		string transcriptXml = await client.GetStringAsync(track.BaseUrl);
		if (string.IsNullOrEmpty(transcriptXml))
			throw new InvalidOperationException("No transcript data was received");

		XmlDocument doc = new();
		doc.LoadXml(transcriptXml);

		XmlNode transcriptNode = doc.DocumentElement?.SelectSingleNode("/transcript") ?? throw new InvalidOperationException("Failed to get transcript node in XML");

		StringBuilder sb = new();
		foreach (var node in transcriptNode.ChildNodes)
		{
			if (node is not XmlElement element)
				continue;
			sb.AppendLine(element.InnerText);
		}

		transcript.Text = sb.ToString();

		return transcript;
	}

	public static async Task DownloadTranscriptsAsync(string connectionString, IEnumerable<Channel> channels, int skip = 0)
	{
		const int maxAddsBeforeSave = 100;
		const int batchSize = maxAddsBeforeSave;

		int index = skip;
		foreach (var channel in channels.Skip(skip))
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, true);

			index++;
			Console.WriteLine($"Creating transcripts for channel {channel.Title} ({channel.ChannelId}) {index}/{context.Channels.Count()}");

			int videoCount = context.Videos.Count(v => v.ChannelId == channel.ChannelId);
			int processedTranscripts = context.Transcripts.Count(t => t.Video!.ChannelId == channel.ChannelId);

			Console.WriteLine($"Video Count: {videoCount}, Processed: {processedTranscripts}, Remaining: {videoCount - processedTranscripts}");

			int transcriptCount = 0;

			ConcurrentBag<Transcript> transcripts = [];
			Video[] unprocesedVideos = context.Videos.Where(v => v.ChannelId == channel.ChannelId && !v.Transcripts.Any()).ToArray();

			int processed = 0;

			while (unprocesedVideos.Length - processed > 0)
			{
				transcripts.Clear();

				ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

				async ValueTask Action(Video video, CancellationToken token = default)
				{
					Transcript? transcript = null;
					try
					{
						transcript = await TranscriptDownloader.DownloadTranscriptAsync(video.VideoId);
					}
					catch (Exception e)
					{
						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine($"Transcript download failed for {video.VideoId}: {e.Message}");
						Console.ResetColor();

						return;
					}

					if (transcript is null)
					{
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.WriteLine($"No transcript found for video {video.VideoId}");
						Console.ResetColor();
						return;
					}

					Interlocked.Increment(ref transcriptCount);
					transcripts.Add(transcript);

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"Added transcript for video {video.VideoId}");
					Console.ResetColor();
				}

				//videos.ToList().ForEach(video => Action(video).GetAwaiter().GetResult());

				Video[] videos = unprocesedVideos.Skip(processed).Take(batchSize).ToArray();

				await Parallel.ForEachAsync(videos, options, Action);

				processed += videos.Length;

				if (!transcripts.IsEmpty)
				{
					await context.Transcripts.AddRangeAsync(transcripts);
					await context.SaveChangesAsync();
				}
			}

			Console.WriteLine($"Added {transcriptCount} transcripts for channel {channel.Title} ({channel.ChannelId})\n");
		}
	}

	private static ReadOnlySpan<char> ExtractJson(ReadOnlySpan<char> text)
	{
		const char openBrace = '{';
		const char closeBrace = '}';

		if (text.Length == 0)
			return string.Empty;

		if (text[0] != openBrace)
			throw new InvalidOperationException("Invalid JSON data");

		int pos = 0;
		int stack = 0;
		do
		{
			if (text[pos] == openBrace)
				stack++;
			else if (text[pos] == closeBrace)
				stack--;
		} while (++pos < text.Length && stack > 0);

		if (stack > 0)
			return string.Empty;

		return text.Slice(0, pos);
	}

	private class TracklistRenderer
	{
		[JsonPropertyName("playerCaptionsTracklistRenderer")]
		public required TrackList TrackList { get; set; }
	}

	private class TrackList
	{
		[JsonPropertyName("captionTracks")]
		public List<CaptionTrack> CaptionTracks { get; set; } = [];

		public class CaptionTrack
		{
			[JsonPropertyName("baseUrl")]
			public string BaseUrl { get; set; } = string.Empty;
			[JsonPropertyName("languageCode")]
			public string LanguageCode { get; set; } = string.Empty;
			[JsonPropertyName("kind")]
			public string Kind { get; set; } = string.Empty;
			[JsonPropertyName("name")]
			public SimpleTextName Name { get; set; }
			[JsonPropertyName("vssId")]
			public string VssId { get; set; } = string.Empty;
			[JsonPropertyName("isTranslatable")]
			public bool IsTranslatable { get; set; }
			[JsonPropertyName("trackName")]
			public string TrackName { get; set; } = string.Empty;

			public struct SimpleTextName
			{
				[JsonPropertyName("simpleText")]
				public string SimpleText { get; set; }
			}
		}
	}
}
