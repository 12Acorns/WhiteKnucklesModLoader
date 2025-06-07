using WhiteKnucklesModLoader.Utility;

namespace WhiteKnucklesModLoader.Extensions;

internal static class ProfileExtensions
{
	static ProfileExtensions()
	{
		PathManager.GetBepinEx_Config_Patchers_Plugin_Locations(out _bepinExConfigLocation, out _bepinExPatchersLocation, out _bepinExPluginsLocation);
	}

	private static readonly DirectoryInfo _bepinExPluginsLocation;
	private static readonly DirectoryInfo _bepinExPatchersLocation;
	private static readonly DirectoryInfo _bepinExConfigLocation;

	public static async Task LoadProfileWKDataToWKDataFolder(this Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		var profileWKData = profile.ProfileLocation.CreateSubdirectory(PathManager.WKDATADIRNAME);
		PathManager.WKDataLocation.Empty();
		await PathManager.CopyAllAsync(profileWKData, PathManager.WKDataLocation).ConfigureAwait(false);
	}
	public static async Task LoadProfileBepinExDataToBepinExFolder(this Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		// Clear bepinex data
		_bepinExPluginsLocation.Empty();
		_bepinExPatchersLocation.Empty();
		_bepinExConfigLocation.Empty();
		// Get profile bepinex data
		GetOrCreateBepinExFolders(profile.ProfileLocation, out var toPluginsRoot, out var toPatchersRoot, out var toConfigRoot);
		// Copy profile data to bepinex folders
		var t1 = PathManager.CopyAllAsync(toPluginsRoot, _bepinExPluginsLocation);
		var t2 = PathManager.CopyAllAsync(toPatchersRoot, _bepinExPatchersLocation);
		var t3 = PathManager.CopyAllAsync(toConfigRoot, _bepinExConfigLocation);
		await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
	}
	public static async Task SaveBepinExDataToProfile(this Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		GetOrCreateBepinExFolders(profile.ProfileLocation, out var toPluginsRoot, out var toPatchersRoot, out var toConfigRoot);
		var t1 = PathManager.CopyAllAsync(_bepinExPluginsLocation, toPluginsRoot);
		var t2 = PathManager.CopyAllAsync(_bepinExPatchersLocation, toPatchersRoot);
		var t3 = PathManager.CopyAllAsync(_bepinExConfigLocation, toConfigRoot);
		await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
	}

	private static void GetOrCreateBepinExFolders(DirectoryInfo root, out DirectoryInfo pluginsRoot, out DirectoryInfo patchersRoot, out DirectoryInfo configRoot)
	{
		pluginsRoot = root.CreateSubdirectory(PathManager.BEPINEXPLUGINSNAME);
		patchersRoot = root.CreateSubdirectory(PathManager.BEPINEXPATCHERSNAME);
		configRoot = root.CreateSubdirectory(PathManager.BEPINEXCONFIGNAME);
	}
}
