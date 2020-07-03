//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables {
    //contains all of the variables that need to be tracked between scenes
    //the SaveData functionality reads and writes variables to and from this class
    
    static public bool isApplicationLaunchingFirstTime = true; //if true, load data from StaticVariables

    //when true, the player should load the achievement screen as soon as they go back to the main menu. 
    //Set when the player unlocks an achievement and pushes the "go to achievements" button
    static public bool goingToAchievements = false; 

    //handling player input
    static public bool pressingRollButton = false;
    static public bool justPressedPunchButton = false;
    static public bool usingJoystick = false;
    static public Vector2 joystickDirection;

    //things to save in between sessions
    static public float globalAudioScale = 1f;
    static public bool joystickOnRight = false;
    static public int levelsBeaten = 0;
    static public bool showControlsOnMenu = false;
    static public bool hasBeenOutOfBounds = false;
    static public string achievementsUnlocked = "PLAY SOCK 'N ROLL";
    static public int[] heartsLeftPerLevel = new int[36]; // automatically initialized as 0
    static public int lastLevelLost;
    static public int secondLastLevelLost;
    static public bool hasChangedColorYet = false;

    //list of skins
    static public Material playerMat;
    static public Material[] materialsList;

    //variables that store data used to see if the player has unlocked any achievements
    static public bool isOutOfBounds = false;
    static public int punchCount = 0;
    static public List<Enemy> enemiesPunched = new List<Enemy>();
    static public List<Enemy> enemiesPunchedSimultaneously = new List<Enemy>();
    static public bool pausedFromAchievements;

    static public float resolutionMultiplier;//used in resizing canvases to fit the device screen size
    
}
