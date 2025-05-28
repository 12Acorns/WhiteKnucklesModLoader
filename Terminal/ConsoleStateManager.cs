using System.Runtime.InteropServices;

internal static partial class ConsoleStateManager
{
	private const int STDOUTPUTHANDLE = -11;
	private const uint ENABLEVIRTUALTERMINALPROCESSING = 4;

	/// <summary>
	/// Indicates if virtual termianl processing is supported on system
	/// <para></para>
	/// MUST USE BEFORE TRYING TO FORMAT AS TO PREVENT UNEXPECTED BEHAVIOUR
	/// </summary>
	public static bool IsVirtual
	{
		get
		{
			if(!_firstCall)
			{
				return _isVirtual;
			}
			return _isVirtual = TryInitialise();
		}
	}
	private static bool _isVirtual;
	private static bool _firstCall = true;

	[LibraryImport("kernel32.dll", SetLastError = true)]
	private static partial nint GetStdHandle(int nStdHandle);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

	public static bool Initialize() => IsVirtual;
	private static bool TryInitialise()
	{
		_firstCall = false;
		if(!IsValidOperatingSystem())
		{
			return false;
		}

		var handle = GetStdHandle(STDOUTPUTHANDLE);

		if(handle == Marshal.GetLastWin32Error())
		{
			return false;
		}

		if(!GetConsoleMode(handle, out uint consoleMode))
		{
			return false;
		}

		consoleMode |= ENABLEVIRTUALTERMINALPROCESSING;

		return SetConsoleMode(handle, consoleMode);
	}
	private static bool IsValidOperatingSystem()
	{
		if(!OperatingSystem.IsWindows())
		{
			return false;
		}
		return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10586);
	}
}