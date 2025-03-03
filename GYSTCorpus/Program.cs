using GYSTCorpus;

string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new InvalidOperationException("CONNECTION_STRING not set");

Console.ResetColor();

TranscriptAnalyzer.StripWords(connectionString, "[Musik]", "[Gelächter]", "[Applaus]");

TranscriptAnalyzer.FastAnalyze(connectionString);