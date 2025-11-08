using Backlog_Expedition.Model;
using Archipelago.MultiClient.Net.Enums;
using System.Drawing;

namespace Backlog_Expedition
{
    public static class ScreenHandler
    {
        private static readonly string asciiArtPath = $"{Environment.CurrentDirectory}\\DataStorage\\Ascii";

        public static void PrintLoginScreen(List<string> messages)
        {
            Console.Clear();

            HelperMethods.Log("Printing Login Screen");
            Console.ForegroundColor = ConsoleColor.Green;
            PrintAscii("island", true);
            Console.ResetColor();

            PrintMessages(messages, clear: false, wait: false);
            Console.WriteLine();
        }

        public static void PrintConnectedScreen(string message)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            PrintAscii("archipelago_medium", true);
            Console.WriteLine();
            Console.ResetColor();

            PrintMessage(message, true, false, true, color: ConsoleColor.Green);
        }

        public static void PrintDisconnectedScreen(List<string> messages)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            PrintAscii("archipelago_medium", true);
            Console.WriteLine();

            PrintMessages(messages, true, false, true, color: ConsoleColor.Red);
        }

        public static void PrintIntroScreen(List<string> messages)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            PrintAscii("intro", true);
            Console.WriteLine();
            Console.ResetColor();

            PrintMessages(messages, true, false, true, color: ConsoleColor.White);
        }

        public static void PrintMainScreen(List<Region> availableRegions, GoalHandler goalHandler)
        {
            HelperMethods.Log("Printing Main Screen");

            Console.Clear();

            Console.WriteLine();
            PrintMessage($"Treasures found: {goalHandler.TreasuresFound}/{goalHandler.TreasuresToGoal}", clear: false, wait: false);
            Console.WriteLine();

            List<string> runeFileNames = [.. availableRegions.Where(r => r.Name != "Starting").Select(r => r.RuneAsciiFileNameWText)];

            if (runeFileNames.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                PrintMessage("You do not have any runes.", clear: false, wait: false);
                Console.WriteLine();
            }
            else
            {
                List<string[]> Arts = LoadMultipleArtFiles(runeFileNames);

                Console.ForegroundColor = ConsoleColor.Blue;
                PrintAsciisHorizontally(Arts, true, 7);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void PrintIslandScreen(Region region)
        {
            HelperMethods.Log($"Printing {region.Name} Island screen");

            Console.Clear();
            Console.WriteLine($"{region.Name} Island");

            Console.ForegroundColor = ConsoleColor.Red;
            PrintIslandLocations([.. region.Locations.Where(l => l.Entity == "monster")]);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            PrintIslandLocations([.. region.Locations.Where(l => l.Entity == "container")]);

            Console.ResetColor();
            Console.WriteLine();
        }

        public static void PrintIslandLocations(List<Location> locations)
        {
            List<string> filenames = [];

            foreach (Location location in locations)
            {
                filenames.Add(location.AsciiFileName);
            }

            PrintAsciisHorizontally(LoadMultipleArtFiles(filenames), true, 7);
        }

        public static void PrintLocationScreen(Location location, Dictionary<string, string> descriptions)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            PrintAscii("archipelago_large", true);
            Console.WriteLine();
            Console.ResetColor();

            string message = "";

            ConsoleColor color = ConsoleColor.White;

            switch (location.ScoutedInfo.Flags)
            {
                case ItemFlags.Advancement:
                    message = descriptions["progression"];
                    color = ConsoleColor.Magenta;
                    break;
                case ItemFlags.NeverExclude:
                    message = descriptions["useful"];
                    color = ConsoleColor.Cyan;
                    break;
                case ItemFlags.Trap:
                    message = descriptions["trap"];
                    color = ConsoleColor.Red;
                    break;
                default:
                    message = descriptions["filler"];
                    color = ConsoleColor.Gray;
                    break;
            }

            PrintMessage(message
                .Replace("ENTITY", location.EntityName)
                .Replace("ITEM", location.ScoutedInfo.ItemName)
                .Replace("PLAYER", location.ScoutedInfo.Player.Name), clear: false, color: color);
        }

        public static void PrintTreasureScreen(Region region, Dictionary<string, List<string>> descriptions)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            PrintAscii(region.TreasureAsciiFileName, true);
            Console.WriteLine();
            Console.ResetColor();

            List<string> treasure_description = descriptions.Where(d => d.Key == region.TreasureName).ToList()[0].Value;
            PrintMessages(treasure_description, true, false, true);
        }

        public static void PrintGoalScreen(List<string> goal)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            PrintAscii("goal", true);
            Console.WriteLine();
            Console.ResetColor();

            PrintMessages(goal, true, false, color: ConsoleColor.Green);
        }

        public static void PrintMessage(string message, bool center = true, bool clear = true, bool wait = true, ConsoleColor color = ConsoleColor.White)
        {
            if (clear)
                Console.Clear();

            if (center)
            {
                int windowWidth = Console.WindowWidth;
                int textLength = message.Length;
                int leftPadding = (windowWidth - textLength) / 2;
                message = new string(' ', leftPadding) + message;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();

            if (wait)
                Console.ReadKey(true);
        }

        public static void PrintMessages(List<string> messages, bool center = true, bool clear = true, bool wait = true, ConsoleColor color = ConsoleColor.White)
        {
            if (clear)
                Console.Clear();

            Console.ForegroundColor = color;
            foreach (string message in messages)
            {
                string thisMessage = message;
                if (center)
                {
                    int windowWidth = Console.WindowWidth;
                    int textLength = message.Length;
                    int leftPadding = (windowWidth - textLength) / 2;
                    thisMessage = new string(' ', leftPadding) + message;
                }

                Console.WriteLine(thisMessage);
            }
            Console.ResetColor();

            if (wait)
                Console.ReadKey(true);
        }

        private static List<string[]> LoadMultipleArtFiles(List<string> runeFileNames)
        {
            List<string[]> asciiArts = [];

            foreach (var filename in runeFileNames)
            {
                LoadArtFile(filename, out string filePath);

                asciiArts.Add(File.ReadAllLines(filePath));
            }

            return asciiArts;
        }

        private static void PrintAscii(string filename, bool center)
        {
            LoadArtFile(filename, out string filePath);

            // Print art
            if (center)
            {
                string[] lines = File.ReadAllLines(filePath);

                int consoleWidth = Console.WindowWidth;

                foreach (var line in lines)
                {
                    int leftPadding = Math.Max(0, (consoleWidth - line.Length) / 2);
                    Console.WriteLine(new string(' ', leftPadding) + line);
                }
            }
            else
            {
                string asciiArt = File.ReadAllText(filePath);
                Console.WriteLine(asciiArt);
            }
        }

        private static void PrintAsciisHorizontally(List<string[]> asciiArts, bool center, int maxPerLine)
        {
            int spacing = 4;
            string spacer = new(' ', spacing);

            for (int batchStart = 0; batchStart < asciiArts.Count; batchStart += maxPerLine)
            {
                var batch = asciiArts.Skip(batchStart).Take(maxPerLine).ToList();

                // Aligns bottoms by adding top padding
                int maxHeight = batch.Max(a => a.Length);
                for (int i = 0; i < batch.Count; i++)
                {
                    if (batch[i].Length < maxHeight)
                    {
                        int padLines = maxHeight - batch[i].Length;
                        var padded = new List<string>(Enumerable.Repeat("", padLines));
                        padded.AddRange(batch[i]);
                        batch[i] = [.. padded];
                    }
                }

                for (int row = 0; row < maxHeight; row++)
                {
                    string combinedRow = string.Join(spacer, batch.Select(a => a[row]));

                    if (center)
                    {
                        int consoleWidth = Console.WindowWidth;
                        int leftPadding = Math.Max(0, (consoleWidth - combinedRow.Length) / 2);
                        Console.WriteLine(new string(' ', leftPadding) + combinedRow);
                    }
                    else
                    {
                        Console.WriteLine(combinedRow);
                    }
                }
                Console.WriteLine();
            }
        }

        private static void LoadArtFile(string filename, out string filePath)
        {
            filePath = $"{asciiArtPath}\\{filename}.txt";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }
        }
    }
}
