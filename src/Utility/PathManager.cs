using WhiteKnucklesModLoader.Extensions;
using ZLinq;

namespace WhiteKnucklesModLoader.Utility;

internal static class PathManager
{
	public const string BEPINEXPATCHERSNAME = "patchers";
	public const string BEPINEXPLUGINSNAME = "plugins";
	public const string BEPINEXCONFIGNAME = "config";
	public const string WHITEKNUCKLESMODLOADERNAME = "WKModLoader";
	public const string DOORSTOPFILENAME = "doorstop_config.ini";
	public const string MODCACHENAME = "WKModLoader_ModCache";
	public const string WKDATADIRNAME = "White Knuckle_Data";
	public const string WKINSTALLLDATA = "loaderdata.txt";
	public const string ENABLEDFALSE = "enabled = false";
	public const string ENABLEDTRUE = "enabled = true";
	public const string PROFILEROOTNAME = "Profiles";
	public const string BEPINEXDIRNAME = "BepInEx";

	static PathManager()
	{
		var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
		LocalDataLocation = Directory.CreateDirectory(Path.Combine(localData, WHITEKNUCKLESMODLOADERNAME));
		WKInstallLocation = GetWKInstallLocation();

		_legacyModCacheLocation = Path.Combine(WKInstallLocation.FullName, MODCACHENAME);
		LegacyCacheExists = Directory.Exists(_legacyModCacheLocation);

		ProfileRoot = LocalDataLocation.CreateSubdirectory(PROFILEROOTNAME);
		var bepinExLocationTmp = WKInstallLocation.EnumerateDirectories().FirstOrDefault(x => x.Name is BEPINEXDIRNAME);
		if(bepinExLocationTmp is null)
		{
			Console.WriteLine($"BepInEx directory not found in '{WKInstallLocation.FullName}'. Exiting...");
			Console.Read();
			Environment.Exit(1);
		}
		BepinExLocation = bepinExLocationTmp;
		WKDataLocation = new DirectoryInfo(Path.Combine(WKInstallLocation.FullName, WKDATADIRNAME));
	}

	private static readonly string _legacyModCacheLocation;

	public static bool LegacyCacheExists { get; }
	public static DirectoryInfo LegacyCacheLocation => new(_legacyModCacheLocation);
	public static DirectoryInfo BepinExLocation { get; }
	public static DirectoryInfo WKInstallLocation { get; }
	public static DirectoryInfo LocalDataLocation { get; }
	public static DirectoryInfo ProfileRoot { get; }
	public static DirectoryInfo WKDataLocation { get; }

	public static void GetBepinEx_Config_Patchers_Plugin_Locations(out DirectoryInfo configLocation, out DirectoryInfo patchersLocation, out DirectoryInfo pluginsLocation)
	{
		configLocation = BepinExLocation.CreateSubdirectory("config");
		patchersLocation = BepinExLocation.CreateSubdirectory("patchers");
		pluginsLocation = BepinExLocation.CreateSubdirectory("plugins");
	}
	public static async Task SetDoorStopStateAsync(bool state)
	{
		var doorStopPath = Path.Combine(WKInstallLocation.FullName, DOORSTOPFILENAME);
		var contentTask = File.ReadAllTextAsync(doorStopPath).ConfigureAwait(false);
		var (doorStopStateFrom, doorStopStateTo) = state ? (ENABLEDFALSE, ENABLEDTRUE) : (ENABLEDTRUE, ENABLEDFALSE);
		var content = (await contentTask).Replace(doorStopStateFrom, doorStopStateTo);
		await File.WriteAllTextAsync(doorStopPath, content).ConfigureAwait(false);
	}
	/// <summary>
	/// Copies all files and directories from the source directory to the target directory recursively.
	/// <para></para>
	/// The source directory will spill its contents into target, so make sure to create a subdirectory in target if you want to keep the source directory structure.
	/// </summary>
	public static async Task CopyAllAsync(DirectoryInfo source, DirectoryInfo target)
	{
		if(source == null || target == null)
		{
			throw new ArgumentNullException(source == null ? nameof(source) : nameof(target));
		}
		if(Path.GetFullPath(source.FullName).AsSpan().TrimEnd(Path.DirectorySeparatorChar)
			.CompareWith(Path.GetFullPath(target.FullName).AsSpan().TrimEnd(Path.DirectorySeparatorChar)))
		{
			throw new InvalidOperationException("Source and target directories cannot be the same.");
		}
		Directory.CreateDirectory(target.FullName);
		foreach(var fi in source.EnumerateFiles())
		{
			await fi.CopyToAsync(target, true).ConfigureAwait(false);
		}
		foreach(var diSourceSubDir in source.EnumerateDirectories())
		{
			var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
			await CopyAllAsync(diSourceSubDir, nextTargetSubDir).ConfigureAwait(false);
		}
	}
	/// <summary>
	/// Attempts to migrate old profiles to new profile location. If no such profile is found in the legacy location, this method does nothing.
	/// </summary>
	public static void MigrateProfile(string profileName)
	{
		if(!LegacyCacheExists || string.IsNullOrWhiteSpace(profileName))
		{
			return;
		}
		var legacyProfilePath = Path.Combine(_legacyModCacheLocation, profileName);
		if (!Directory.Exists(legacyProfilePath))
		{
			return;
		}
		var newProfilePath = Path.Combine(ProfileRoot.FullName, profileName);
		if(Directory.Exists(newProfilePath))
		{
			Directory.Delete(newProfilePath, true);
		}
		Directory.Move(legacyProfilePath, newProfilePath);
	}

	private static DirectoryInfo GetWKInstallLocation()
	{
		var wkInstallPathData = Path.Join(LocalDataLocation.FullName, WKINSTALLLDATA);
		string GetAndSavePath()
		{
			var path = ConsoleUtility.PromptAndInput("Please enter the path to your White Knuckles install location:").Trim();
			File.WriteAllText(wkInstallPathData, path);
			Console.WriteLine($"Saved White Knuckles install location '{path}' at '{wkInstallPathData}'");
			return path;
		}
		var whiteKnucklesPath = File.Exists(wkInstallPathData)
			? File.ReadAllText(wkInstallPathData).Trim()
			: GetAndSavePath();
		return new DirectoryInfo(whiteKnucklesPath);
	}
}
