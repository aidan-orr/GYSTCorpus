using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GYSTCorpus;

/// <summary>
/// A class that wraps an integer and provides methods for atomic operations.
/// </summary>
/// <param name="value">The initial value</param>
public class AtomicInteger(int value) : IEquatable<AtomicInteger>, IEquatable<int>
{
	private int _value = value;

	public int Value => _value;

	public int Increment() => Interlocked.Increment(ref _value);
	public int Decrement() => Interlocked.Decrement(ref _value);

	public int Add(int amount) => Interlocked.Add(ref _value, amount);
	public int Subtract(int amount) => Interlocked.Add(ref _value, -amount);

	public static implicit operator int(AtomicInteger atomicInteger) => atomicInteger.Value;
	public static implicit operator AtomicInteger(int value) => new AtomicInteger(value);

	public override string ToString() => _value.ToString();
	public override bool Equals(object? obj) => obj is AtomicInteger atomicInteger && _value == atomicInteger._value;
	public override int GetHashCode() => _value.GetHashCode();

	public bool Equals(AtomicInteger? other) => other is not null && _value == other._value;
	public bool Equals(int other) => _value == other;

	public static bool operator ==(AtomicInteger left, AtomicInteger right) => left._value == right._value;
	public static bool operator !=(AtomicInteger left, AtomicInteger right) => left._value != right._value;

	public static bool operator <(AtomicInteger left, AtomicInteger right) => left._value < right._value;
	public static bool operator >(AtomicInteger left, AtomicInteger right) => left._value > right._value;

	public static bool operator <=(AtomicInteger left, AtomicInteger right) => left._value <= right._value;
	public static bool operator >=(AtomicInteger left, AtomicInteger right) => left._value >= right._value;
}
