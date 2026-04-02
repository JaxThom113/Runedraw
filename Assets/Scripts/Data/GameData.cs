using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData 
{ 
    // data for transitioning from CharacterMenu scene -> Overworld scene
    public static int SelectedAreaType; 
    public static int SelectedPlayer;
    public static bool IsSeededRun;
    public static int SelectedSeed;

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
    public static int CardsBurned;
    public static int RunesPlayed;
    public static int MostPlayedCard;   

    public static void InitializeData()
    {
        SelectedAreaType = 0; 
        SelectedPlayer = 0;
        IsSeededRun = false;
        SelectedSeed = 0;

        PlayTime = 0;
        StartedFromTutorial = false;
        Area1 = 0;
        Area2 = 0;
        Area3 = 0;
        WinRun = false;
        EnemiesFought = 0;
        ChestsLooted = 0;
        TimesRested = 0;
        CardsBurned = 0;
        RunesPlayed = 0;
        MostPlayedCard = 0;   
    }
}
