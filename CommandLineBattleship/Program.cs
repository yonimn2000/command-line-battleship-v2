using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using YonatanMankovich.BattleshipGameLibrary;
using YonatanMankovich.SimpleConsoleMenus;

namespace YonatanMankovich.CommandLineBattleship
{
    class Program
    {
        static void Main(string[] args)
        {
            MaximizeConsoleWindow();
            Console.Title = "Yonatan's Command Line Battleship Game";
            Console.CursorVisible = false;
            while (true)
            {
                Console.WriteLine("Welcome to Yonatan's Command Line Battleship Game!");
                ShowStartMenu();
                Console.Clear();
            }
        }

        private static void ShowStartMenu()
        {
            SimpleActionConsoleMenu startMenu = new SimpleActionConsoleMenu("What would you like to do?");
            startMenu.AddOption("Play", StartGame);
            startMenu.AddOption("Change game options", delegate
            {
                Board.Options.CreateDefaultBoardOptionsFileIfDoesNotExist();
                Process.Start(Board.Options.PATH_TO_OPTIONS);
            });
            startMenu.AddOption("Exit", () => Environment.Exit(0));
            startMenu.ShowAndDoAction();
        }

        private static void StartGame()
        {
            Console.Clear();
            GameController gameController = new GameController();
            gameController.Play();
        }

        // This code is for maximizing the window. *********************
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        private static void MaximizeConsoleWindow()
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 3);
        }
        //**************************************************************
    }
}