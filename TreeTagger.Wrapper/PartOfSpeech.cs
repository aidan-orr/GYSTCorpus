using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace TreeTagger.Wrapper;
public enum PartOfSpeech
{
	None = 0,
	ADJA = 1,       // Attributive adjective
	ADJD = 2,       // Predicative or adverbial adjective
	ADV = 3,        // Adverb
	APPR = 4,       // Preposition; circomposition (left part)
	APPRART = 5,    // Preposition with fused article
	APPO = 6,       // Postposition
	APZR = 7,       // Circumposition (right part)
	ART = 8,        // Definite or indefinite article
	CARD = 9,       // Cardinal number
	FM = 10,        // Foreign language material
	ITJ = 11,       // Interjection
	KOUI = 12,      // Subordinating conjunction with "zu" and infinitive
	KOUS = 13,      // Subordinating conjunction with sentence
	KON = 14,       // Coordinating conjunction
	KOKOM = 15,     // Comparative conjunction
	NN = 16,        // Simple noun
	NE = 17,        // Proper noun
	PDS = 18,       // Substitutive demonstrative pronoun
	PDAT = 19,      // Attributive demonstrative pronoun
	PIS = 20,       // Substitutive indefinite pronoun
	PIAT = 21,      // Attributive indefinite pronoun without determiner
	PIDAT = 22,     // Attributive indefinite pronoun with determiner
	PPER = 23,      // irreflexive personal pronoun
	PPOSS = 24,     // substitutive possessive pronoun
	PPOSAT = 25,    // attributive possessive pronoun
	PRELS = 26,     // Substitutive relative pronoun
	PRELAT = 27,    // Attributive relative pronoun
	PRF = 28,       // Reflexive personal pronoun
	PWS = 29,       // Substitutive interrogative pronoun
	PWAT = 30,      // Attributive interrogative pronoun
	PAV = 31,       // Pronominal adverb
	PTKZU = 32,     // "zu" particle before infinitive
	PTKNEG = 33,    // Negation particle
	PTKVZ = 34,     // Separated verb particle
	PTKANT = 35,    // Answering particle
	PTKA = 36,      // Particle before adjective or adverb
	TRUNC = 37,     // First element of a (truncated) compound
	VVFIN = 38,     // Finite full verb
	VVIMP = 39,     // Imperative (full verb)
	VVINF = 40,     // Infinitive (full verb)
	VVIZU = 41,     // Infinitive with incorporated "zu" particle (full verb)
	VVPP = 42,      // Past particle (full verb)
	VAFIN = 43,     // Finite verb (haben, sein, werden)
	VAIMP = 44,     // Imperative (haben, sein, werden)
	VAINF = 45,     // Infinitive (haben, sein, werden)
	VAPP = 46,      // Past participle (haben, sein, werden)
	VMFIN = 47,     // Finite modal verb
	VMINF = 48,     // Infinitive (modal verb)
	VMPP = 49,      // Past participle (modal verb)
	XY = 50,        // Non-word containing special symbols
}

