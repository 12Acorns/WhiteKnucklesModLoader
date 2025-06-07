using WhiteKnucklesModLoader.Extensions;
using WhiteKnucklesModLoader.Utility;

internal sealed record class Profile
{
	public Profile(string profileName)
	{
		if(string.IsNullOrWhiteSpace(profileName))
		{
			throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(profileName));
		}
		var path = Path.Combine(PathManager.ProfileRoot.FullName, profileName);
		ProfileLocation = Directory.CreateDirectory(path);
	}

	public DirectoryInfo ProfileLocation { get; }

	public async Task SaveDataIntoProfile(DirectoryInfo data)
	{
		if(data is null)
		{
			throw new ArgumentNullException(nameof(data), "Data directory cannot be null.");
		}
		await PathManager.CopyAllAsync(data, ProfileLocation.CreateSubdirectory(data.Name)).ConfigureAwait(false);
	}
	public void ClearProfileData() => ProfileLocation.Empty();

	public static bool Exists(string name)
	{
		if(string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		var profilePath = Path.Combine(PathManager.ProfileRoot.FullName, name);
		return Directory.Exists(profilePath);
	}
	public static void DeleteProfile(Profile profile)
	{
		if(profile is null)
		{
			throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
		}
		profile.ProfileLocation.Delete(true);
	}
}