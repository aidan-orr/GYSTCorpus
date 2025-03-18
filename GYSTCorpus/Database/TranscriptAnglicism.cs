using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class TranscriptAnglicism
{
	public string VideoId { get; set; } = string.Empty;
	public string LangCode { get; set; } = string.Empty;
	public string Word { get; set; } = string.Empty;
	public int TranscriptIndex { get; set; }
	public TreeTagger.Wrapper.PartOfSpeech GermanPos { get; set; }

	public virtual Anglicism Anglicism { get; set; } = default!;

	public virtual Transcript Transcript { get; set; } = default!;
}
