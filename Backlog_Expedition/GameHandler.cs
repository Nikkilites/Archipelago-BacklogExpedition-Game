using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
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

            bool gameIsOpen = true;
            while (gameIsOpen)
            {
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

                GoalHandler.TreasuresToGoal = Convert.ToInt32(slotData["beaten_to_goal"]);

                RegionHandler.CreateRegions(slotData);

                ItemHandler.SetupItemHandler();

                ScreenHandler.PrintConnectedScreen("Successfully connected to Archipelago! Press any key to continue to the game");

                ScreenHandler.PrintIntroScreen(DataStorageHandler.StoryData.introduction);

                PlayGame();
            }
        }

        private static void PlayGame()
        {
            bool inMenu = true;
            while (inMenu && ConnectionHandler.Connected)
            {
                List<Region> availableRegions = [.. RegionHandler.Regions.Where(r => r.RuneReceived == true)];

                ScreenHandler.PrintMainScreen(availableRegions, GoalHandler);

                int i = 1;

                foreach (Region region in availableRegions)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    string toAdd = "";
                    if (region.TreasureFound)
                    {
                        toAdd = " (Treasure Found";
                        if (region.Locations.Count() == 0)
                        { 
                            toAdd = $"{toAdd} - Fully Raided)";
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                            toAdd = $"{toAdd})";
                    }

                    Console.WriteLine($"{i}. Go to {region.Name} Island{toAdd}");
                    i++;
                }

                Console.WriteLine($"{i}. Go to the Hint Shop");
                i++;
                Console.WriteLine($"{i}. Refresh");
                i++;
                Console.WriteLine($"{i}. Exit Game");

                string? selection = Console.ReadLine()?.Trim();

                if (!ConnectionHandler.Connected)
                {
                    ScreenHandler.PrintDisconnectedScreen(["You have been disconnected from archipelago!", "Please login again."]);
                    inMenu = false;
                    continue;
                }
                else if (selection != null && int.TryParse(selection, out int keypress))
                {
                    if (keypress > 0 && keypress <= availableRegions.Count)
                    {
                        OpenIsland(availableRegions[keypress - 1]);
                    }
                    else if (keypress == i - 2)
                    {
                        OpenHintShop();
                    }
                    else if (keypress == i - 1)
                    {
                        continue;
                    }
                    else if (keypress == i)
                    {
                        inMenu = false;
                        Environment.Exit(0);
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

        private static void OpenIsland(Region region)
        {
            bool onIsland = true;
            while (onIsland && ConnectionHandler.Connected)
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

                if (!ConnectionHandler.Connected)
                {
                    ScreenHandler.PrintDisconnectedScreen(["You have been disconnected from archipelago!", "The location has not been sent.", "Please log in again."]);
                    onIsland = false;
                    continue;
                }
                else if (selection != null && int.TryParse(selection, out int keypress))
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

        private static void OpenHintShop()
        {
            bool inShop = true;
            while (inShop && ConnectionHandler.Connected)
            {
                ScreenHandler.PrintHintShopScreen(ItemHandler.TrashAvailable);

                int hintCost = (int)Math.Round(ItemHandler.TrashInWorld / 20.0, MidpointRounding.AwayFromZero);
                if (hintCost <= 1)
                    hintCost = 1;

                Console.WriteLine($"1. Purchase Hint (Cost: {hintCost} trash)");
                Console.WriteLine($"2. Return to Map");

                string? selection = Console.ReadLine()?.Trim();

                if (!ConnectionHandler.Connected)
                {
                    ScreenHandler.PrintDisconnectedScreen(["You have been disconnected from archipelago!", "The location has not been sent.", "Please log in again."]);
                    inShop = false;
                    continue;
                }
                else if (selection != null && int.TryParse(selection, out int keypress))
                {
                    if (keypress == 1)
                    {
                        if (ItemHandler.TrashAvailable >= hintCost)
                        {
                            List<long> alreadyHintedLocationIds = ConnectionHandler.GetHints()
                                .Where(h => h.Status == HintStatus.Unspecified)
                                .Select(h => h.LocationId)
                                .ToList();

                            List<Location> allLocations = RegionHandler.Regions
                                .Where(r => r.Locations.Any())
                                .SelectMany(r => r.Locations)
                                .Where(loc => !alreadyHintedLocationIds.Contains(loc.Id))
                                .ToList();

                            if (allLocations.Count > 0)
                            {
                                Random rng = new Random();
                                Location randomLocation = allLocations[rng.Next(allLocations.Count)];

                                ConnectionHandler.SendLocationHint(randomLocation.Id);
                                ItemHandler.UseTrash(hintCost);
                                ScreenHandler.PrintHintScreen(randomLocation);
                            }
                            else
                            {
                                ScreenHandler.PrintMessage("You have no unhinted locations to hint!", color: ConsoleColor.Yellow);
                            }
                        }
                        else
                        {
                            ScreenHandler.PrintMessage("You do not have enough trash to pay for this hint!", color: ConsoleColor.Yellow);
                        }
                    }
                    else if (keypress == 2)
                    {
                        inShop = false;
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
