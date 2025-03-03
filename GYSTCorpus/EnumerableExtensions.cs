using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic;
public static class EnumerableExtensions
{
	public static IEnumerable<(T Value, int Index)> WithIndex<T>(this IEnumerable<T> source)
	{
		int index = 0;
		foreach (var item in source)
			yield return (item, index++);
	}

	public static IEnumerable<T> AsSingleEnumerable<T>(this T item)
	{
		yield return item;
	}
}