public static class PartOfSpeechHelpers
{
	public static string ToString(this PartOfSpeech pos) => pos switch
	{
		PartOfSpeech.ADJA => "ADJA",
		PartOfSpeech.ADJD => "ADJD",
		PartOfSpeech.ADV => "ADV",
		PartOfSpeech.APPR => "APPR",
		PartOfSpeech.APPRART => "APPRART",
		PartOfSpeech.APPO => "APPO",
		PartOfSpeech.APZR => "APZR",
		PartOfSpeech.ART => "ART",
		PartOfSpeech.CARD => "CARD",
		PartOfSpeech.FM => "FM",
		PartOfSpeech.ITJ => "ITJ",
		PartOfSpeech.KOUI => "KOUI",
		PartOfSpeech.KOUS => "KOUS",
		PartOfSpeech.KON => "KON",
		PartOfSpeech.KOKOM => "KOKOM",
		PartOfSpeech.NN => "NN",
		PartOfSpeech.NE => "NE",
		PartOfSpeech.PDS => "PDS",
		PartOfSpeech.PDAT => "PDAT",
		PartOfSpeech.PIS => "PIS",
		PartOfSpeech.PIAT => "PIAT",
		PartOfSpeech.PIDAT => "PIDAT",
		PartOfSpeech.PPER => "PPER",
		PartOfSpeech.PPOSS => "PPOSS",
		PartOfSpeech.PPOSAT => "PPOSAT",
		PartOfSpeech.PRELS => "PRELS",
		PartOfSpeech.PRELAT => "PRELAT",
		PartOfSpeech.PRF => "PRF",
		PartOfSpeech.PWS => "PWS",
		PartOfSpeech.PWAT => "PWAT",
		PartOfSpeech.PAV => "PAV",
		PartOfSpeech.PTKZU => "PTKZU",
		PartOfSpeech.PTKNEG => "PTKNEG",
		PartOfSpeech.PTKVZ => "PTKVZ",
		PartOfSpeech.PTKANT => "PTKANT",
		PartOfSpeech.PTKA => "PTKA",
		PartOfSpeech.TRUNC => "TRUNC",
		PartOfSpeech.VVFIN => "VVFIN",
		PartOfSpeech.VVIMP => "VVIMP",
		PartOfSpeech.VVINF => "VVINF",
		PartOfSpeech.VVIZU => "VVIZU",
		PartOfSpeech.VVPP => "VVPP",
		PartOfSpeech.VAFIN => "VAFIN",
		PartOfSpeech.VAIMP => "VAIMP",
		PartOfSpeech.VAINF => "VAINF",
		PartOfSpeech.VAPP => "VAPP",
		PartOfSpeech.VMFIN => "VMFIN",
		PartOfSpeech.VMINF => "VMINF",
		PartOfSpeech.VMPP => "VMPP",
		PartOfSpeech.XY => "XY",
		_ => "UNKNOWN"
	};

	public static bool TryParse(ReadOnlySpan<char> value, out PartOfSpeech result)
	{
		(bool Success, PartOfSpeech PartOfSpeech) v = value switch
		{
			"ADJA" => (true, PartOfSpeech.ADJA),
			"ADJD" => (true, PartOfSpeech.ADJD),
			"ADV" => (true, PartOfSpeech.ADV),
			"APPR" => (true, PartOfSpeech.APPR),
			"APPRART" => (true, PartOfSpeech.APPRART),
			"APPO" => (true, PartOfSpeech.APPO),
			"APZR" => (true, PartOfSpeech.APZR),
			"ART" => (true, PartOfSpeech.ART),
			"CARD" => (true, PartOfSpeech.CARD),
			"FM" => (true, PartOfSpeech.FM),
			"ITJ" => (true, PartOfSpeech.ITJ),
			"KOUI" => (true, PartOfSpeech.KOUI),
			"KOUS" => (true, PartOfSpeech.KOUS),
			"KON" => (true, PartOfSpeech.KON),
			"KOKOM" => (true, PartOfSpeech.KOKOM),
			"NN" => (true, PartOfSpeech.NN),
			"NE" => (true, PartOfSpeech.NE),
			"PDS" => (true, PartOfSpeech.PDS),
			"PDAT" => (true, PartOfSpeech.PDAT),
			"PIS" => (true, PartOfSpeech.PIS),
			"PIAT" => (true, PartOfSpeech.PIAT),
			"PIDAT" => (true, PartOfSpeech.PIDAT),
			"PPER" => (true, PartOfSpeech.PPER),
			"PPOSS" => (true, PartOfSpeech.PPOSS),
			"PPOSAT" => (true, PartOfSpeech.PPOSAT),
			"PRELS" => (true, PartOfSpeech.PRELS),
			"PRELAT" => (true, PartOfSpeech.PRELAT),
			"PRF" => (true, PartOfSpeech.PRF),
			"PWS" => (true, PartOfSpeech.PWS),
			"PWAT" => (true, PartOfSpeech.PWAT),
			"PAV" => (true, PartOfSpeech.PAV),
			"PTKZU" => (true, PartOfSpeech.PTKZU),
			"PTKNEG" => (true, PartOfSpeech.PTKNEG),
			"PTKVZ" => (true, PartOfSpeech.PTKVZ),
			"PTKANT" => (true, PartOfSpeech.PTKANT),
			"PTKA" => (true, PartOfSpeech.PTKA),
			"TRUNC" => (true, PartOfSpeech.TRUNC),
			"VVFIN" => (true, PartOfSpeech.VVFIN),
			"VVIMP" => (true, PartOfSpeech.VVIMP),
			"VVINF" => (true, PartOfSpeech.VVINF),
			"VVIZU" => (true, PartOfSpeech.VVIZU),
			"VVPP" => (true, PartOfSpeech.VVPP),
			"VAFIN" => (true, PartOfSpeech.VAFIN),
			"VAIMP" => (true, PartOfSpeech.VAIMP),
			"VAINF" => (true, PartOfSpeech.VAINF),
			"VAPP" => (true, PartOfSpeech.VAPP),
			"VMFIN" => (true, PartOfSpeech.VMFIN),
			"VMINF" => (true, PartOfSpeech.VMINF),
			"VMPP" => (true, PartOfSpeech.VMPP),
			"XY" => (true, PartOfSpeech.XY),
			_ => (false, PartOfSpeech.None)
		};

		result = v.PartOfSpeech;
		return v.Success;
	}

