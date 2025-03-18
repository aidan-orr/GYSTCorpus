using EFCore.BulkExtensions;
using GYSTCorpus;
using GYSTCorpus.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TreeTagger.Wrapper;

using PartOfSpeech = GYSTCorpus.Database.PartOfSpeech;

namespace GYSTCorpus;
public static class TranscriptAnalyzer
{
	public static IEnumerable<string> FindAllWords(string connectionString, int maxTasks = 64)
	{
		CultureInfo german = CultureInfo.GetCultureInfo("de");

		Transcript[] allTranscripts;
		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			allTranscripts = context.Transcripts.AsNoTracking().ToArray();
		}

		Console.WriteLine($"Loaded {allTranscripts.Length} transcripts");

		ConcurrentDictionary<string, int> words = [];

		int processed = 0;
		int runningThreads = 0;

		void Action(Transcript transcript)
		{
			string[] textWords = transcript.Text.Replace('\r', ' ').Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			int added = 0;

			foreach (var word in textWords)
			{
				string lower = word.ToLower(german);

				if (words.TryAdd(lower, 0))
					added++;
			}

			Console.WriteLine($"Found {added} new words in transcript {transcript.VideoId}-{transcript.LangCode} ({Interlocked.Increment(ref processed)}/{allTranscripts.Length})");
		}

		for (int i = 0; i < allTranscripts.Length; ++i)
		{
			while (runningThreads >= maxTasks)
			{
				Thread.Sleep(10);
			}

			Transcript transcript = allTranscripts[i];

			Interlocked.Increment(ref runningThreads);
			_ = Task.Run(() => { Action(transcript); Interlocked.Decrement(ref runningThreads); });
		}

