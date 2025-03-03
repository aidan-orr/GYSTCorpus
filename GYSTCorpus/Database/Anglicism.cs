using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class Anglicism
{
	public string Word { get; set; } = string.Empty;
	public string BaseWord { get; set; } = string.Empty;

	public PartOfSpeech EnglishPos { get; set; }
	public PartOfSpeech GermanPos { get; set; }

	public float Entropy { get; set; }

	public virtual ICollection<TranscriptAnglicism> TranscriptAnglicisms { get; set; } = [];
	public virtual ICollection<AnglicismContextWindow> AnglicismContextWindows { get; set; } = [];
}
