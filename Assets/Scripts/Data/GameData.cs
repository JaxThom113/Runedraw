using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData 
{ 
    // data for transitioning from Menu scene -> Overwold scene
    public static int SelectedAreaType = 1; 
    public static int SelectedPlayer = 0;

    // data for the Run Info panel
    public static int EnemiesFought = 0;
    public static int ChestsLooted = 0;
    public static int TimesRested = 0;
    public static int CardsBurned = 0;
    public static int RunesPlayed = 0;
    public static int PlayTime = 0;
    public static int Seed = 0;
    public static string MostPlayedCard = "Cut"; 
    public static bool StartedFromTutorial = false;
    public static int Area1 = 0;
    public static int Area2 = 0;
    public static int Area3 = 0;
    public static bool WinRun = false;
}
