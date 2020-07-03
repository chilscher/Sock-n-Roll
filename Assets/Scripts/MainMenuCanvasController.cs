//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCanvasController : MonoBehaviour {
    //controls the operation of the main menu, including all the settings, achievement-unlocking, skin-equipping, and level-entering functions

    [Header("Canvases")] //the canvases on the main menu
    public GameObject homeCanvas;
    public GameObject creditsCanvas;
    public GameObject playCanvas;
    public GameObject settingsCanvas;
    public GameObject achievementsCanvas;
    
    [Header("Back Key")]
    public KeyCode backKey = KeyCode.Escape;
    
    [Header("Player Color Materials")] //the different skins for the player
    public Material[] materialsList;

    [Header("Achievement Window Player Models")] //the player models used to show off the different colored skins
    public Player rightModel;
    public Player leftModel;


    private Player player;
    private Enemy enemy;

    private AudioManager audioManager;

    private void Start() {
        //set basic variables
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
        audioManager = FindObjectOfType<AudioManager>();
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);

        //start the main menu music
        audioManager.fadeIn("Main Menu");

        //set the StaticVariables list of materials to be those picked in the inpsector
        StaticVariables.materialsList = materialsList;

        //code that is run when the player launches the game
        if (StaticVariables.isApplicationLaunchingFirstTime) {
            SaveSystem.LoadGame();
            StaticVariables.isApplicationLaunchingFirstTime = false;

            audioManager.applyGlobalVolume();
            save();
        }
        
        //apply the settings from StaticVariables
        updatePlayerColor();
        updateFlipControlsText();
        updateShowControlsText();
        updateAudioText();
        updateAchievementButtons();
        showAchievementPlayerModels();
        if (StaticVariables.joystickOnRight) { flipControls(); }

        //set up the buttons that allow players to jump into a level
        lockLevels();
        showHeartsOnLevels();
        lockAchievements();

        //if the player came from the middle of a level to visit the acheivements page, go there immediately
        if (StaticVariables.goingToAchievements) {
            homeCanvas.SetActive(false);
            achievementsCanvas.SetActive(true);
            StaticVariables.goingToAchievements = false;
            showAchievementPlayerModels();
        }
        
        //scale the canvases to match the device screen size
        StaticVariables.resolutionMultiplier = 1850 / ((float)Screen.width / (float)Screen.height);
        homeCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2 (900,StaticVariables.resolutionMultiplier);
        settingsCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        creditsCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        playCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        achievementsCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
    }

    private void Update() {
        hitBackKey(); //if the player hits the back button on their device, go back to a different canvas, or quit the game entirely
    }

    // ---------------------------------------------------
    //ALL OF THE FUNCTIONS THAT ARE USED TO SWITCH BETWEEN CANVASES
    // ---------------------------------------------------
    
    public void _btnMainMenu() {
        //go to the main menu
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        //hide the player models on the achievement screen
        showAchievementPlayerModels();
    }

    public void _btnLevelSelect() {
        //go to the level-select screen
        settingsCanvas.SetActive(false);
        playCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        //hide the player models on the achievement screen
        showAchievementPlayerModels();
    }

    public void _btnCredits() {
        //go to the credits screen
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        //hide the player models on the achievement screen
        showAchievementPlayerModels();
    }

    private void hitBackKey() {
        //go back to a prior menu, or quit the game if on the main menu already
        if (Input.GetKeyDown(backKey)) {
            if (creditsCanvas.activeSelf) { _btnSettings(); }
            else if (achievementsCanvas.activeSelf) { _btnSettings(); }
            else if (playCanvas.activeSelf) { _btnMainMenu(); }
            else if (settingsCanvas.activeSelf) { _btnMainMenu(); }
            else if (homeCanvas.activeSelf) { _btnQuit(); }
        }
    }

    public void _btnSettings() {
        //go to the settings screen
        settingsCanvas.SetActive(true);
        creditsCanvas.SetActive(false);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        //hide the player models on the achievement screen
        showAchievementPlayerModels();
    }

    public void _btnAchievements() {
        //go to the achievements screen
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(true);
        //show the player models on the achievement screen
        showAchievementPlayerModels();
    }

    // ---------------------------------------------------
    //ALL OF THE FUNCTIONS THAT ARE USED TO TOGGLE SETTINGS
    // ---------------------------------------------------

    public void _btnMute() {
        //when the player pushes the toggle-mute button
        //turn the sounds on or off
        if (StaticVariables.globalAudioScale == 0f) {
            StaticVariables.globalAudioScale = 1f;
        }
        else {
            StaticVariables.globalAudioScale = 0f;
        }
        //start playing the main menu theme over again
        audioManager.stopAll();
        AudioListener.volume = StaticVariables.globalAudioScale;
        audioManager.play("Main Menu");
        //update the visuals for the toggle-mute button
        updateAudioText();
        save();
    }

    public void _btnFlipControls() {
        //when the player taps the flip-controls button on the settings
        //switchesall controls to the other side of the screen
        //this also applies to the in-level controls as well
        StaticVariables.joystickOnRight = !StaticVariables.joystickOnRight;
        updateFlipControlsText();
        flipControls();
        save();
    }

    private void updateFlipControlsText() {
        //update the visuals for the flip-controls button
        string s = "LEFT";
        if (StaticVariables.joystickOnRight) { s = "RIGHT"; }
        settingsCanvas.transform.Find("Flip Controls Button").Find("Text").gameObject.GetComponent<Text>().text = "FLIP CONTROLS:\nJOYSTICK ON " + s;
    }

    public void _btnShowControlsOnMenu() {
        //when the player taps the show-controls-on-menu button from the settings
        //shows the player movement and ability controls on the home screen of the main menu
        StaticVariables.showControlsOnMenu = !StaticVariables.showControlsOnMenu;
        updateShowControlsText();
        save();
    }

    private void updateShowControlsText() {
        //update the visuals for the show-controls button
        string s = "HIDDEN";
        if (StaticVariables.showControlsOnMenu) { s = "SHOWN"; }
        settingsCanvas.transform.Find("Controls On Menu").Find("Text").gameObject.GetComponent<Text>().text = "CONTROLS ON MENU:\n" + s;

        homeCanvas.transform.Find("Player Controls").gameObject.SetActive(StaticVariables.showControlsOnMenu);
    }

    private void updateAudioText() {
        //update the visuals for the mute-audio button
        string s = "MUTED";
        if (StaticVariables.globalAudioScale == 1f) { s = "PLAYING"; }
        settingsCanvas.transform.Find("Audio Button").Find("Text").gameObject.GetComponent<Text>().text = "AUDIO:\n" + s;
    }

    private void flipControls() {
        //flips all controls to the other side of the screen
        RectTransform joystick = homeCanvas.transform.Find("Player Controls").Find("Fixed Joystick").gameObject.GetComponent<RectTransform>();
        RectTransform punchButton = homeCanvas.transform.Find("Player Controls").Find("Punch Button").gameObject.GetComponent<RectTransform>();
        RectTransform rollButton = homeCanvas.transform.Find("Player Controls").Find("Roll Button").gameObject.GetComponent<RectTransform>();
        RectTransform settingsButton = homeCanvas.transform.Find("Settings Button").gameObject.GetComponent<RectTransform>();
        RectTransform backSettings = settingsCanvas.transform.Find("Menu Button").gameObject.GetComponent<RectTransform>();
        RectTransform backCredits = creditsCanvas.transform.Find("Menu Button").gameObject.GetComponent<RectTransform>();
        RectTransform borderCredits = creditsCanvas.transform.Find("Border").gameObject.GetComponent<RectTransform>();
        RectTransform backPlay = playCanvas.transform.Find("Menu Button").gameObject.GetComponent<RectTransform>();
        RectTransform backAchievements = achievementsCanvas.transform.Find("Menu Button").gameObject.GetComponent<RectTransform>();

        Vector2 pos;

        pos = joystick.anchoredPosition;
        pos.x *= -1;
        joystick.anchoredPosition = pos;

        pos = punchButton.anchoredPosition;
        pos.x *= -1;
        punchButton.anchoredPosition = pos;

        pos = rollButton.anchoredPosition;
        pos.x *= -1;
        rollButton.anchoredPosition = pos;

        pos = settingsButton.anchoredPosition;
        pos.x *= -1;
        settingsButton.anchoredPosition = pos;

        pos = backSettings.anchoredPosition;
        pos.x *= -1;
        backSettings.anchoredPosition = pos;

        pos = backCredits.anchoredPosition;
        pos.x *= -1;
        backCredits.anchoredPosition = pos;

        pos = borderCredits.anchoredPosition;
        pos.x *= -1;
        borderCredits.anchoredPosition = pos;

        pos = backPlay.anchoredPosition;
        pos.x *= -1;
        backPlay.anchoredPosition = pos;

        pos = backAchievements.anchoredPosition;
        pos.x *= -1;
        backAchievements.anchoredPosition = pos;

        showAchievementPlayerModels();
    }

    // ---------------------------------------------------
    //FUNCTIONS USED IN THE LEVEL SELECTION PROCESS
    // ---------------------------------------------------

    public void _btnLoadLevel(int x) {
        //when the player pushes one of the go-to-level buttons on the level select canvas
        //go to that level, if the player has beaten the level before it
        if (StaticVariables.levelsBeaten + 1 >= x) {
            //if (true) { //leave this, for specific level debugging purposes in later development
            audioManager.fadeOutAll();
            SceneManager.LoadScene("Level " + x);
        }
    }

    private void lockLevels() {
        //shows a little lock icon over the levels that the player has not yet unlocked
        for (int i = 1; i <= 36; i++) {
            GameObject levelObject = playCanvas.transform.Find("Level " + i).gameObject;
            if (StaticVariables.levelsBeaten + 1 >= i) {
                levelObject.transform.Find("Text").gameObject.SetActive(true);
                levelObject.transform.Find("Locked").gameObject.SetActive(false);
            }
            else {
                levelObject.transform.Find("Text").gameObject.SetActive(false);
                levelObject.transform.Find("Locked").gameObject.SetActive(true);
            }
        }
    }

    private void showHeartsOnLevels() {
        //shows the amount of hearts the player had remaining when they beat each specific level
        for (int i = 0; i < 36; i++) {
            GameObject levelObject = playCanvas.transform.Find("Level " + (i + 1)).gameObject;
            GameObject heartsObject = levelObject.transform.Find("Hearts").gameObject;
            if (i > StaticVariables.levelsBeaten) {
                heartsObject.SetActive(false);
            }
            else {
                heartsObject.SetActive(true);
                for (int j = 1; j <= 3; j++) {
                    heartsObject.transform.Find("Heart " + j).gameObject.SetActive(StaticVariables.heartsLeftPerLevel[i] >= j);
                }
            }
        }
    }

    // ---------------------------------------------------
    //FUNCTIONS USED FOR UNLOCKING AND DISPLAYING ACHIEVEMENTS
    // ---------------------------------------------------

    public void _btnChangeColor(Material mat) {
        //when the player pushes one of the buttons to apply a different skin from the achievements screen
        if (StaticVariables.playerMat != mat) {
            StaticVariables.playerMat = mat;
            StaticVariables.hasChangedColorYet = true;
            updatePlayerColor();
            updateAchievementButtons();
            save();
        }
    }

    private void updatePlayerColor() {
        //changes the player's skin to match the currently-selected material
        if (StaticVariables.hasChangedColorYet) {
            player.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            rightModel.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            leftModel.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            showAchievementPlayerModels();
        }
    }

    private void updateAchievementButtons() {
        //updates the visuals on the achievements screen to reflect which skin is currently equipped
        //also hides the achievements the player should not yet be able to view
        foreach (Transform child in achievementsCanvas.transform) {
            if (child.gameObject.name.Contains("Color")) {
                string searchingFor = "Player_" + child.gameObject.name.Split(' ')[1];
                foreach (Material mat in StaticVariables.materialsList) {
                    if (mat.name == searchingFor) {
                        child.Find("Button").Find("Icon").gameObject.SetActive(mat == StaticVariables.playerMat);
                    }
                }
            }
        }

        hideAchievements();
    }

    private void showAchievementPlayerModels() {
        //shows or hides the player models shown on the achievement screen, based if the player is currently on the achievement screen or not
        rightModel.gameObject.SetActive((!StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
        leftModel.gameObject.SetActive((StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
    }
    
    private void lockAchievements() {
        //locks or unlocks all achievements based on if the player fulfilled the necessary requirements
        foreach (Transform child in achievementsCanvas.transform) {
            if (child.gameObject.name.Contains("Color")) {
                setAchievementLock(child, false);
                string req = child.Find("Requirement").gameObject.GetComponent<Text>().text;
                foreach (string achvt in StaticVariables.achievementsUnlocked.Split('-')) {
                    if (achvt == req) {
                        setAchievementLock(child, true);
                    }
                }
            }
        }
        hideAchievements();
    }

    private void hideAchievements() {
        //breaks all of the achievements into categories
        //each category has a specific set of requirements before the player can view the achievement criteria
        //hides the criteria for all achievements the player has not met the viewing prerequisites for
        string[] alwaysShowing = { "Blue", "Purple", "Yellow", "Orange" };
        string[] untilBeaten36 = { "Bronze", "Silver", "Gold", "White" };
        string[] untilBeaten24 = { "Pink" };
        string[] untilAllOthers1 = { "Green" };
        string[] untilAllOthers2 = { "Black" };
        string[] untilAllOthers3 = { "Checkered" };

        List<string> alreadyUnlocked = new List<string>(); //added here if the player already has them
        List<string> visible = new List<string>(); //if the player does not have them but if they are visible
        List<string> notVisible = new List<string>(); //if the player cannot yet see them

        foreach (string s in alwaysShowing) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, true); }
        foreach (string s in untilBeaten36) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, StaticVariables.levelsBeaten >= 36); }
        foreach (string s in untilBeaten24) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, StaticVariables.levelsBeaten >= 24); }
        foreach (string s in untilAllOthers1) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, (visible.Count == 0 && notVisible.Count == 0)); }
        foreach (string s in untilAllOthers2) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, (visible.Count == 0 && notVisible.Count == 0)); }
        foreach (string s in untilAllOthers3) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, (visible.Count == 0 && notVisible.Count == 0)); }

        foreach (string s in alreadyUnlocked) {
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement").gameObject.SetActive(true);
            achievementsCanvas.transform.Find("Color " + s).Find("Reward").gameObject.SetActive(true);
            achievementsCanvas.transform.Find("Color " + s).Find("Button").gameObject.SetActive(true);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 2").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 3").gameObject.SetActive(false);

        }
        foreach (string s in visible) {
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Reward").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Button").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 2").gameObject.SetActive(true);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 3").gameObject.SetActive(false);

        }
        foreach (string s in notVisible) {
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Reward").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Button").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 2").gameObject.SetActive(false);
            achievementsCanvas.transform.Find("Color " + s).Find("Requirement 3").gameObject.SetActive(true);
        }
    }

    private bool hasPlayerUnlockedAchievement(string color) {
        //returns true if the player has completed an achievement for a specified skin
        string requirement = achievementsCanvas.transform.Find("Color " + color).Find("Requirement").GetComponent<Text>().text;
        foreach (string unlock in StaticVariables.achievementsUnlocked.Split('-')) {
            if (unlock == requirement) { return true; }
        }
        return false;
    }

    private void addAchievementToList(List<string> alreadyUnlocked, List<string> visible, List<string> notVisible, string color, bool cond) {
        //used to help sort all of the achievements into the separate categories defined in hideAchievements
        if (hasPlayerUnlockedAchievement(color)) { alreadyUnlocked.Add(color); }
        else if (cond) { visible.Add(color); }
        else { notVisible.Add(color); }
    }

    private void setAchievementLock(Transform obj, bool cond) {
        //locks or unlocks an achievement
        obj.Find("Overlay").gameObject.SetActive(!cond);
    }

    private int countLevelsWith3Hearts() {
        //counts the total number of levels the player has beaten with 3 hearts left.
        //used in determining if a player has unlocked a category of achievements
        int count = 0;
        foreach (int hearts in StaticVariables.heartsLeftPerLevel) {
            if (hearts == 3) { count++; }
        }
        return count;
    }
    
    // ---------------------------------------------------
    //SAVING AND QUITTING FUNCTIONS
    // ---------------------------------------------------
    
    public void _btnQuit() {
        Application.Quit();
    }
    
    private void OnApplicationQuit() {
        save();
    }
    
    private void save() {
        //saves the player's data
        SaveSystem.SaveGame();
    }
}
