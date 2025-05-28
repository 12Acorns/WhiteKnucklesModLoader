internal sealed class FileContentComparer : IEqualityComparer<FileInfo>
{
	public bool Equals(FileInfo? f1, FileInfo? f2)
	{
		if(f1 is null || f2 is null)
		{
			return false;
		}

		// Determine if the same file was referenced two times.
		if(f1 == f2)
		{
			// Return true to indicate that the files are the same.
			return true;
		}

		// Open the two files.
		using var fs1 = new FileStream(f1.FullName, FileMode.Open);
		using var fs2 = new FileStream(f2.FullName, FileMode.Open);

		// Check the file sizes. If they are not the same, the files
		// are not the same.
		if(fs1.Length != fs2.Length)
		{
			return false;
		}

		int file1byte;
		int file2byte;
		do
		{
			file1byte = fs1.ReadByte();
			file2byte = fs2.ReadByte();
		}
		while((file1byte == file2byte) && (file1byte != -1));

		// Return the success of the comparison. "file1byte" is
		// equal to "file2byte" at this point only if the files are
		// the same.
		return (file1byte - file2byte) == 0;
	}

	public int GetHashCode(FileInfo fi)
	{
		return $"{fi.Name}{fi.Length}".GetHashCode();
	}

	public static IEnumerable<FileInfo> NewFiles(DirectoryInfo d1, DirectoryInfo d2)
	{
		if(d1 is null || d2 is null)
		{
			return [];
		}
		var comparer = new FileContentComparer();
		var files1 = d1.EnumerateFiles("*", SearchOption.AllDirectories);
		var files2 = d2.EnumerateFiles("*", SearchOption.AllDirectories);
		return files2.Except(files1, comparer);
	}
	public static IEnumerable<FileInfo> DeletedFiles(DirectoryInfo d1, DirectoryInfo d2)
	{
		if(d1 is null || d2 is null)
		{
			return [];
		}
		var comparer = new FileContentComparer();
		var files1 = d1.EnumerateFiles("*", SearchOption.AllDirectories);
		var files2 = d2.EnumerateFiles("*", SearchOption.AllDirectories);
		return files1.Except(files2, comparer);
	}
	public static IEnumerable<FileInfo> ChangedFiles(DirectoryInfo d1, DirectoryInfo d2)
	{
		if(d1 is null || d2 is null)
		{
			return [];
		}
		var comparer = new FileContentComparer();
		var files1 = d1.EnumerateFiles("*", SearchOption.AllDirectories);
		var files2 = d2.EnumerateFiles("*", SearchOption.AllDirectories);
		return files1.Intersect(files2, comparer).Where(f => !comparer.Equals(f, files2.FirstOrDefault(x => x.Name == f.Name)));
	}
}