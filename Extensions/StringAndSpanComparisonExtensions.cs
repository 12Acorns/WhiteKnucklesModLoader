namespace WhiteKnucklesModLoader.Extensions;

internal static class StringAndSpanComparisonExtensions
{
	public static bool CompareWith(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, StringComparison comparison = StringComparison.OrdinalIgnoreCase) =>
		a.CompareTo(b, comparison) == 0;
	public static bool CompareWith(this string a, string b, StringComparison comparison = StringComparison.OrdinalIgnoreCase) =>
		a.AsSpan().CompareWith(b, comparison);
}
