using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables {

    static public bool isApplicationLaunchingFirstTime = true;
    static public bool pressingRollButton = false;
    static public bool justPressedPunchButton = false;
    static public bool usingJoystick = false;
    static public Vector2 joystickDirection;

    //things to save in between sessions
    static public float globalAudioScale = 1f;
    static public bool joystickOnRight = false;
    static public int levelsBeaten = 0;
    //static public string levelsBeatenWithFullHP = "0";
    static public bool showControlsOnMenu = false;
    static public bool hasBeenOutOfBounds = false;
    static public string achievementsUnlocked = "PLAY BATTLE BOX";

    static public int[] heartsLeftPerLevel = new int[36]; // automatically initialized as 0
    
    static public bool hasChangedColorYet = false;
    static public Material playerMat;
    static public Material[] materialsList;
    static public bool goingToAchievements = false;
    static public bool isOutOfBounds = false;
    static public int punchCount = 0;
    static public List<Enemy> enemiesPunched = new List<Enemy>();
    static public List<Enemy> enemiesPunchedSimultaneously = new List<Enemy>();
    static public int lastLevelLost;
    static public int secondLastLevelLost;
    static public bool pausedFromAchievements;
    
}
