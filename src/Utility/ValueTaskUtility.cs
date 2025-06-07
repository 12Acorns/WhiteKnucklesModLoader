namespace WhiteKnucklesModLoader.Utility;

internal static class ValueTaskUtility
{
	public static async ValueTask WhenAll(params ValueTask[] tasks)
	{
		ArgumentNullException.ThrowIfNull(tasks);
		if(tasks.Length == 0)
			return;

		for(var i = 0; i < tasks.Length; i++)
			await tasks[i].ConfigureAwait(false);
	}
}
