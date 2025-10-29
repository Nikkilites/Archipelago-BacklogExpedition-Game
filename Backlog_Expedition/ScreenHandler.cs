using Backlog_Expedition.Model;

namespace Backlog_Expedition
{
    public static class ScreenHandler
    {
        private static readonly string asciiArtPath = $"{Environment.CurrentDirectory}\\DataStorage\\Ascii";

        public static void PrintIslandScreen(Region region)
        {
            Console.Clear();
            Console.WriteLine($"{region.Name} Island");

            Console.ForegroundColor = ConsoleColor.Red;
            PrintIslandLocations([.. region.Locations.Where(l => l.Entity == "monster")]);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            PrintIslandLocations([.. region.Locations.Where(l => l.Entity == "container")]);

            Console.ResetColor();
            Console.WriteLine();
        }

        private static void PrintIslandLocations(List<Location> locations)
        {
            List<string> filenames = [];

            foreach (Location location in locations)
            {
                filenames.Add(location.AsciiFileName);
            }

            PrintAsciisHorizontally(LoadMultipleArtFiles(filenames), true, 7);
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

        public static void PrintMainScreen(List<Region> availableRegions)
        {
            HelperMethods.Log("Printing Main Screen");

            Console.Clear();

            List<string> runeFileNames = [.. availableRegions.Where(r => r.Name != "Starting").Select(r => r.RuneAsciiFileNameWText)];

            if (runeFileNames.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                PrintMessage("You do not have any runes.", clear: false, wait: false);
                Console.WriteLine();
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
