using MCGalaxy;
using MCGalaxy.Commands;

public class RainbowPlugin : Plugin
{
    public override string name { get { return "RainbowPlugin"; } }

    public override void Load()
    {
        Command.Register(new Command("rainbowtext", RainbowText));
    }

    // Command to display rainbow text
    public static void RainbowText(CommandArgs args)
    {
        if (args.Length == 0)
        {
            args.Player.SendMessage("Please provide some text.");
            return;
        }

        string inputText = args.GetJoined(0);
        string rainbowText = GetRainbowText(inputText);
        args.Player.SendMessage(rainbowText);
    }

    // Function to apply the rainbow effect
    public static string GetRainbowText(string input)
    {
        string result = "";
        string[] colors = new string[] { "1", "2", "a", "b", "3", "9", "5", "d", "4", "c", "6", "e" }; // Color codes
        int colorIndex = 0;

        foreach (char c in input)
        {
            if (c == '%') // Handle special character "%"
            {
                result += "%" + colors[colorIndex % colors.Length]; // Apply the next color
                colorIndex++;
            }
            else
            {
                result += c; // Append the character itself
            }
        }

        return result;
    }
}