	public static PartOfSpeech Parse(string value)
	{
		if (TryParse(value, out PartOfSpeech result))
			return result;

		throw new ArgumentException($"Invalid part of speech: {value}", nameof(value));
	}

	public static PartOfSpeech Parse(ReadOnlySpan<char> value)
	{
		if (TryParse(value, out PartOfSpeech result))
			return result;
		throw new ArgumentException($"Invalid part of speech: {value.ToString()}", nameof(value));
	}

	public static FrozenSet<PartOfSpeech> AdjectivePartsOfSpeech { get; } = [PartOfSpeech.ADJA, PartOfSpeech.ADJD];
	public static FrozenSet<PartOfSpeech> AdverbPartsOfSpeech { get; } = [PartOfSpeech.ADV];
	public static FrozenSet<PartOfSpeech> NounPartsOfSpeech { get; } = [PartOfSpeech.NN];
	public static FrozenSet<PartOfSpeech> VerbPartsOfSpeech { get; } = [PartOfSpeech.VVFIN, PartOfSpeech.VVIMP, PartOfSpeech.VVINF, PartOfSpeech.VVIZU, PartOfSpeech.VVPP, PartOfSpeech.VAFIN, PartOfSpeech.VAIMP, PartOfSpeech.VAINF, PartOfSpeech.VAPP, PartOfSpeech.VMFIN, PartOfSpeech.VMINF, PartOfSpeech.VMPP];

	public static ICollection<PartOfSpeech> TargetPartsOfSpeech { get; } = [.. AdjectivePartsOfSpeech, .. AdverbPartsOfSpeech, .. NounPartsOfSpeech, .. VerbPartsOfSpeech];

	public static bool IsAdjective(this PartOfSpeech pos) => AdjectivePartsOfSpeech.Contains(pos);
	public static bool IsAdverb(this PartOfSpeech pos) => AdverbPartsOfSpeech.Contains(pos);
	public static bool IsNoun(this PartOfSpeech pos) => NounPartsOfSpeech.Contains(pos);
	public static bool IsVerb(this PartOfSpeech pos) => VerbPartsOfSpeech.Contains(pos);

	public static bool IsTargetPartOfSpeech(this PartOfSpeech pos) => TargetPartsOfSpeech.Contains(pos);

	public static string ToCategory(this PartOfSpeech pos)
	{
		if (pos.IsAdjective())
			return "ADJ";
		if (pos.IsAdverb())
			return "ADV";
		if (pos.IsNoun())
			return "NOUN";
		if (pos.IsVerb())
			return "VERB";
		return pos.ToString();
	}

	public const int CategoryCount = 4;
	public static string[] OutputCategories { get; } = ["ALL", "ADJ", "ADV", "NOUN", "VERB", "OTHER"];
}
