namespace WhiteKnucklesModLoader.Utility;

internal static class ConsoleUtility
{
	public static string PromptAndInput(string prompt)
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
}
