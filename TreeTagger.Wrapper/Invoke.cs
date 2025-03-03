using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TreeTagger.Wrapper;

public static class Invoke
{
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
}