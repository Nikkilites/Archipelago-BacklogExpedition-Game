using Backlog_Expedition.Archipelago;
using Backlog_Expedition.Model;
using System.Data;

namespace Backlog_Expedition
{
    public class GameHandler
    {
        public static ConnectionHandler ConnectionHandler { get; private set; }
        public static DataStorageHandler DataStorageHandler { get; private set; }
        public static GoalHandler GoalHandler { get; private set; }
        public static ItemHandler ItemHandler { get; private set; }
        public static RegionHandler RegionHandler { get; private set; }

        public GameHandler()
        {
            ConnectionHandler = new();
            RegionHandler = new();
            ItemHandler = new();
            GoalHandler = new();
            DataStorageHandler = new();
        }

        public async void StartGame()
        {
            HelperMethods.ClearLog();

            while (!ConnectionHandler.Connected)
            {
                ScreenHandler.PrintLoginScreen(DataStorageHandler.StoryData.login);
                try
                {
                    Console.ResetColor();

                    Console.WriteLine("Server: ");
                    string? server = Console.ReadLine()?.Trim();

                    Console.WriteLine("Player: ");
                    string? player = Console.ReadLine()?.Trim();

                    Console.WriteLine("Password (optional): ");
                    string? pass = Console.ReadLine()?.Trim();
                    pass ??= string.Empty;

                    if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(player))
                    {
                        ScreenHandler.PrintMessage("Server and Player fields are required. Press any key to try again.", color: ConsoleColor.Yellow);
                        continue;
                    }

                    bool success = ConnectionHandler.Connect(server, player, pass);

                    if (!success)
                    {
                        ScreenHandler.PrintMessage("Connection failed. Press any key to try again.", color: ConsoleColor.Red);
                        continue;
                    }

                    ItemHandler.SetupFrameUpdater();
                }
                catch (Exception ex)
                {
                    ScreenHandler.PrintMessage($"Unexpected error while connecting:\n{ex.Message}\nPress any key to try again.", color: ConsoleColor.Red);
                }
            }

            Dictionary<string, object> slotData = ConnectionHandler.SlotData;

            GoalHandler.BeatenToGoal = Convert.ToInt32(slotData["beaten_to_goal"]);

            RegionHandler.CreateRegions(slotData);

            ScreenHandler.PrintConnectedScreen("Successfully connected to Archipelago! Press any key to continue to the game");

            ScreenHandler.PrintIntroScreen(DataStorageHandler.StoryData.introduction);

            PlayGame();
        }

        private static void PlayGame()
        {
            bool inMenu = true;
            while (inMenu)
            {
                List<Region> availableRegions = [.. RegionHandler.Regions.Where(r => r.RuneReceived == true)];

                ScreenHandler.PrintMainScreen(availableRegions);

                int i = 1;

                foreach (Region region in availableRegions)
                {
                    string toAdd = "";
                    if (region.TreasureFound)
                    {
                        toAdd = " (Treasure Found";
                        if (region.Locations.Count() == 0)
                            toAdd = $"{toAdd} - Fully Raided)";
                        else
                            toAdd = $"{toAdd})";
                    }

                    Console.WriteLine($"{i}. Go to {region.Name} Island{toAdd}");
                    i++;
                }

                Console.WriteLine($"{i}. Refresh");
                i++;
                Console.WriteLine($"{i}. Exit Game");

                string? selection = Console.ReadLine()?.Trim();

                if (selection != null && int.TryParse(selection, out int keypress))
                {
                    if (keypress > 0 && keypress <= availableRegions.Count)
                    {
                        OpenIsland(availableRegions[keypress - 1]);
                    }
                    else if (keypress == i - 1)
                    {
                        continue;
                    }
                    else if (keypress == i)
                    {
                        inMenu = false;
                    }
                    else
                    {
                        ScreenHandler.PrintMessage("Invalid input! Press to try again", color: ConsoleColor.Yellow);
                    }
                }
                else
                {
                    ScreenHandler.PrintMessage("Invalid input! Press to try again", color: ConsoleColor.Yellow);
                }
            }
            Environment.Exit(0);
        }

        private static void OpenIsland(Region region)
        {
            bool onIsland = true;
            while (onIsland)
            {
                ScreenHandler.PrintIslandScreen(region);

                int i = 1;

                foreach (Location location in region.Locations)
                {
                    Console.WriteLine($"{i}. {location.Name.Split(" in ")[0]} - ({location.Hint})");
                    i++;
                }

                Console.WriteLine($"{i}. Return to Map");

                string? selection = Console.ReadLine()?.Trim();

                if (selection != null && int.TryParse(selection, out int keypress))
                {
                    if (keypress > 0 && keypress <= region.Locations.Count)
                    {
                        Location pickedLocation = region.Locations[keypress - 1];

                        region.CheckLocation(pickedLocation);

                        Dictionary<string, string> descriptions = pickedLocation.Entity == "monster" ? DataStorageHandler.StoryData.slay_monster : DataStorageHandler.StoryData.open_chest;
                        ScreenHandler.PrintLocationScreen(pickedLocation, descriptions);

                        if (region.TreasureFound && pickedLocation.Entity == "monster")
                            ScreenHandler.PrintTreasureScreen(region, DataStorageHandler.StoryData.treasure_descriptions);

                        GoalHandler.CheckIfGoal();
                    }
                    else if (keypress == i)
                    {
                        onIsland = false;
                    }
                    else
                    {
                        ScreenHandler.PrintMessage("Invalid input! Press to try again", color: ConsoleColor.Yellow);
                    }
                }
                else
                {
                    ScreenHandler.PrintMessage("Invalid input! Press to try again", color: ConsoleColor.Yellow);
                }
            }
        }
    }
}
