using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class AnglicismContextWindow
{
	public string Anglicism { get; set; } = string.Empty;
	public int Year { get; set; }
	public int Category { get; set; }
	public string ContextWord { get; set; } = string.Empty;
	public TreeTagger.Wrapper.PartOfSpeech GermanPos { get; set; }

	public int Count { get; set; }

	public virtual Anglicism AnglicismNavigation { get; set; } = default!;
}

public readonly record struct AnglicismContextKey(string Anglicism, int Year, int Category, string ContextWord, TreeTagger.Wrapper.PartOfSpeech GermanPos)
{
	private const string separator = "||";
	public override string ToString() => $"{Anglicism}{separator}{Year}{separator}{Category}{separator}{ContextWord}{separator}{GermanPos}";

	public static AnglicismContextKey Parse(string key)
	{
		Span<Range> ranges = stackalloc Range[4];
		var span = (ReadOnlySpan<char>)key;
		
		int count = span.Split(ranges, separator);

		if (count < 5)
			throw new ArgumentException("Invalid key format", nameof(key));

		return new AnglicismContextKey(
			span[ranges[0]].ToString(),
			int.Parse(span[ranges[1]]),
			int.Parse(span[ranges[2]]),
			span[ranges[3]].ToString(),
			Enum.Parse<TreeTagger.Wrapper.PartOfSpeech>(span[ranges[4]].ToString())
		);
	}
}