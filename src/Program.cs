
using System.Diagnostics.CodeAnalysis;
using WhiteKnucklesModLoader.Extensions;

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
const string WKLOCATIONPROMPT = "White Knuckles Install Location:";
const string ENABLEDTRUE = "enabled = true";
const string ENABLEDFALSE = "enabled = false";
const string DOORSTOPFILENAME = "doorstop_config.ini";
const string BEPINEXCONFIGNAME = "config";
const string BEPINEXPATCHERSNAME = "patchers";
const string BEPINEXPLUGINSNAME = "plugins";
const string BEPINEXDIRNAME = "BepInEx";
const string WKDATADIRNAME = "White Knuckle_Data";
const string MODCACHENAME = "WKModLoader_ModCache";
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

var localLowRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
var wkModLoaderRoot = Path.Combine(localLowRoot, "WKModLoader");
Directory.CreateDirectory(wkModLoaderRoot);
var wkModLoaderDataPath = Path.Combine(wkModLoaderRoot, "loaderdata.txt");
string whiteKnucklesPath;
if(!File.Exists(wkModLoaderDataPath))
{
	whiteKnucklesPath = PromptAndInput(WKLOCATIONPROMPT);
	File.WriteAllText(wkModLoaderDataPath, whiteKnucklesPath);
}
else
{
	whiteKnucklesPath = File.ReadAllText(wkModLoaderDataPath).Trim();
	if(string.IsNullOrWhiteSpace(whiteKnucklesPath))
	{
		whiteKnucklesPath = PromptAndInput(WKLOCATIONPROMPT);
		File.WriteAllText(wkModLoaderDataPath, whiteKnucklesPath);
	}
	else
	{
		Console.WriteLine($"Using saved White Knuckles install location: {whiteKnucklesPath}");
	}
}


if(!Directory.Exists(whiteKnucklesPath))
{
	Console.WriteLine("Directory does not exist. Exiting.");
	return;
}
GetRequiredFolderLocations(whiteKnucklesPath, out var whiteKnucklesDirRoot, out var bepinExRoot, out var modCacheRoot, out var wkDataDirRoot);
CreateVanillaProfileOrIgnore(bepinExRoot, wkDataDirRoot, modCacheRoot);

var option = PromptAndInput(OPTIONS);

switch(option[0])
{
	case '1':
		var path = Path.Combine(whiteKnucklesDirRoot.FullName, DOORSTOPFILENAME);
		var content = File.ReadAllText(path);
		content = content.Replace(ENABLEDTRUE, ENABLEDFALSE);
		File.WriteAllText(path, content);
		Console.Write("Vanilla mode enabled. Press any key to exit...");
		Console.ReadKey();
		break;
	case '2':
		path = Path.Combine(whiteKnucklesDirRoot.FullName, DOORSTOPFILENAME);
		content = File.ReadAllText(path);
		content = content.Replace(ENABLEDFALSE, ENABLEDTRUE);
		File.WriteAllText(path, content);
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
		var profileRoot = Directory.CreateDirectory(Path.Combine(modCacheRoot.FullName, profileName));
		var profileDataRoot = profileRoot.CreateSubdirectory(WKDATADIRNAME);
		var vanillaDataRoot = new DirectoryInfo(Path.Combine(modCacheRoot.FullName, "Vanilla", WKDATADIRNAME));
		CopyProfile(bepinExRoot, profileRoot);
		//var changedFiles = FileContentComparer.ChangedFiles(wkDataDirRoot, vanillaDataRoot);
		//var createdFiles = FileContentComparer.NewFiles(wkDataDirRoot, vanillaDataRoot);
		CopyAll(wkDataDirRoot, profileDataRoot);
		Console.WriteLine("Profile Saved.\nPress any key to exit...");
		Console.ReadKey();
		break;
	case '4':
		var profiles = Directory.EnumerateDirectories(modCacheRoot.FullName).Select(x => $"-{new DirectoryInfo(x).Name}");
		var profileSelected = PromptAndInput($"Profiles available:\n{string.Join("\n", profiles)}");
		var profilePath = Path.Combine(modCacheRoot.FullName, profileSelected);
		if(!Directory.Exists(profilePath))
		{
			Console.WriteLine("Profile does not exist. Enter a valid profile.");
			goto case '4';
		}
		profileRoot = new DirectoryInfo(profilePath);
		var dataRoot = profileRoot.CreateSubdirectory(WKDATADIRNAME);
		CopyProfile(profileRoot, bepinExRoot);
		CopyAll(dataRoot, wkDataDirRoot);
		Console.Write("Profile loaded succesfully. Press any key to exit...");
		Console.Read();
		break;
	case '5':
		Environment.Exit(0);
		break;
}

