using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class Video
{
	[JsonPropertyName("id")]
	public string VideoId { get; set; } = string.Empty;
	[JsonPropertyName("channelId")]
	public string ChannelId { get; set; } = string.Empty;
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;
	[JsonPropertyName("publishedAt")]
	public DateTime PublishedAt { get; set; }
	[JsonPropertyName("live")]
	public LiveStreamStatus LiveStreamStatus { get; set; }
	[JsonPropertyName("categoryId")]
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public int CategoryId { get; set; }
	[JsonPropertyName("captionsEnabled")]
	[JsonConverter(typeof(BooleanConverter))]
	public bool CaptionsEnabled { get; set; }

	public virtual Channel? Channel { get; set; }
	public virtual ICollection<Transcript> Transcripts { get; set; } = new HashSet<Transcript>();
}

[JsonConverter(typeof(JsonStringEnumConverter<LiveStreamStatus>))]
public enum LiveStreamStatus
{
	[JsonStringEnumMemberName("upcoming")]
	Upcoming,
	[JsonStringEnumMemberName("live")]
	Live,
	[JsonStringEnumMemberName("none")]
	None
}