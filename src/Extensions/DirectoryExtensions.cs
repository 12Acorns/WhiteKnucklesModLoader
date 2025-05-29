namespace WhiteKnucklesModLoader.Extensions;

internal static class DirectoryExtensions
{
	public static void Empty(this DirectoryInfo dir)
	{
		if (dir is null)
		{
			throw new ArgumentNullException(nameof(dir), "Directory cannot be null.");
		}
		if (!dir.Exists)
		{
			return;
		}
		foreach (var file in dir.EnumerateFiles())
		{
			file.Delete();
		}
		foreach (var subDir in dir.EnumerateDirectories())
		{
			subDir.Delete(true);
		}
	}
}
