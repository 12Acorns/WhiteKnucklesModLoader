namespace WhiteKnucklesModLoader.Extensions;

internal static class FileExtensions
{
	public static async Task CopyToAsync(this FileInfo file, DirectoryInfo destination, CancellationToken cancellationToken,
		bool overwrite = false, int bufferSize = 4096)
	{
		if(file is null)
		{
			throw new ArgumentNullException(nameof(file), "File cannot be null.");
		}
		if(destination is null)
		{
			throw new ArgumentNullException(nameof(destination), "Destination cannot be null.");
		}
		if(!destination.Exists)
		{
			destination.Create();
		}
		var destFilePath = Path.Combine(destination.FullName, file.Name);
		var overwriteMode = overwrite ? FileMode.Create : FileMode.CreateNew;

		using var sourceStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize,
			FileOptions.Asynchronous | FileOptions.SequentialScan);
		using var destinationStream = new FileStream(destFilePath, overwriteMode, FileAccess.Write, FileShare.None, bufferSize,
			FileOptions.Asynchronous | FileOptions.SequentialScan);

		await sourceStream.CopyToAsync(destinationStream).ConfigureAwait(false);
	}
	public static async Task CopyToAsync(this FileInfo file, DirectoryInfo destination, bool overwrite = false, int bufferSize = 4096) =>
		await CopyToAsync(file, destination, CancellationToken.None, overwrite, bufferSize).ConfigureAwait(false);
}
