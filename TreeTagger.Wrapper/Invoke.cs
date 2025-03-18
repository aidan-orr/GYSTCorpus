using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TreeTagger.Wrapper;

public static class Invoke
{
	/// <summary>
	/// Invokes the TreeTagger executable with the given input text and additional parameters.
	/// </summary>
	/// <param name="inputText">The text to run through the TreeTagger. One word per line expected.</param>
	/// <param name="additionalParms">A list of additional parameters to send to TreeTagger</param>
	/// <returns>The output from TreeTagger</returns>
	/// <exception cref="Exception">An invalid exit code was returned.</exception>
	public static string Tagger(string inputText, params IEnumerable<string> additionalParms)
	{
		string inputFile = Path.GetTempFileName();
		string outputFile = Path.GetTempFileName();

		File.WriteAllText(inputFile, inputText);

		IEnumerable<string> parameters = additionalParms.Prepend("" /*"-quiet"*/).Append("german.par").Append(inputFile).Append(outputFile);

		using var proc = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "tree-tagger.exe",
				Arguments = string.Join(" ", parameters),
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardInput = false,

			}
		};

		proc.Start();
		string stdout = proc.StandardOutput.ReadToEnd();
		string stderr = proc.StandardError.ReadToEnd();
		proc.WaitForExit();

		if (proc.ExitCode != 0)
		{
			throw new Exception($"TreeTagger exited with code {proc.ExitCode}\nError: {stderr}");
		}

		string outputText = File.ReadAllText(outputFile);

		File.Delete(inputFile);
		File.Delete(outputFile);

		return outputText;
	}

	public static IEnumerable<(string, PartOfSpeech)> TagPartsOfSpeech(string inputText, CultureInfo? culture = null, params IEnumerable<string> additionalParams)
	{
		string results = Tagger(WordsByLine(inputText), additionalParams.Append("-token").Append("-lemma"));

		char[] buffer = ArrayPool<char>.Shared.Rent(results.Length);

		foreach ((int start, int length) in SplitByLines(results))
		{
			if (length <= 0)
				continue;

			ReadOnlySpan<char> line = results.AsSpan(start, length);
			TaggerLine taggerLine = ParseTaggerLine(line);

			int lowerLength = line.Slice(taggerLine.Word.Start, taggerLine.Word.Length).ToLower(buffer, culture);

			string word = string.Intern(buffer.AsSpan(0, lowerLength).ToString());
			ReadOnlySpan<char> tag = line.Slice(taggerLine.PartOfSpeech.Start, taggerLine.PartOfSpeech.Length);

			_ = PartOfSpeechHelpers.TryParse(tag, out PartOfSpeech pos);

			yield return (word, pos);
		}

		ArrayPool<char>.Shared.Return(buffer);
	}

	private static string WordsByLine(ReadOnlySpan<char> inputText)
	{
		StringBuilder sb = new(inputText.Length);

		int wordStart = 0;
		for (int index = 0; index < inputText.Length; index++)
		{
			if (char.IsWhiteSpace(inputText[index]))
			{
				if (wordStart < index)
					sb.Append(inputText.Slice(wordStart, index - wordStart)).Append('\n');

				wordStart = index + 1;
			}
		}

		if (wordStart < inputText.Length)
			sb.Append(inputText.Slice(wordStart));

		if (sb.Length <= 0)
			return string.Empty;

		if (sb[^1] == '\n')
			sb.Length--;

		return sb.ToString();
	}

	private static IEnumerable<(int StartIndex, int Length)> SplitByLines(string text)
	{
		int start = 0;
		for (int index = 0; index < text.Length; index++)
		{
			if (text[index] == '\n')
			{
				if (text[index - 1] == '\r')
					yield return (start, index - start - 1);
				else
					yield return (start, index - start);
				
				start = index + 1;
			}
		}

		if (start < text.Length)
			yield return (start, text.Length - start);
	}

	private static TaggerLine ParseTaggerLine(ReadOnlySpan<char> line)
	{
		Span<(int Start, int Length)> ranges = stackalloc (int Start, int Length)[3];

		int rangeIndex = 0;
		int start = 0;
		for (int index = 0; rangeIndex < ranges.Length && index < line.Length; index++)
		{
			if (char.IsWhiteSpace(line[index]))
			{
				if (start < index)
					ranges[rangeIndex++] = (start, index - start);

				start = index + 1;
			}
		}

		if (rangeIndex < ranges.Length && start < line.Length)
		{
			ranges[rangeIndex] = (start, line.Length - start - 1);
		}

		return new TaggerLine(ranges[0], ranges[1], ranges[2]);
	}

	private readonly record struct TaggerLine((int Start, int Length) Word, (int Start, int Length) PartOfSpeech, (int Start, int Length) Lemma);
}