using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class Transcript
{
	public string VideoId { get; set; } = string.Empty;
	public string LangCode { get; set; } = string.Empty;
	public string LanguageName { get; set; } = string.Empty;
	public string? Url { get; set; }
	public bool IsGenerated { get; set; }
	public string Text { get; set; } = string.Empty;

	public virtual Video? Video { get; set; }

	public virtual ICollection<TranscriptAnglicism> TranscriptAnglicisms { get; set; } = [];
}
