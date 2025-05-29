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

	public static void LoadProfileWKDataToWKDataFolder(this Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		var profileWKData = profile.ProfileLocation.CreateSubdirectory(PathManager.WKDATADIRNAME);
		PathManager.WKDataLocation.Empty();
		PathManager.CopyAll(profileWKData, PathManager.WKDataLocation);
	}
	public static void LoadProfileBepinExDataToBepinExFolder(this Profile profile)
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
		PathManager.CopyAll(toPluginsRoot, _bepinExPluginsLocation);
		PathManager.CopyAll(toPatchersRoot, _bepinExPatchersLocation);
		PathManager.CopyAll(toConfigRoot, _bepinExConfigLocation);
	}
	public static void SaveBepinExDataToProfile(this Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		GetOrCreateBepinExFolders(profile.ProfileLocation, out var toPluginsRoot, out var toPatchersRoot, out var toConfigRoot);
		PathManager.CopyAll(_bepinExPluginsLocation, toPluginsRoot);
		PathManager.CopyAll(_bepinExPatchersLocation, toPatchersRoot);
		PathManager.CopyAll(_bepinExConfigLocation, toConfigRoot);
	}

	private static void GetOrCreateBepinExFolders(DirectoryInfo root, out DirectoryInfo pluginsRoot, out DirectoryInfo patchersRoot, out DirectoryInfo configRoot)
	{
		pluginsRoot = root.CreateSubdirectory(PathManager.BEPINEXPLUGINSNAME);
		patchersRoot = root.CreateSubdirectory(PathManager.BEPINEXPATCHERSNAME);
		configRoot = root.CreateSubdirectory(PathManager.BEPINEXCONFIGNAME);
	}
}
