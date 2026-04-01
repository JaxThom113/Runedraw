using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData 
{ 
    // data for transitioning from CharacterMenu scene -> Overworld scene
    public static int SelectedAreaType = 1; 
    public static int SelectedPlayer = 0;
    public static bool IsSeededRun = false;
    public static int SelectedSeed = 0;

    // data for the Run Info panel in Overworld
    public static float PlayTime = 0;

    public static bool StartedFromTutorial = false;
    public static int Area1 = 0;
    public static int Area2 = 0;
    public static int Area3 = 0;
    public static bool WinRun = false;

    public static int EnemiesFought = 0;
    public static int ChestsLooted = 0;
    public static int TimesRested = 0;
    public static int CardsBurned = 0;
    public static int RunesPlayed = 0;
    public static int MostPlayedCard = 0;
}
