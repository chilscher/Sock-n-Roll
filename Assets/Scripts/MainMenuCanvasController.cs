using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCanvasController : MonoBehaviour {

    [Header("Canvases")]
    public GameObject homeCanvas;
    public GameObject creditsCanvas;
    public GameObject playCanvas;
    public GameObject settingsCanvas;
    public GameObject achievementsCanvas;

    [Header("Options")]
    public bool canReviveEnemy = true;

    [Header("Back Key")]
    public KeyCode backKey = KeyCode.Escape;
    
    [Header("Player Color Materials")]
    public Material[] materialsList;

    [Header("Achievement Window Player Models")]
    public Player rightModel;
    public Player leftModel;

    private GameObject homeRevPlayerBtn;
    private GameObject homeRevEnemyBtn;

    private Player player;
    private Enemy enemy;
    private AudioManager audioManager;

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
        audioManager = FindObjectOfType<AudioManager>();
        homeRevPlayerBtn = homeCanvas.transform.Find("Revive Player Button").gameObject;
        homeRevEnemyBtn = homeCanvas.transform.Find("Revive Enemy Button").gameObject;
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);

        audioManager.fadeIn("Main Menu");
        
        StaticVariables.materialsList = materialsList;
        if (StaticVariables.isApplicationLaunchingFirstTime) {
            SaveSystem.LoadGame();
            StaticVariables.isApplicationLaunchingFirstTime = false;

            audioManager.applyGlobalVolume();
        }
        
        updatePlayerColor();
        updateFlipControlsText();
        updateShowControlsText();
        updateAudioText();
        updateAchievementButtons();
        showAchievementPlayerModels();
        if (StaticVariables.joystickOnRight) { flipControls(); }

        lockLevels();
        showHeartsOnLevels();
        lockAchievements();

        if (StaticVariables.goingToAchievements) {
            homeCanvas.SetActive(false);
            achievementsCanvas.SetActive(true);
            StaticVariables.goingToAchievements = false;
            showAchievementPlayerModels();
        }
        
    }

    private void Update() {
        hitBackKey();
        if (canReviveEnemy) { homeRevEnemyBtn.SetActive(enemy.getIsDead()); }
        homeRevPlayerBtn.SetActive(player.getIsDead());
    }

    public void _btnMainMenu() {
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        showAchievementPlayerModels();
    }

    public void _btnLevelSelect() {
        settingsCanvas.SetActive(false);
        playCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        showAchievementPlayerModels();
    }

    public void _btnCredits() {
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        showAchievementPlayerModels();
    }

    public void _btnQuit() {
        Application.Quit();
    }

    public void _btnRevivePlayer() {
        player.startReviving();
    }

    public void _btnReviveEnemy() {
        enemy.startReviving();
    }

    public void _btnLoadLevel(int x) {
        if (StaticVariables.levelsBeaten + 1 >= x) {
            audioManager.fadeOutAll();
            SceneManager.LoadScene("Level " + x);
        }
    }

    private void hitBackKey() {
        if (Input.GetKeyDown(backKey)) {
            if (creditsCanvas.activeSelf) {
                _btnSettings();
            }
            else if (achievementsCanvas.activeSelf) {
                _btnSettings();
            }
            else if (playCanvas.activeSelf) {
                _btnMainMenu();
            }
            else if (settingsCanvas.activeSelf) {
                _btnMainMenu();
            }
            else if (homeCanvas.activeSelf){
                _btnQuit();
            }
        }
    }

    public void _btnMute() {
        if (StaticVariables.globalAudioScale == 0f) {
            StaticVariables.globalAudioScale = 1f;
        }
        else {
            StaticVariables.globalAudioScale = 0f;
        }

        audioManager.stopAll();
        AudioListener.volume = StaticVariables.globalAudioScale;
        audioManager.play("Main Menu");
        updateAudioText();
    }

    public void _btnSettings() {
        settingsCanvas.SetActive(true);
        creditsCanvas.SetActive(false);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(false);
        showAchievementPlayerModels();
    }

   public void _btnFlipControls() {
        StaticVariables.joystickOnRight = !StaticVariables.joystickOnRight;
        updateFlipControlsText();
        flipControls();
    }

    private void updateFlipControlsText() {
        string s = "LEFT";
        if (StaticVariables.joystickOnRight) { s = "RIGHT"; }
        settingsCanvas.transform.Find("Flip Controls Button").Find("Text").gameObject.GetComponent<Text>().text = "FLIP CONTROLS:\nJOYSTICK ON " + s;
    }

    public void _btnShowControlsOnMenu() {
        StaticVariables.showControlsOnMenu = !StaticVariables.showControlsOnMenu;
        updateShowControlsText();
    }

    private void updateShowControlsText() {
        string s = "HIDDEN";
        if (StaticVariables.showControlsOnMenu) { s = "SHOWN"; }
        settingsCanvas.transform.Find("Controls On Menu").Find("Text").gameObject.GetComponent<Text>().text = "CONTROLS ON MENU:\n" + s;

        homeCanvas.transform.Find("Player Controls").gameObject.SetActive(StaticVariables.showControlsOnMenu);
    }

    private void updateAudioText() {
        string s = "MUTED";
        if (StaticVariables.globalAudioScale == 1f) { s = "PLAYING"; }
        settingsCanvas.transform.Find("Audio Button").Find("Text").gameObject.GetComponent<Text>().text = "AUDIO:\n" + s;
    }


    private void OnApplicationQuit() {
        save();
    }


    public void _btnChangeColor(Material mat) {
        if (StaticVariables.playerMat != mat) {
            StaticVariables.playerMat = mat;
            StaticVariables.hasChangedColorYet = true;
            updatePlayerColor();
            updateAchievementButtons();
        }
    }

    private void updatePlayerColor() {
        if (StaticVariables.hasChangedColorYet) {
            player.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            rightModel.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            leftModel.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            showAchievementPlayerModels();
        }
    }

    private void updateAchievementButtons() {
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

    public void _btnAchievements() {
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
        achievementsCanvas.SetActive(true);
        showAchievementPlayerModels();

    }


    private void flipControls() {
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

    private void showAchievementPlayerModels() {
        rightModel.gameObject.SetActive((!StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
        leftModel.gameObject.SetActive((StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
    }

    private void lockLevels() {
        for (int i = 1; i<= 36; i++) {
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
        for (int i = 0; i < 36; i++) {
            GameObject levelObject = playCanvas.transform.Find("Level " + (i + 1)).gameObject;
            GameObject heartsObject = levelObject.transform.Find("Hearts").gameObject;
            if (i > StaticVariables.levelsBeaten) {
                heartsObject.SetActive(false);
            }
            else {
                heartsObject.SetActive(true);
                for (int j = 1; j<=3; j++) {
                    heartsObject.transform.Find("Heart " + j).gameObject.SetActive(StaticVariables.heartsLeftPerLevel[i] >= j);
                }
            }
        }
    }

    private void lockAchievements() {
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
        string[] alwaysShowing = { "Blue", "Purple", "Yellow", "Orange" };
        string[] untilBeaten36 = { "Bronze", "Silver", "Gold", "White" };
        string[] untilBeaten24 = { "Pink" };
        string[] untilAllOthers1 = { "Green" };
        string[] untilAllOthers2 = { "Black" };
        string[] untilAllOthers3 = { "Checkered" };

        List<string> alreadyUnlocked = new List<string>(); //added here if the player already has them
        List<string> visible = new List<string>(); //if the player does not have them but if they are visible
        List<string> notVisible = new List<string>(); //if the player cannot yet see them
        
        foreach(string s in alwaysShowing) { addAchievementToList(alreadyUnlocked, visible, notVisible, s, true); }
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
        string requirement = achievementsCanvas.transform.Find("Color " + color).Find("Requirement").GetComponent<Text>().text;
        foreach (string unlock in StaticVariables.achievementsUnlocked.Split('-')) {
            if (unlock == requirement) { return true; }
        }
        return false;
    }

    private void addAchievementToList(List<string> alreadyUnlocked, List<string> visible, List<string> notVisible, string color, bool cond) {
        if (hasPlayerUnlockedAchievement(color)) { alreadyUnlocked.Add(color); }
        else if (cond) { visible.Add(color); }
        else { notVisible.Add(color); }
    }

    private void setAchievementLock(Transform obj, bool cond) {
        obj.Find("Overlay").gameObject.SetActive(!cond);
    }

    private int countLevelsWith3Hearts() {
        int count = 0;
        foreach(int hearts in StaticVariables.heartsLeftPerLevel) {
            if (hearts == 3) { count++; }
        }
        return count;
    }

    private void save() {
        SaveSystem.SaveGame();
    }

}
