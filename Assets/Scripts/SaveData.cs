using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SaveData {

    public float globalAudioScale;
    public bool joystickOnRight;
    public int levelsBeaten;
    public string levelsBeatenWithFullHP;
    public bool hasChangedColorYet;
    public string playerMaterial;
    public bool showControlsOnMenu;
    public bool hasBeenOutOfBounds;
    //to save:
    //levels beaten
    //which levels you beat with all hearts
    //have you made it out of bounds

    public SaveData() {
        globalAudioScale = StaticVariables.globalAudioScale;
        joystickOnRight = StaticVariables.joystickOnRight;
        levelsBeaten = StaticVariables.levelsBeaten;
        levelsBeatenWithFullHP = StaticVariables.levelsBeatenWithFullHP;
        playerMaterial = StaticVariables.playerMat + "";
        hasChangedColorYet = StaticVariables.hasChangedColorYet;
        showControlsOnMenu = StaticVariables.showControlsOnMenu;
        hasBeenOutOfBounds = StaticVariables.hasBeenOutOfBounds;
    }

    public void LoadData() {
        StaticVariables.globalAudioScale = globalAudioScale;
        StaticVariables.joystickOnRight = joystickOnRight;
        StaticVariables.levelsBeaten = levelsBeaten;
        StaticVariables.levelsBeatenWithFullHP = levelsBeatenWithFullHP;
        StaticVariables.hasChangedColorYet = hasChangedColorYet;
        StaticVariables.playerMat = StaticVariables.materialsList[0];
        StaticVariables.showControlsOnMenu = showControlsOnMenu;
        StaticVariables.hasBeenOutOfBounds = hasBeenOutOfBounds;
        foreach (Material mat in StaticVariables.materialsList) {
            if (playerMaterial == mat + "") {
                StaticVariables.playerMat = mat;
            }
        }

        
    }


}
