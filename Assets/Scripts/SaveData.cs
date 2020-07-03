//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SaveData {
    //handles the player's save data

    //settings
    public float globalAudioScale;
    public bool joystickOnRight;
    public bool showControlsOnMenu;

    //game progress
    public int levelsBeaten;
    public bool hasChangedColorYet;

    //variables used in calculating achievements and available skins
    public string playerMaterial;
    public bool hasBeenOutOfBounds;
    public int[] heartsLeftPerLevel;
    public string achievementsUnlocked;
    public int lastLevelLost;
    public int secondLastLevelLost;

    public SaveData() {
        //takes all of the necessary variables from StaticVariables and stores them into the SaveData object variables
        globalAudioScale = StaticVariables.globalAudioScale;
        joystickOnRight = StaticVariables.joystickOnRight;
        levelsBeaten = StaticVariables.levelsBeaten;
        playerMaterial = StaticVariables.playerMat + "";
        hasChangedColorYet = StaticVariables.hasChangedColorYet;
        showControlsOnMenu = StaticVariables.showControlsOnMenu;
        hasBeenOutOfBounds = StaticVariables.hasBeenOutOfBounds;
        heartsLeftPerLevel = StaticVariables.heartsLeftPerLevel;
        achievementsUnlocked = StaticVariables.achievementsUnlocked;
        lastLevelLost = StaticVariables.lastLevelLost;
        secondLastLevelLost = StaticVariables.secondLastLevelLost;
    }

    public void LoadData() {
        //takes all of the variables stored in the SaveData object and stores them into StaticVariables
        StaticVariables.globalAudioScale = globalAudioScale;
        StaticVariables.joystickOnRight = joystickOnRight;
        StaticVariables.levelsBeaten = levelsBeaten;
        StaticVariables.hasChangedColorYet = hasChangedColorYet;
        StaticVariables.playerMat = StaticVariables.materialsList[0];
        StaticVariables.showControlsOnMenu = showControlsOnMenu;
        StaticVariables.hasBeenOutOfBounds = hasBeenOutOfBounds;
        StaticVariables.heartsLeftPerLevel = heartsLeftPerLevel;
        StaticVariables.achievementsUnlocked = achievementsUnlocked;
        StaticVariables.lastLevelLost = lastLevelLost;
        StaticVariables.secondLastLevelLost = secondLastLevelLost;
        foreach (Material mat in StaticVariables.materialsList) {
            if (playerMaterial == mat + "") {
                StaticVariables.playerMat = mat;
            }
        }
    }
}
