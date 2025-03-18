using GYSTCorpus;
using System.Globalization;

Console.ResetColor();

string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new InvalidOperationException("CONNECTION_STRING not set");
string outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? throw new InvalidOperationException("OUTPUT_DIR not set");

//Console.Write(TranscriptAnalyzer.CountWords(connectionString));

//TranscriptAnalyzer.AnalyzeWithPartOfSpeech(connectionString, 16);
TranscriptAnalyzer.CalculateEntropies(connectionString, 2014, 2024, outputDir);
//TranscriptAnalyzer.FastAnalyze(connectionString);