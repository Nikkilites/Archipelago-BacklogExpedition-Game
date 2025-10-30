using Backlog_Expedition;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll", SetLastError = true)]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll", SetLastError = true)]
static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


IntPtr console = GetConsoleWindow();

int width = 1920;
int height = 1080;
int x = 0;
int y = 0;
MoveWindow(console, x, y, width, height, true);

try
{
    int charWidth = width / 8;
    int charHeight = height / 16;

    Console.SetWindowSize(Math.Min(charWidth, Console.LargestWindowWidth),
                            Math.Min(charHeight, Console.LargestWindowHeight));

    Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
}
catch (Exception ex)
{
    Console.WriteLine($"Could not resize console text area: {ex.Message}");
}

GameHandler gameHandler = new GameHandler();

gameHandler.StartGame();
