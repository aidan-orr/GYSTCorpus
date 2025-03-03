using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class Channel
{
	[JsonPropertyName("id")]
	public string ChannelId { get; set; } = string.Empty;
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;
	[JsonPropertyName("created")]
	public DateTime PublishedAt { get; set; }
	[JsonPropertyName("videoCount")]
	public int VideoCount { get; set; }
	[JsonPropertyName("uploadsPlaylist")]
	public string PlaylistId { get; set; } = string.Empty;
	//public bool TranscriptsDownloaded { get; set; }

	public virtual ICollection<Video> Videos { get; set; } = new HashSet<Video>();
}
