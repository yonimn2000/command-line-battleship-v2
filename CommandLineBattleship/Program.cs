using System;
using YonatanMankovich.BattleshipGameEngine;
using YonatanMankovich.SimpleConsoleMenus;

namespace YonatanMankovich.CommandLineBattleship
{
    class Program
    {
        static void Main(string[] args)
        {
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
                System.Diagnostics.Process.Start(Board.Options.PATH_TO_OPTIONS);
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
    }
}