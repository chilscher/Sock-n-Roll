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
    //private GameObject playRevPlayerBtn;
    //private GameObject playRevEnemyBtn;
    //private GameObject creditsRevPlayerBtn;
    //private GameObject creditsRevEnemyBtn;

    private Player player;
    private Enemy enemy;
    private AudioManager audioManager;

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
        audioManager = FindObjectOfType<AudioManager>();
        homeRevPlayerBtn = homeCanvas.transform.Find("Revive Player Button").gameObject;
        homeRevEnemyBtn = homeCanvas.transform.Find("Revive Enemy Button").gameObject;
        //playRevPlayerBtn = playCanvas.transform.Find("Revive Player Button").gameObject;
        //playRevEnemyBtn = playCanvas.transform.Find("Revive Enemy Button").gameObject;
        //creditsRevPlayerBtn = creditsCanvas.transform.Find("Revive Player Button").gameObject;
        //creditsRevEnemyBtn = creditsCanvas.transform.Find("Revive Enemy Button").gameObject;
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
            //updatePlayerColor();
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

        //print(StaticVariables.achievementsUnlocked);
    }

    private void Update() {
        //print(StaticVariables.hasBeenOutOfBounds);
        hitBackKey();
        if (canReviveEnemy) {
            homeRevEnemyBtn.SetActive(enemy.getIsDead());
            //playRevEnemyBtn.SetActive(enemy.getIsDead());
            //creditsRevEnemyBtn.SetActive(enemy.getIsDead());
        }
        homeRevPlayerBtn.SetActive(player.getIsDead());
        //playRevPlayerBtn.SetActive(player.getIsDead());
        //creditsRevPlayerBtn.SetActive(player.getIsDead());
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
            if (homeCanvas.activeSelf == false) {
                homeCanvas.SetActive(true);
                playCanvas.SetActive(false);
                creditsCanvas.SetActive(false);
                achievementsCanvas.SetActive(false);
                showAchievementPlayerModels();
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
        SaveSystem.SaveGame();
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
            //achievementsCanvas.transform.Find("Duplicate Player Right").Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
            //achievementsCanvas.transform.Find("Duplicate Player Left").Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
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
        //achievementsCanvas.transform.Find("Duplicate Player Right").gameObject.SetActive(!StaticVariables.joystickOnRight);
        //achievementsCanvas.transform.Find("Duplicate Player Left").gameObject.SetActive(StaticVariables.joystickOnRight);

        rightModel.gameObject.SetActive((!StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
        leftModel.gameObject.SetActive((StaticVariables.joystickOnRight) && (achievementsCanvas.activeSelf));
    }

    private void lockLevels() {
        //print(StaticVariables.levelsBeaten);
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
                //print(child);
                setAchievementLock(child, false);
                //setAchievementLock(child, true);
                string req = child.Find("Requirement").gameObject.GetComponent<Text>().text;
                foreach (string achvt in StaticVariables.achievementsUnlocked.Split('-')) {
                    if (achvt == req) {
                        setAchievementLock(child, true);
                    }
                }
                /*
                switch (req) {
                    case "BEAT LEVEL 18":
                        setAchievementLock(child, StaticVariables.levelsBeaten >= 18);
                        break;
                    case "BEAT LEVEL 30":
                        setAchievementLock(child, StaticVariables.levelsBeaten >= 30);
                        break;
                    case "BEAT ALL LEVELS":
                        setAchievementLock(child, StaticVariables.levelsBeaten >= 36);
                        break;
                    case "GET OUT OF BOUNDS":
                        setAchievementLock(child, StaticVariables.hasBeenOutOfBounds);
                        break;
                    case "BEAT ANY 18 LEVELS\nWITH 3 HEARTS LEFT":
                        setAchievementLock(child, countLevelsWith3Hearts() >= 18);
                        break;
                    case "BEAT ANY 30 LEVELS\nWITH 3 HEARTS LEFT":
                        setAchievementLock(child, countLevelsWith3Hearts() >= 30);
                        break;
                    case "BEAT ALL LEVELS\nWITH 3 HEARTS LEFT":
                        setAchievementLock(child, countLevelsWith3Hearts() >= 36);
                        break;
                }
                */
            }
        }
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

}
