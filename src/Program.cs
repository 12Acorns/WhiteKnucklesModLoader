
using static WhiteKnucklesModLoader.Utility.ConsoleUtility;
using WhiteKnucklesModLoader.Extensions;
using WhiteKnucklesModLoader.Utility;

// TODO: Save only new and changed files between vanilla and modded profiles
// Everything is implemented but hash of each file is identical
// Therefore byte content is identical
// Therefore the files would be identical
// But they are not as when loading (for reference this is the animal paw mod) the mod the game has the paw sprites instead
// Idk how to check for this :\

const string CREDITS = 
"This tool takes inspiration from Bepswitch, created by proudunicorn. " +
"Find at: \x1b[4m\x1b[36mhttps://www.nexusmods.com/valheim/mods/1281\x1b[0m";
const string PREREQUISITES =
"""
In order for this tool to work please make sure you have done the following:
1) Installed BepinEx and is in your White Knuckles directory. For more info see the White Knuckles discord server and go to modding forum.
2) Make sure you have launched White Knuckles and then quit White Knuckles so BepinEx can generate the necessary files.
3) When launching the WK Mod Loader ensure you have no mods installed. This tool saves the required files on first startup to restore the game to vanilla settings.
4) Further on 3, if you have launched the tool already modded then go to the 'WKModLoader_ModCache' folder and delete Vanilla. Use steam's verify file integrity and then relaunch the tool after following 3.
""";
const string HELPDETAILS =
"""
First Usage:
1) Follow the prerequisites above.
2) Launch the tool and select the White Knuckles install location.
3) Close the tool
4) Install your desired mods into the BepInEx plugins folder, patchers folder, and White Knuckle_Data folder.
5) Launch White Knuckles and quit.
6) Launch the tool again and select the White Knuckles install location.
7) Save the profile and give it a name
8) Play WK as usual
Later Usage:
If you wish to play vanilla, use option 1, else if you wish to return to modded use 2
Use Load profile to load a profile you have saved.
If you wish to revert your game to vanilla settings, use options 4 and type Vanilla to go back to a vanilla profile.
FYI: Using option 1 will not revert asset changes. To go to a fully vanilla state please load the Vanilla profile.
""";

const string VANILLAPROFILENAME = "Vanilla";
const string OPTIONS =
"""
Select what you wish to do:
1) Start Vanilla
2) Start Modded
3) Save Profile
4) Load Profile
5) Exit
""";

Console.Title = "White Knuckles Mod Loader";

ConsoleStateManager.Initialize();

Console.WriteLine(CREDITS);
Console.WriteLine(PREREQUISITES);
Console.WriteLine(HELPDETAILS);
Console.WriteLine();

if(!Directory.Exists(PathManager.WKInstallLocation.FullName))
{
	Console.WriteLine("Directory does not exist. Exiting.");
	return;
}
if(PathManager.LegacyCacheExists)
{
	Console.WriteLine("Legacy mod cache found. Migration of profile will occour.\nPress enter to start...");
	Console.ReadKey();
	var legacyCache = PathManager.LegacyCacheLocation;
	foreach(var profile in legacyCache.EnumerateDirectories())
	{
		PathManager.MigrateProfile(profile.Name);
	}
	legacyCache.Delete(true);
}

var vanillaProfile = CreateVanillaProfileOrIgnore();
var option = PromptAndInput(OPTIONS);
switch(option[0])
{
	case '1':
		PathManager.SetDoorStopState(false);
		Console.Write("Vanilla mode enabled. Press any key to exit...");
		Console.ReadKey();
		break;
	case '2':
		PathManager.SetDoorStopState(true);
		Console.Write("Modded mode enabled. Press any key to exit...");
		Console.ReadKey();
		break;
	case '3':
		var profileName = PromptAndInput("What would you like to name the profile?");
		if(profileName.CompareWith("Vanilla"))
		{
			Console.WriteLine("Cannot save profile with name 'Vanilla'. Please choose a different name.");
			goto case '3';
		}
		if(Profile.Exists(profileName))
		{
			Console.Write($"Are you sure you want to overwrite the data for '{profileName}'? Y/N\n>");
			var key = Console.ReadKey().Key;
			if(key is ConsoleKey.Y)
			{
				Console.WriteLine("Overwriting profile");
			}
			else if(key is ConsoleKey.N)
			{
				goto case '3';
			}
			else
			{
				Console.WriteLine("Unrecognized input. Defaulting to N");
				goto case '3';
			}
		}
		Console.WriteLine("Saving profile, this may take a while and the console may freeze. This is expected behaviour.");
		var profile = new Profile(profileName);
		profile.ClearProfileData();
		profile.SaveBepinExDataToProfile();
		profile.SaveDataIntoProfile(PathManager.WKDataLocation);
		Console.WriteLine("Profile Saved.\nPress any key to exit...");
		Console.ReadKey();
		break;
	case '4':
		var profiles = Directory.EnumerateDirectories(PathManager.ProfileRoot.FullName).Select(x =>
		{
			var dirName = Path.GetFileName(Path.TrimEndingDirectorySeparator(x.AsSpan()));
			return string.Create(dirName.Length + 2, dirName, (buffer, val) =>
			{
				buffer[0] = '\n';
				buffer[1] = '-';
				val.CopyTo(buffer[2..]);
			});
		});
		var profileSelected = PromptAndInput($"Profiles available:{string.Concat<string>(profiles)}");
		if(!Profile.Exists(profileSelected))
		{
			Console.WriteLine("Profile does not exist. Enter a valid profile.");
			goto case '4';
		}
		Console.WriteLine("Loading profile, this may take a while and the console may freeze. This is expected behaviour.");
		profile = new Profile(profileSelected);
		profile.LoadProfileBepinExDataToBepinExFolder();
		profile.LoadProfileWKDataToWKDataFolder();
		Console.Write("Profile loaded succesfully. Press any key to exit...");
		Console.Read();
		break;
	case '5':
		Environment.Exit(0);
		break;
}

static Profile CreateVanillaProfileOrIgnore()
{
	var vanillaProfile = new Profile(VANILLAPROFILENAME);
	if(PathManager.ProfileRoot.EnumerateDirectories().Any(x => x.Name is VANILLAPROFILENAME))
	{
		return vanillaProfile;
	}
	vanillaProfile.SaveDataIntoProfile(PathManager.WKDataLocation);
	vanillaProfile.SaveBepinExDataToProfile();
	return vanillaProfile;
}