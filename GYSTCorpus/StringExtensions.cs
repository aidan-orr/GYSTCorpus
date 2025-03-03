using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus;
public static class StringExtensions
{
	public static string Remove(this string input, params ICollection<char> toRemove)
	{
		StringBuilder sb = new StringBuilder(input.Length);

		foreach (var c in input)
		{
			if (!toRemove.Contains(c))
				sb.Append(c);
		}

		return sb.ToString();
	}

	public static string Replace(this string input, char replacement, params ReadOnlySpan<char> toReplace)
	{
		return string.Create(input.Length, new FastReplaceRecord(input, replacement, toReplace), (Span<char> span, FastReplaceRecord state) =>
		{
			for (int i = 0; i < state.Input.Length; i++)
			{
				if (state.ToReplace.Contains(state.Input[i]))
					span[i] = state.Replacement;
				else
					span[i] = state.Input[i];
			}
		});
	}

	readonly ref struct FastReplaceRecord(string Input, char Replacement, ReadOnlySpan<char> ToReplace)
	{
		public readonly string Input = Input;
		public readonly char Replacement = Replacement;
		public readonly ReadOnlySpan<char> ToReplace = ToReplace;
	}
}