static string PromptAndInput(string prompt)
{
	Console.WriteLine('\n');
	var badInputIndicatorLocation = Console.CursorTop - 1;
	Console.Write(prompt + "\n>");
	var cursorTop = Console.CursorTop;
	var input = Console.ReadLine();
	while(string.IsNullOrWhiteSpace(input))
	{
		Console.SetCursorPosition(0, badInputIndicatorLocation);
		Console.Write("Invalid input. Try again.");
		Console.SetCursorPosition(0, cursorTop);
		Console.Write(new string(' ', Console.WindowWidth));
		Console.SetCursorPosition(0, cursorTop);
		Console.Write('>');
		input = Console.ReadLine();
	}
	return input;
}
void GetRequiredFolderLocations(string whiteKnucklesPath,
	[NotNull] out DirectoryInfo wkDirRoot, 
	[NotNull] out DirectoryInfo? bepinExRoot,
	[NotNull] out DirectoryInfo modCacheRoot,
	[NotNull] out DirectoryInfo wkDataDirRoot)
{
	wkDirRoot = new DirectoryInfo(whiteKnucklesPath);
	bepinExRoot = wkDirRoot.EnumerateDirectories().FirstOrDefault(x => x.Name is BEPINEXDIRNAME);
	if(bepinExRoot is null)
	{
		Console.WriteLine($"BepInEx directory not found in {whiteKnucklesPath}. Exiting.");
		Console.Read();
		Environment.Exit(1);
	}
	modCacheRoot = Directory.CreateDirectory(Path.Combine(whiteKnucklesPath, MODCACHENAME));
	wkDataDirRoot = new DirectoryInfo(Path.Combine(whiteKnucklesPath, WKDATADIRNAME));
}
static void GetOrCreateBepinExFolders(DirectoryInfo bepinExRoot, out DirectoryInfo pluginsRoot, out DirectoryInfo patchersRoot, out DirectoryInfo configRoot)
{
	pluginsRoot = bepinExRoot.CreateSubdirectory(BEPINEXPLUGINSNAME);
	patchersRoot = bepinExRoot.CreateSubdirectory(BEPINEXPATCHERSNAME);
	configRoot = bepinExRoot.CreateSubdirectory(BEPINEXCONFIGNAME);
}
void CreateVanillaProfileOrIgnore(DirectoryInfo bepinExRoot, DirectoryInfo wkDataDirRoot, DirectoryInfo modCacheRoot)
{
	if(modCacheRoot.EnumerateDirectories().Any(x => x.Name is "Vanilla"))
	{
		return;
	}
	var vanillaProfile = modCacheRoot.CreateSubdirectory("Vanilla");
	var vanillaData = Directory.CreateDirectory(Path.Combine(vanillaProfile.FullName, WKDATADIRNAME));
	CopyAll(wkDataDirRoot, vanillaData);
	CopyProfile(bepinExRoot, vanillaProfile);
}
void CopyProfile(DirectoryInfo bepinExRoot, DirectoryInfo toRoot)
{
	GetOrCreateBepinExFolders(bepinExRoot, out var pluginsRoot, out var patchersRoot, out var configRoot);
	GetOrCreateBepinExFolders(toRoot, out var toPluginsRoot, out var toPatchersRoot, out var toConfigRoot);
	CopyAll(pluginsRoot, toPluginsRoot);
	CopyAll(patchersRoot, toPatchersRoot);
	CopyAll(configRoot, toConfigRoot);
}
static void CopyAll(DirectoryInfo source, DirectoryInfo target)
{
	Directory.CreateDirectory(target.FullName);

	// Copy each file into the new directory.
	foreach(FileInfo fi in source.GetFiles())
	{
		fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
	}

	// Copy each subdirectory using recursion.
	foreach(DirectoryInfo diSourceSubDir in source.GetDirectories())
	{
		DirectoryInfo nextTargetSubDir =
			target.CreateSubdirectory(diSourceSubDir.Name);
		CopyAll(diSourceSubDir, nextTargetSubDir);
	}
}