		return words.Keys;
	}

	public static long CountWords(string connectionString, int maxTasks = 64)
	{
		Transcript[] transcripts;
		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			transcripts = context.Transcripts.AsNoTracking().ToArray();
		}

		long count = 0;

		Parallel.ForEach(transcripts, transcript =>
		{
			string[] words = transcript.Text.Replace(' ', '\r', '\n', ':', ',', '.', ';', '?', '!', '=', '/', '[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			Interlocked.Add(ref count, words.Length);
		});

		return count;
	}

	private static FrozenDictionary<string, List<(int WordIndex, int TextIndex, TreeTagger.Wrapper.PartOfSpeech GermanPos)>> CreateTranscriptLookupWithPos(string text, CultureInfo culture, out (string Word, TreeTagger.Wrapper.PartOfSpeech GermanPos)[] taggedWords)
	{
		Dictionary<string, List<(int WordIndex, int TextIndex, TreeTagger.Wrapper.PartOfSpeech GermanPos)>> lookup = [];

		string lower = text.ToLower(culture);
		taggedWords = TreeTagger.Wrapper.Invoke.TagPartsOfSpeech(text.Replace(' ', '\r', '\n', ':', ',', '.', ';', '?', '!', '=', '/', '[', ']'), culture).ToArray();

		int startIndex = 0;
		foreach (var (tagged, i) in taggedWords.WithIndex())
		{
			if (!lookup.ContainsKey(tagged.Word))
				lookup[tagged.Word] = [];

			List<(int WordIndex, int TextIndex, TreeTagger.Wrapper.PartOfSpeech GermanPos)> indices = lookup[tagged.Word];

			int index = lower.IndexOf(tagged.Word, startIndex);
			if (index != -1)
				startIndex = index + tagged.Word.Length;

			indices.Add((i, index, tagged.GermanPos));
		}

		return lookup.ToFrozenDictionary();
	}

	private static FrozenDictionary<string, List<(int WordIndex, int TextIndex)>> CreateTranscriptLookup(string text, CultureInfo info, out string[] words)
	{
		Dictionary<string, List<(int WordIndex, int TextIndex)>> lookup = [];
		text = text.ToLower(info);
		words = text.Replace(' ', '\r', '\n', ':', ',', '.', ';', '?', '!', '=', '/', '[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		foreach (var (word, i) in words.WithIndex())
		{
			if (!lookup.ContainsKey(word))
				lookup[word] = [];

			List<(int WordIndex, int TextIndex)> indices = lookup[word];

			int startIndex = indices.Count == 0 ? 0 : indices.Last().TextIndex + word.Length;

			indices.Add((i, text.IndexOf(word, startIndex)));
		}

		return lookup.ToFrozenDictionary();
	}

	private static FrozenDictionary<string, List<int>> CreateTranscriptLookupFast(string text, CultureInfo info, out string[] words)
	{
		Dictionary<string, List<int>> lookup = [];
		text = text.ToLower(info);
		words = text.Replace(' ', '\r', '\n', ':', ',', '.', ';', '?', '!', '=', '/', '[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		foreach (var (word, i) in words.WithIndex())
		{
			if (!lookup.ContainsKey(word))
				lookup[word] = [];

			List<int> indices = lookup[word];

			indices.Add(i);
		}

		return lookup.ToFrozenDictionary();
	}

	private static void AnalyzeWindow(ConcurrentDictionary<AnglicismContextKey, AtomicInteger> contextWindows, int year, int categoryId, string[] orderedWords, int wordIndex, int windowSize, IDictionary<string, PartOfSpeech> taggedWords, params ReadOnlySpan<PartOfSpeech> allowedParts)
	{
		const int maxStackWindowSize = 50;

		Span<int> rightWordIndices = windowSize <= maxStackWindowSize ? stackalloc int[maxStackWindowSize] : new int[maxStackWindowSize];
		Span<int> leftWordIndices = windowSize <= maxStackWindowSize ? stackalloc int[maxStackWindowSize] : new int[maxStackWindowSize];

		ReadOnlySpan<int> reinterpAllowedParts = MemoryMarshal.Cast<PartOfSpeech, int>(allowedParts);

		string targetWord = orderedWords[wordIndex];

		int rightExtentCount = 0;
		for (int rightExtent = wordIndex + 1; rightExtent < orderedWords.Length && rightExtentCount < windowSize; rightExtent++)
		{
			string word = orderedWords[rightExtent];

			if (word == targetWord)
				return; // target word is within the context, ignore it

			if (taggedWords.TryGetValue(word, out PartOfSpeech pos) && reinterpAllowedParts.Contains((int)pos))
				rightWordIndices[rightExtentCount++] = rightExtent;
		}

		int leftExtentCount = 0;
		for (int leftExtent = wordIndex - 1; leftExtent >= 0 && leftExtentCount < windowSize; leftExtent--)
		{
			string word = orderedWords[leftExtent];
			if (taggedWords.TryGetValue(word, out PartOfSpeech pos) && reinterpAllowedParts.Contains((int)pos))
			{
				leftWordIndices[leftExtentCount++] = leftExtent;
			}
		}

		int fullSize = leftExtentCount + rightExtentCount;

		Span<int> wordWindowIndices = fullSize <= 2 * maxStackWindowSize ? stackalloc int[2 * maxStackWindowSize] : new int[fullSize];
		leftWordIndices[..leftExtentCount].CopyTo(wordWindowIndices);
		rightWordIndices[..rightExtentCount].CopyTo(wordWindowIndices[leftExtentCount..]);

		for (int i = 0; i < fullSize; i++)
		{
			string word = orderedWords[wordWindowIndices[i]];
			contextWindows.GetOrAdd(new(targetWord, year, categoryId, word, TreeTagger.Wrapper.PartOfSpeech.None), new AtomicInteger(0)).Increment();
		}
	}

	private static void AnalyzeWindowTreeTagger(ConcurrentDictionary<AnglicismContextKey, AtomicInteger> contextWindows, int year, int categoryId, (string Word, TreeTagger.Wrapper.PartOfSpeech PartOfSpeech)[] orderedWords, int wordIndex, int windowSize, params ICollection<TreeTagger.Wrapper.PartOfSpeech> allowedParts)
	{
		const int maxStackWindowSize = 50;

		Span<int> rightWordIndices = windowSize <= maxStackWindowSize ? stackalloc int[maxStackWindowSize] : new int[maxStackWindowSize];
		Span<int> leftWordIndices = windowSize <= maxStackWindowSize ? stackalloc int[maxStackWindowSize] : new int[maxStackWindowSize];

		string targetWord = orderedWords[wordIndex].Word;

		int rightExtentCount = 0;
		for (int rightExtent = wordIndex + 1; rightExtent < orderedWords.Length && rightExtentCount < windowSize; rightExtent++)
		{
			var word = orderedWords[rightExtent];

			if (word.Word == targetWord)
				return; // target word is within the context, ignore it

			if (allowedParts.Contains(word.PartOfSpeech))
				rightWordIndices[rightExtentCount++] = rightExtent;
		}

		int leftExtentCount = 0;
		for (int leftExtent = wordIndex - 1; leftExtent >= 0 && leftExtentCount < windowSize; leftExtent--)
		{
			var word = orderedWords[leftExtent];

			if (allowedParts.Contains(word.PartOfSpeech))
				leftWordIndices[leftExtentCount++] = leftExtent;
		}

		int fullSize = leftExtentCount + rightExtentCount;

		Span<int> wordWindowIndices = fullSize <= 2 * maxStackWindowSize ? stackalloc int[2 * maxStackWindowSize] : new int[fullSize];
		leftWordIndices[..leftExtentCount].CopyTo(wordWindowIndices);
		rightWordIndices[..rightExtentCount].CopyTo(wordWindowIndices[leftExtentCount..]);

		for (int i = 0; i < fullSize; i++)
		{
			var word = orderedWords[wordWindowIndices[i]];
			contextWindows.GetOrAdd(new(targetWord, year, categoryId, word.Word, word.PartOfSpeech), new AtomicInteger(0)).Increment();
		}
	}

	private readonly record struct VideoInfo(string VideoId, DateTime PublishedAt, int CategoryId);

	/// <summary>
	/// Analyzes all transcripts in the database for anglicisms and their context words and parts of speech
	/// </summary>
	/// <param name="connectionString"></param>
	/// <param name="maxTasks"></param>
	public static void AnalyzeWithPartOfSpeech(string connectionString, int maxTasks = 64)
	{
		const int windowSize = 50;

		CultureInfo german = CultureInfo.GetCultureInfo("de");

		string[] anglicisms; // List of anglicisms to search for
		Transcript[] allTranscripts; // All transcripts in the database
		FrozenDictionary<string, VideoInfo> videoLookup; // Lookup table for videos by video id

		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			anglicisms = context.Anglicisms.Select(a => a.Word).ToArray();

			allTranscripts = context.Transcripts.Where(t => t.LangCode == "de").Include(t => t.Video).AsNoTracking().ToArray();
			videoLookup = allTranscripts.DistinctBy(t => t.VideoId).Select(t => new VideoInfo(t.VideoId, t.Video!.PublishedAt, t.Video.CategoryId)).ToFrozenDictionary(t => t.VideoId, t => t);
		}

		// Concurrent dictionary to store the context window information
		ConcurrentDictionary<AnglicismContextKey, AtomicInteger> contextWindows = new ConcurrentDictionary<AnglicismContextKey, AtomicInteger>(maxTasks, 128 * 1024 * 1024);

		void Action(Transcript transcript, AtomicInteger processed, ConcurrentQueue<TranscriptAnglicism> transcriptAnglicisms, long index, CancellationToken token)
		{
			VideoInfo video = videoLookup[transcript.VideoId];
			int year = video.PublishedAt.Year;
			int category = video.CategoryId;

			FrozenDictionary<string, List<(int WordIndex, int TextIndex, TreeTagger.Wrapper.PartOfSpeech PartOfSpeech)>> transcriptLookup = CreateTranscriptLookupWithPos(transcript.Text, german, out (string Word, TreeTagger.Wrapper.PartOfSpeech PartOfSpeech)[] orderedWords);

			int found = 0;

			foreach (string anglicism in anglicisms)
			{
				if (transcriptLookup.TryGetValue(anglicism, out List<(int WordIndex, int TextIndex, TreeTagger.Wrapper.PartOfSpeech PartOfSpeech)>? indices) && indices is not null)
				{
					foreach (var (wordIndex, textIndex, partOfSpeech) in indices)
					{
						AnalyzeWindowTreeTagger(contextWindows, year, category, orderedWords, wordIndex, windowSize, TreeTagger.Wrapper.PartOfSpeechHelpers.TargetPartsOfSpeech);

						TranscriptAnglicism ta = new TranscriptAnglicism
						{
							VideoId = transcript.VideoId,
							LangCode = transcript.LangCode,
							Word = anglicism,
							TranscriptIndex = textIndex,
							GermanPos = partOfSpeech
						};

						transcriptAnglicisms.Enqueue(ta);
						found++;
					}
				}
			}

			allTranscripts[index] = null!; // Try to free up memory

			Console.WriteLine($"Found {found} anglicisms in transcript {transcript.VideoId}-{transcript.LangCode} ({processed.Increment()}/{allTranscripts.Length})");
		}

		// Force full collection before starting
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

		ParallelTaskExecutor.ForEachWithSave<Transcript, TranscriptAnglicism>(allTranscripts, Action, (transcriptAnglicisms, cancellation) =>
		{
			if (cancellation.IsCancellationRequested)
				return;

			using TranscriptsContext context = new TranscriptsContext(connectionString, false);
			while (transcriptAnglicisms.TryDequeue(out TranscriptAnglicism? anglicism) && anglicism is not null)
			{
				context.TranscriptAnglicism.Add(anglicism);
			}

			context.SaveChanges();
		}, maxTasks: maxTasks);

		AnglicismContextKey[] keys = GC.AllocateUninitializedArray<AnglicismContextKey>(contextWindows.Count);
		contextWindows.Keys.CopyTo(keys, 0);

		Console.WriteLine($"Saving context windows to database");

		// Force collection before saving to database
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

		int toProcess = contextWindows.Count;

		const int chunkSize = 1000 * 1000;
		int finished = 0;

		BulkConfig bulkConfig = new BulkConfig { BatchSize = 10000 };
		foreach (var group in contextWindows.Chunk(chunkSize))
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, false);


			AnglicismContextWindow[] windows = new AnglicismContextWindow[group.Length];

			Parallel.ForEach(group, (value, _, i) =>
			{
				windows[i] = new AnglicismContextWindow
				{
					Anglicism = value.Key.Anglicism,
					Year = value.Key.Year,
					Category = value.Key.Category,
					ContextWord = value.Key.ContextWord,
					GermanPos = value.Key.GermanPos,
					Count = value.Value
				};
			});

			context.BulkInsert(windows, bulkConfig);
			context.SaveChanges();

			Console.WriteLine($"Finished {Interlocked.Increment(ref finished)} / {Math.Ceiling(contextWindows.Count / (double)chunkSize)} steps");
		}
	}

	public static Dictionary<AnglicismContextKey, int> Analyze(string connectionString, int maxTasks = 64)
	{
		CultureInfo german = CultureInfo.GetCultureInfo("de");

		string[] anglicisms; // List of anglicisms to search for
		Transcript[] allTranscripts; // All transcripts in the database
		FrozenDictionary<string, Video> videoLookup; // Lookup table for videos by video id
		FrozenDictionary<string, PartOfSpeech> taggedWords; // Lookup table for part of speech by word

		HashSet<string> processedVideos = [];

		// Copy all of the data from the database into memory
		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			anglicisms = context.Anglicisms.Select(a => a.Word).ToArray();

			taggedWords = context.WordPartOfSpeech.ToFrozenDictionary(w => w.Word, w => w.GermanPartOfSpeech);

			allTranscripts = context.Transcripts.Include(t => t.Video).AsNoTracking().ToArray();
			videoLookup = allTranscripts.DistinctBy(t => t.VideoId).ToFrozenDictionary(t => t.VideoId, t => t.Video!);

			processedVideos = new HashSet<string>(context.TranscriptAnglicism.AsNoTracking().Select(t => t.VideoId).Distinct().ToArray());
		}

		// Concurrent dictionary to store the context window information
		ConcurrentDictionary<AnglicismContextKey, AtomicInteger> contextWindows = new ConcurrentDictionary<AnglicismContextKey, AtomicInteger>(maxTasks, 128 * 1024 * 1024);

		// List of completed video ids, int is just a placeholder
		ConcurrentDictionary<string, int> completedVideos = [];

		void Action(Transcript transcript, AtomicInteger processed, ConcurrentQueue<TranscriptAnglicism> transcriptAnglicisms, CancellationToken token)
		{
			const int windowSize = 25;

			if (completedVideos.TryGetValue(transcript.VideoId, out _))
			{
				Console.WriteLine($"This should not happen");
			}

			if (processedVideos.Contains(transcript.VideoId))
			{
				Console.WriteLine($"Already found anglicisms in transcript {transcript.VideoId}-{transcript.LangCode} ({processed.Increment()}/{allTranscripts.Length})");
				return;
			}

			Video video = videoLookup[transcript.VideoId];
			int year = video.PublishedAt.Year;
			int category = video.CategoryId;

			FrozenDictionary<string, List<(int WordIndex, int TextIndex)>> transcriptLookup = CreateTranscriptLookup(transcript.Text, german, out string[] orderedWords);

			int found = 0;

			foreach (string anglicism in anglicisms)
			{
				if (transcriptLookup.TryGetValue(anglicism, out List<(int WordIndex, int TextIndex)>? indices) && indices is not null)
				{
					foreach (var (wordIndex, textIndex) in indices)
					{
						AnalyzeWindow(contextWindows, year, category, orderedWords, wordIndex, windowSize, taggedWords, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Adverb);

						TranscriptAnglicism ta = new TranscriptAnglicism
						{
							VideoId = transcript.VideoId,
							LangCode = transcript.LangCode,
							Word = anglicism,
							TranscriptIndex = textIndex
						};

						transcriptAnglicisms.Enqueue(ta);
						found++;
					}
				}
			}

			completedVideos.TryAdd(transcript.VideoId, 0);

			Console.WriteLine($"Found {found} anglicisms in transcript {transcript.VideoId}-{transcript.LangCode} ({processed.Increment()}/{allTranscripts.Length})");
		}

		TimeSpan collectInterval = TimeSpan.FromMinutes(1);
		DateTime lastCollect = DateTime.UtcNow;

		GC.Collect();

		ParallelTaskExecutor.ForEachWithSave<Transcript, TranscriptAnglicism>(allTranscripts, Action, (transcriptAnglicisms, _) =>
		{
			//if (DateTime.UtcNow - lastCollect >= collectInterval)
			//{
			//	GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
			//	lastCollect = DateTime.UtcNow;
			//}

			using TranscriptsContext context = new TranscriptsContext(connectionString, false);
			while (transcriptAnglicisms.TryDequeue(out TranscriptAnglicism? anglicism) && anglicism is not null)
			{
				context.TranscriptAnglicism.Add(anglicism);
			}

			context.SaveChanges();
		}, maxTasks: maxTasks);

		Dictionary<AnglicismContextKey, int> outputContextWindows = [];

		AnglicismContextKey[] keys = GC.AllocateUninitializedArray<AnglicismContextKey>(contextWindows.Count);
		contextWindows.Keys.CopyTo(keys, 0);

		foreach (AnglicismContextKey key in keys)
		{
			if (contextWindows.TryRemove(key, out AtomicInteger? counts) && counts is not null)
				outputContextWindows[key] = counts.Value;
		}

		contextWindows = null!; // Force this object to be collected

		Console.WriteLine($"Saving context windows to database");

		GC.Collect();

		foreach (var group in keys.Chunk(128))
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, false);

			foreach (AnglicismContextKey key in group)
			{
				if (outputContextWindows.Remove(key, out int count))
				{
					AnglicismContextWindow window = new AnglicismContextWindow
					{
						Anglicism = key.Anglicism,
						Year = key.Year,
						Category = key.Category,
						ContextWord = key.ContextWord,
						Count = count
					};

					context.AnglicismContextWindows.Add(window);
				}
			}

			context.SaveChanges();
		}

		return outputContextWindows;
	}

	/// <summary>
	/// Analyzes all transcripts in the database for anglicisms and their context words
	/// Assumes that the database is already populated with the list of anglicisms in transcripts
	/// </summary>
	/// <param name="connectionString">The connection string for the database.</param>
	/// <param name="maxTasks">The maximum number of concurrent tasks. Default is 64.</param>
	public static void FastAnalyze(string connectionString, int maxTasks = 64)
	{
		CultureInfo german = CultureInfo.GetCultureInfo("de");

		string[] anglicisms;
		Transcript[] allTranscripts;
		FrozenDictionary<string, Video> videoLookup;
		FrozenDictionary<string, PartOfSpeech> taggedWords;

		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			anglicisms = context.TranscriptAnglicism.ToArray().DistinctBy(t => t.Word).Select(t => t.Word).ToArray();
			taggedWords = context.WordPartOfSpeech.AsNoTracking().ToFrozenDictionary(w => w.Word, w => w.GermanPartOfSpeech);
			allTranscripts = context.Transcripts.AsNoTracking().Include(t => t.Video).ToArray();

			videoLookup = allTranscripts.DistinctBy(t => t.VideoId).ToFrozenDictionary(t => t.VideoId, t => t.Video!);
		}

		ConcurrentDictionary<AnglicismContextKey, AtomicInteger> contextWindows = new ConcurrentDictionary<AnglicismContextKey, AtomicInteger>(maxTasks, 80 * 1024 * 1024);

		AtomicInteger processed = 0;
		double percentStep = 0.1;
		double nextPercent = percentStep;

		void Action(Transcript transcript)
		{
			const int windowSize = 25;

			Video video = videoLookup[transcript.VideoId];
			int year = video.PublishedAt.Year;
			int category = video.CategoryId;

			FrozenDictionary<string, List<int>> transcriptLookup = CreateTranscriptLookupFast(transcript.Text, german, out string[] orderedWords);

			int found = 0;

			foreach (string anglicism in anglicisms)
			{
				if (transcriptLookup.TryGetValue(anglicism, out List<int>? indices) && indices is not null)
				{
					foreach (int wordIndex in indices)
					{
						AnalyzeWindow(contextWindows, year, category, orderedWords, wordIndex, windowSize, taggedWords, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Adverb);
						found++;
					}
				}
			}

			double percent;
			if ((percent = 100.0 * processed.Increment() / allTranscripts.Length) >= nextPercent)
			{
				nextPercent += percentStep;
				Console.WriteLine($"Processed {percent:F1}% of transcripts");
			}
		}

		TimeSpan collectInterval = TimeSpan.FromSeconds(60);
		DateTime lastCollect = DateTime.UtcNow;

		GC.Collect();
		int runningTasks = 0;
		foreach (var transcript in allTranscripts)
		{
			while (runningTasks >= maxTasks)
				Thread.Sleep(10);

			Interlocked.Increment(ref runningTasks);
			_ = Task.Run(() => { Action(transcript); Interlocked.Decrement(ref runningTasks); });
		}

		GC.Collect();

		Console.WriteLine($"Saving context windows to database");

		int toProcess = contextWindows.Count;

		runningTasks = 0;

		const int chunkSize = 1000 * 1000;
		int finished = 0;

		foreach (var group in contextWindows.Chunk(chunkSize))
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, false);

			BulkConfig bulkConfig = new BulkConfig { BatchSize = 10000 };

			AnglicismContextWindow[] windows = new AnglicismContextWindow[group.Length];

			Parallel.ForEach(group, (value, _, i) =>
			{
				windows[i] = new AnglicismContextWindow
				{
					Anglicism = value.Key.Anglicism,
					Year = value.Key.Year,
					Category = value.Key.Category,
					ContextWord = value.Key.ContextWord,
					Count = value.Value
				};
			});

			context.BulkInsert(windows, bulkConfig);
			context.SaveChanges();

			Console.WriteLine($"Finished {Interlocked.Increment(ref finished)} / {Math.Ceiling(contextWindows.Count / (double)chunkSize)} steps");
		}

		//foreach (var group in keys.Chunk(1024))
		//{
		//	while (runningTasks >= 16)
		//		Thread.Sleep(10);

		//	Interlocked.Increment(ref runningTasks);

		//	_ = Task.Run(() =>
		//	{
		//		using TranscriptsContext context = new TranscriptsContext(connectionString, false);

		//		foreach (AnglicismContextKey key in group)
		//		{
		//			if (contextWindows.TryRemove(key, out AtomicInteger? count) && count is not null)
		//			{
		//				AnglicismContextWindow window = new AnglicismContextWindow
		//				{
		//					Anglicism = key.Anglicism,
		//					Year = key.Year,
		//					Category = key.Category,
		//					ContextWord = key.ContextWord,
		//					Count = count
		//				};

		//				context.AnglicismContextWindows.Add(window);
		//			}

		//			Interlocked.Increment(ref windowsProcessed);
		//		}

		//		float percent;
		//		while ((percent = 100.0f * windowsProcessed / toProcess) >= nextPercent)
		//		{
		//			Console.WriteLine($"Processed {percent:F1}% of context windows");
		//			nextPercent += percentIncrement;
		//		}

		//		context.SaveChanges();

		//		Interlocked.Decrement(ref runningTasks);
		//	});
		//}
	}

	public static void StripWords(string connectionString, params IEnumerable<string> words)
	{
		Transcript[] allTranscripts;

		using (TranscriptsContext context = new TranscriptsContext(connectionString, false))
		{
			allTranscripts = context.Transcripts.ToArray();
		}

		int count = allTranscripts.Length;

		int i = 0;
		foreach (var transcripts in allTranscripts.Chunk(1024))
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, false);

			Parallel.ForEach(transcripts, transcript =>
			{
				foreach (var word in words)
				{
					transcript.Text = transcript.Text.Replace(word, string.Empty);
				}

				Console.WriteLine($"Stripped transcript {transcript.Video} ({Interlocked.Increment(ref i)}/{count})");
			});

			context.Transcripts.UpdateRange(transcripts);
			context.SaveChanges();
		}
	}

	public static void CalculateEntropies(string connectionString, int startYear, int endYear, string outputDir)
	{
		using TranscriptsContext context = new TranscriptsContext(connectionString, false);


		int[] years = Enumerable.Range(startYear, endYear - startYear + 1).ToArray();
		int[] categoryIds = [1, 2, 10, 15, 17, 18, 19, 20, 21, 22, 23, 24, 26, 27, 28, 29];
		FrozenDictionary<int, string> categoryNames = new Dictionary<int, string>
		{
			[1] = "film_anmiation",
			[2] = "autos_vehicles",
			[10] = "music",
			[15] = "pets_anmials",
			[17] = "sports",
			[18] = "short_movies",
			[19] = "travel_events",
			[20] = "gaming",
			[21] = "videoblogging",
			[22] = "people_blogs",
			[23] = "comedy",
			[24] = "entertainment",
			[25] = "news_politics",
			[26] = "howto_style",
			[27] = "education",
			[28] = "science_technology",
			[29] = "nonprofits_activism"
		}.ToFrozenDictionary();

		var anglicisms = (from ta in context.TranscriptAnglicism
						  join a in context.Anglicisms on ta.Word equals a.Word
						  group ta by a.Word into g
						  orderby g.Count() descending
						  select g.Key).ToArray();

		float[,,] entropies = new float[categoryIds.Length, anglicisms.Length * PartOfSpeechHelpers.OutputCategories.Length, years.Length];
		long[,,] counts = new long[categoryIds.Length, anglicisms.Length * PartOfSpeechHelpers.OutputCategories.Length, years.Length];

		Parallel.ForEach(anglicisms.WithIndex(), new ParallelOptions { MaxDegreeOfParallelism = 1 }, word =>
		{
			using TranscriptsContext innerContext = new TranscriptsContext(connectionString, false);

			var allWindows = innerContext.AnglicismContextWindows.Where(w => w.Anglicism == word.Value).Select(w => new { w.Count, w.Category, w.GermanPos, w.Year }).ToArray();

			var anglicismMatches = innerContext.TranscriptAnglicism
				.Include(t => t.Transcript)
				.ThenInclude(t => t.Video)
				.Where(t => t.Word == word.Value)
				.Select(t => new { t.Word, t.GermanPos, t.Transcript!.Video!.CategoryId, t.Transcript.Video.PublishedAt })
				.AsEnumerable()
				.Select(t => (t.GermanPos, t.CategoryId, t.PublishedAt.Year)).ToArray();

			for (int ci = 0; ci < categoryIds.Length; ci++)
			{
				int category = categoryIds[ci];
				var catWindows = allWindows.Where(w => w.Category == category).Select(w => (w.Count, w.Year, w.GermanPos));

				var catAnglicisms = anglicismMatches.Where(w => w.CategoryId == category).ToArray();

				int wordStartIndex = word.Index * PartOfSpeechHelpers.OutputCategories.Length;

				// Calculate the overall entropy for all years in a category
				for (int yi = 0; yi < years.Length; yi++)
				{
					int year = years[yi];

					var windows = catWindows.Where(w => w.Year == year).Select(w => w.Count).ToArray();
					var yearAnglicisms = catAnglicisms.Where(w => w.Year == year).ToArray();

					long total = windows.Sum(w => (long)w);

					float entropy = 0;
					foreach (var window in windows)
					{
						float p = (float)window / total;
						entropy += p * MathF.Log2(p);
					}

					entropies[ci, wordStartIndex, yi] = -entropy;
					counts[ci, wordStartIndex, yi] = yearAnglicisms.Length;
				}

				// Calculate the entropy for each part of speech
				for (int posIndex = 1; posIndex <= PartOfSpeechHelpers.CategoryCount; posIndex++)
				{
					string pos = PartOfSpeechHelpers.OutputCategories[posIndex];

					var posWindows = catWindows.Where(w => w.GermanPos.ToCategory() == pos).Select(w => (w.Count, w.Year));
					var posAnglicisms = catAnglicisms.Where(w => w.GermanPos.ToCategory() == pos);

					for (int yi = 0; yi < years.Length; yi++)
					{
						int year = years[yi];

						var windows = posWindows.Where(w => w.Year == year).Select(w => w.Count).ToArray();
						var yearAnglicisms = posAnglicisms.Where(w => w.Year == year).ToArray();

						long total = windows.Sum(w => (long)w);

						float entropy = 0;
						foreach (var window in windows)
						{
							float p = (float)window / total;
							entropy += p * MathF.Log2(p);
						}

						entropies[ci, wordStartIndex + posIndex, yi] = -entropy;
						counts[ci, wordStartIndex + posIndex, yi] = yearAnglicisms.Length;
					}
				}

				// Calculate the entropy for other parts of speech
				{
					var posWindows = catWindows.Where(w => !w.GermanPos.IsTargetPartOfSpeech()).Select(w => (w.Count, w.Year));
					var posAnglicisms = catAnglicisms.Where(w => !w.GermanPos.IsTargetPartOfSpeech());

					for (int yi = 0; yi < years.Length; yi++)
					{
						int year = years[yi];

						var windows = posWindows.Where(w => w.Year == year).Select(w => w.Count).ToArray();
						var yearAnglicisms = posAnglicisms.Where(w => w.Year == year).ToArray();

						long total = windows.Sum(w => (long)w);

						float entropy = 0;
						foreach (var window in windows)
						{
							float p = (float)window / total;
							entropy += p * MathF.Log2(p);
						}

						entropies[ci, wordStartIndex + PartOfSpeechHelpers.OutputCategories.Length - 1, yi] = -entropy;
						counts[ci, wordStartIndex + PartOfSpeechHelpers.OutputCategories.Length - 1, yi] = yearAnglicisms.Length;
					}
				}

				Console.WriteLine($"Finished entropies for word '{word}', Category {categoryNames[category]}");
			}
		});

		Console.WriteLine("Finished calculating entropies");

		for (int ci = 0; ci < categoryIds.Length; ci++)
		{
			int category = categoryIds[ci];

			StringBuilder csvBuilder = new();
			csvBuilder.Append("Word,Part of Speech,");

			foreach(int year in years)
			{
				csvBuilder.Append(year).Append(" - Entropy,");
				csvBuilder.Append(year).Append(" - Count,");
			}

			csvBuilder.AppendLine();

			for (int wi = 0; wi < anglicisms.Length; wi++)
			{
				for (int i = 0; i < PartOfSpeechHelpers.OutputCategories.Length; ++i)
				{
					csvBuilder.Append(anglicisms[wi]).Append(',');
					csvBuilder.Append(PartOfSpeechHelpers.OutputCategories[i]).Append(',');

					for (int yi = 0; yi < years.Length; yi++)
					{
						csvBuilder.Append(entropies[ci, wi * PartOfSpeechHelpers.OutputCategories.Length + i, yi]).Append(',');
						csvBuilder.Append(counts[ci, wi * PartOfSpeechHelpers.OutputCategories.Length + i, yi]);

						if (yi < years.Length - 1)
							csvBuilder.Append(',');
					}
					
					csvBuilder.AppendLine();
				}
			}

			if (!Directory.Exists(Path.Join(outputDir, "categories")))
				Directory.CreateDirectory(Path.Join(outputDir, "categories"));

			File.WriteAllText(Path.Join(outputDir, "categories", $"{categoryNames[category]}.csv"), csvBuilder.ToString());
		}
	}

	public static async Task UpdateAnglicisms(string connectionString, Dictionary<string, Dictionary<string, PartsOfSpeech>> anglicisms)
	{
		int count = 0;
		foreach (var (baseWord, words) in anglicisms)
		{
			using TranscriptsContext context = new TranscriptsContext(connectionString, true);

			List<Anglicism> toBeAdded = [];

			if (!words.ContainsKey(baseWord))
				words.Add(baseWord, new PartsOfSpeech { DeutschPartOfSpeech = PartOfSpeech.Other, EnglishPartOfSpeech = PartOfSpeech.Other });

			foreach (var (word, pos) in words)
			{
				if ((word != baseWord && anglicisms.ContainsKey(word)) || context.Anglicisms.Any(a => a.Word == word))
					continue;

				Anglicism a = new Anglicism
				{
					Word = word,
					BaseWord = baseWord,
					GermanPos = pos.DeutschPartOfSpeech,
					EnglishPos = pos.EnglishPartOfSpeech
				};

				toBeAdded.Add(a);
			}

			await context.Anglicisms.AddRangeAsync(toBeAdded);

			await context.SaveChangesAsync();

			Console.WriteLine($"Added {toBeAdded.Count} generated words for base {baseWord} ({++count}/{anglicisms.Count})");
		}
	}
}

public record struct PartsOfSpeech
{
	[JsonPropertyName("de_pos")]
	public PartOfSpeech DeutschPartOfSpeech { get; set; }
	[JsonPropertyName("en_pos")]
	public PartOfSpeech EnglishPartOfSpeech { get; set; }
}