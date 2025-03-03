using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GYSTCorpus.Database;
public class WordPartOfSpeech
{
	public string Word { get; set; } = string.Empty;
	public PartOfSpeech GermanPartOfSpeech { get; set; }
	public PartOfSpeech EnglishPartOfSpeech { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartOfSpeech : int
{
	[JsonStringEnumMemberName("X")]
	Other = 0,
	[JsonStringEnumMemberName("ADJ")]
	Adjective = 1,
	[JsonStringEnumMemberName("ADP")]
	Adposition = 2,
	[JsonStringEnumMemberName("ADV")]
	Adverb = 3,
	[JsonStringEnumMemberName("AUX")]
	Auxiliary = 4,
	[JsonStringEnumMemberName("CCONJ")]
	CoordinatingConjunction = 5,
	[JsonStringEnumMemberName("DET")]
	Determiner = 6,
	[JsonStringEnumMemberName("INTJ")]
	Interjection = 7,
	[JsonStringEnumMemberName("NOUN")]
	Noun = 8,
	[JsonStringEnumMemberName("NUM")]
	Numeral = 9,
	[JsonStringEnumMemberName("PART")]
	Particle = 10,
	[JsonStringEnumMemberName("PRON")]
	Pronoun = 11,
	[JsonStringEnumMemberName("PROPN")]
	ProperNoun = 12,
	[JsonStringEnumMemberName("PUNCT")]
	Punctuation = 13,
	[JsonStringEnumMemberName("SCONJ")]
	SubordinatingConjunction = 14,
	[JsonStringEnumMemberName("SYM")]
	Symbol = 15,
	[JsonStringEnumMemberName("VERB")]
	Verb = 16,
	[JsonStringEnumMemberName("SPACE")]
	Space = 17
}