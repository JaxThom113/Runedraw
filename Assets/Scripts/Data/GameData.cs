using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData 
{ 
    // data for transitioning from CharacterMenu scene -> Overworld scene
    public static int SelectedPlayer;
    public static bool IsSeededRun;
    public static int SelectedSeed;
    public static string SpecialSeed;

    // data for the Run Info panel in Overworld
    public static float PlayTime;
    public static bool StartedFromTutorial;
    public static int Area1;
    public static int Area2;
    public static int Area3;
    public static bool WinRun;
    public static int EnemiesFought;
    public static int ChestsLooted;
    public static int TimesRested;
    public static int RunesPlayed;

    public static void InitializeData()
    {
        SelectedPlayer = 0;
        IsSeededRun = false;
        SelectedSeed = 0;
        SpecialSeed = null;

        PlayTime = 0;
        StartedFromTutorial = false;
        Area1 = 0;
        Area2 = 0;
        Area3 = 0;
        WinRun = false;
        EnemiesFought = 0;
        ChestsLooted = 0;
        TimesRested = 0;
        RunesPlayed = 0;
    }
}
