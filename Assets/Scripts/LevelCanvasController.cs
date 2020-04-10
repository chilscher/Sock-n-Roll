using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelCanvasController : MonoBehaviour{

    [Header("Heart Icon")]
    public GameObject heartPrefab;
    public float gapBetweenHearts = 60f;

    [Header("Canvases")]
    public GameObject ingameCanvas;
    public GameObject pausedCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    public GameObject achievementCanvas;
    
    private UnityEngine.UI.Text ingameLevelNumberTextBox;
    private UnityEngine.UI.Text pausedeLevelNumberTextBox;
    private UnityEngine.UI.Text winLevelNumberTextBox;
    private UnityEngine.UI.Text loseLevelNumberTextBox;
    

    [Header("Pause/Resume Key")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Tutorials and Menus")]
    public bool showHealth = true;

    [Header("Achievement Zoom-In Time")]
    public float achievementTime = 0.5f;

    private Player player;
    private List<GameObject> hearts;
    private string nextLevelName;
    private bool isPaused = false;
    private bool isPlaying = true;
    private bool doesNextLevelExist;
    private bool onWinScreenYet = false;
    private bool onLoseScreenYet = false;
    private AudioManager audioManager;


    private GameObject nextLevelBtn;


    private GameObject joystick;
    private GameObject punchButton;
    private GameObject rollButton;
    private GameObject pauseButton;
    private GameObject levelNumberText;
    private GameObject heartsPosition;
    private int levelNumber;
    private bool fadingInAchievement = false;
    private float fadingAchievementTimer = 0f;
    private string achievementsToShow = "";

    void Start() {

        ingameCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        pausedCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        winCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        loseCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        achievementCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        nextLevelBtn = winCanvas.transform.Find("Next Level Button").gameObject;
        audioManager = FindObjectOfType<AudioManager>();
        joystick = ingameCanvas.transform.Find("Fixed Joystick").gameObject;
        punchButton = ingameCanvas.transform.Find("Punch Button").gameObject;
        rollButton = ingameCanvas.transform.Find("Roll Button").gameObject;
        pauseButton = ingameCanvas.transform.Find("Pause Button").gameObject;
        levelNumberText = ingameCanvas.transform.Find("Level Number").gameObject;
        //heartsPosition = ingameCanvas.transform.Find("Hearts Position").gameObject;
        heartsPosition = ingameCanvas.transform.Find("Level Number").Find("Hearts Position").gameObject;
        flipControls();
        levelNumber = Int32.Parse(SceneManager.GetActiveScene().name.Split(' ')[1]);
        if (levelNumber < 3) { 
            rollButton.SetActive(false);
        }
        setLevelNumber();

        ingameCanvas.SetActive(true);
        pausedCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
        achievementCanvas.SetActive(false);

        if (showHealth) { drawAllHearts(); }
        

        getNextLevelName();
        doesNextLevelExist = Application.CanStreamedLevelBeLoaded(nextLevelName);
        hideNextLevelButton();
        
        if (!audioManager.isPlaying("Level Theme")) {
            audioManager.fadeIn("Level Theme");
        }

        //ingameCanvas.transform.Find("Hearts Position").gameObject.SetActive(false);
        ingameCanvas.transform.Find("Level Number").Find("Hearts Position").gameObject.SetActive(false);
        updatePlayerColor();
        StaticVariables.punchCount = 0;
        StaticVariables.enemiesPunched = new List<Enemy>();

    }

    
    void Update(){
        if (showHealth && hearts.Count != player.getHP()) { redrawHearts(); }

        hitPauseKey();
        win();
        lose();

        if (fadingInAchievement) {
            fadingAchievementTimer -= Time.deltaTime;
            if (fadingAchievementTimer < 0f) { fadingAchievementTimer = 0f; }
            float fadePercent = 1 - (fadingAchievementTimer / achievementTime);

            achievementCanvas.transform.Find("Achievement").localScale = new Vector3(fadePercent, fadePercent, fadePercent);
            achievementCanvas.transform.Find("Double Achievement").localScale = new Vector3(fadePercent, fadePercent, fadePercent);
            achievementCanvas.transform.Find("Triple Achievement").localScale = new Vector3(fadePercent, fadePercent, fadePercent);
            if (fadingAchievementTimer <= 0f) {
                fadingInAchievement = false;
            }
        }

        if (StaticVariables.isOutOfBounds && !StaticVariables.hasBeenOutOfBounds) {
            StaticVariables.hasBeenOutOfBounds = true;
            string t = "GET OUT OF BOUNDS";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        
        if (StaticVariables.enemiesPunchedSimultaneously.Count >= 3) {
            string t = "HIT 3 ENEMIES\nWITH ONE PUNCH";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }

        

        if (achievementsToShow != "") {
            showAchievements();
            achievementsToShow = "";
            save();
        }

    }

    private void flipControls() {
        if (StaticVariables.joystickOnRight) {
            Vector2 pos;
            
            pos = joystick.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            joystick.GetComponent<RectTransform>().anchoredPosition = pos;

            pos = punchButton.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            punchButton.GetComponent<RectTransform>().anchoredPosition = pos;

            pos = rollButton.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            rollButton.GetComponent<RectTransform>().anchoredPosition = pos;

            pos = pauseButton.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            pauseButton.GetComponent<RectTransform>().anchoredPosition = pos;
            
            pos = levelNumberText.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            levelNumberText.GetComponent<RectTransform>().anchoredPosition = pos;
            
            pos = heartsPosition.GetComponent<RectTransform>().anchoredPosition;
            pos.x *= -1;
            heartsPosition.GetComponent<RectTransform>().anchoredPosition = pos;

        }
    }

    private void drawHearts(GameObject canvas) {
        int heartAmount = player.getHP();
        hearts = new List<GameObject>();
        Vector2 heartPos = heartsPosition.GetComponent<RectTransform>().anchoredPosition;
        float firstX = heartPos.x;
        float firstY = heartPos.y;
        float firstZ = 0f;

        for (int i = 0; i<heartAmount; i++) {
            float x = firstX + ((gapBetweenHearts + heartPrefab.gameObject.transform.GetComponent<RectTransform>().rect.width) * (i - 1));
            float y = firstY;
            float z = firstZ;

            Vector3 pos = new Vector3(x,y,z);
            GameObject h = Instantiate(heartPrefab);
            h.transform.SetParent(canvas.transform.Find("Level Number"));
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            h.transform.localScale *= ingameCanvas.transform.localScale.x;
            hearts.Add(h);
        }
    }

    private void redrawHearts() {
        foreach (GameObject heart in hearts) { Destroy(heart); }
        hearts = new List<GameObject>();
        drawAllHearts();
    }

    private void drawAllHearts() {
        drawHearts(ingameCanvas);
    }
    


    private void pause() {
        if (!StaticVariables.pausedFromAchievements && !player.hasWonYet()) {
            isPaused = true;
            isPlaying = false;
            Time.timeScale = 0f;
            pausedCanvas.SetActive(true);

            pauseButton.SetActive(false);
            winCanvas.SetActive(false);
            loseCanvas.SetActive(false);
            if (StaticVariables.globalAudioScale > 0) {
                audioManager.pause();
            }
        }
        
        
    }

    private void hitPauseKey() {
        if (Input.GetKeyDown(pauseKey)) {
            if (isPlaying) { pause(); }
            else if (isPaused) { resume(); }
        }
    }
    

    private void win() {
        if (player.hasWonYet() && !onWinScreenYet) {
            onWinScreenYet = true;
            isPaused = false;
            isPlaying = false;
            winCanvas.SetActive(true);
            
            pausedCanvas.SetActive(false);
            loseCanvas.SetActive(false);
            audioManager.stopAll();
            audioManager.play("Victory Jingle");
            StaticVariables.usingJoystick = false;
            if (StaticVariables.levelsBeaten < levelNumber) {
                StaticVariables.levelsBeaten = levelNumber;
            }
            if (StaticVariables.heartsLeftPerLevel[levelNumber - 1] < player.HP) {
                StaticVariables.heartsLeftPerLevel[levelNumber - 1] = player.HP;
            }
            checkAchievementsOnWin();
            save();
        }
    }

    private void checkAchievementsOnWin() {

        if (levelNumber == 18) {
            string t = "BEAT LEVEL 18";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (levelNumber == 30) {
            string t = "BEAT LEVEL 30";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (levelNumber == 36) {
            string t = "BEAT ALL LEVELS";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (countLevelsWith3Hearts() == 18) {
            string t = "BEAT ANY 18 LEVELS\nWITH 3 HEARTS LEFT";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (countLevelsWith3Hearts() == 30) {
            string t = "BEAT ANY 30 LEVELS\nWITH 3 HEARTS LEFT";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (countLevelsWith3Hearts() == 36) {
            string t = "BEAT ALL LEVELS\nWITH 3 HEARTS LEFT";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (levelNumber == 36) {
            string t = "BEAT LEVEL 36\nWITHOUT PUNCHING";
            if (!checkIfAchievementUnlockedYet(t) && StaticVariables.punchCount == 0) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        if (levelNumber == 24) {
            string t = "DEFEAT ALL ENEMIES IN\nLEVEL 24 WITH PUNCHES";
            if (!checkIfAchievementUnlockedYet(t) && StaticVariables.enemiesPunched.Count >= 9) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }

        }

    }

    private void lose() {
        if (player.hasLostYet() && !onLoseScreenYet) {
            onLoseScreenYet = true;
            isPaused = false;
            isPlaying = false;
            loseCanvas.SetActive(true);

            pauseButton.SetActive(false);
            pausedCanvas.SetActive(false);
            winCanvas.SetActive(false);
            audioManager.fadeIn("Defeat Jingle");

            if ((StaticVariables.lastLevelLost == StaticVariables.secondLastLevelLost) && (StaticVariables.lastLevelLost == levelNumber)){
                string t = "LOSE THE SAME LEVEL\n3 TIMES IN A ROW";
                if (!checkIfAchievementUnlockedYet(t)) {
                    achievementsToShow += (t + "-");
                    addStringToAchievements(t);
                }
            }
            else {
                StaticVariables.secondLastLevelLost = StaticVariables.lastLevelLost;
                StaticVariables.lastLevelLost = levelNumber;
            }
            save();
        }
    }

    private void resume() {
        isPaused = false;
        isPlaying = true;
        Time.timeScale = 1f;

        if (levelNumber > 2) { rollButton.SetActive(true); }
        punchButton.SetActive(true);
        joystick.SetActive(true);
        pauseButton.SetActive(true);

        pausedCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
        if (StaticVariables.globalAudioScale > 0) {
            audioManager.resume();
        }
        
    }

    private void quit() {
        if (StaticVariables.globalAudioScale > 0) {
            if (Time.timeScale == 0) { audioManager.resumeWithMusicFadeout(); } //if the game was paused, then fade out the music from its already quieter level
            else { audioManager.fadeOutAll(); } //if the game was not paused, fade out the music like normal
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void restart() {
        audioManager.stopPausableSounds();
        Time.timeScale = 1f;
        if (StaticVariables.globalAudioScale > 0) {
            audioManager.resume();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void nextLevel() {
        if (doesNextLevelExist) {
            if (StaticVariables.globalAudioScale > 0) {
                audioManager.fadeOutAll();
                audioManager.resume();
            }
            SceneManager.LoadScene(nextLevelName);
        }
    }

    private void getNextLevelName() {
        string name = SceneManager.GetActiveScene().name;
        string[] words = name.Split(' ');
        string num = words[words.Length - 1];
        int n = int.Parse(num) + 1;
        words[words.Length - 1] = n.ToString();
        string newName = string.Join(" ", words);
        nextLevelName = newName;
    }

    private void hideNextLevelButton() {
        if (!doesNextLevelExist) { nextLevelBtn.SetActive(false); }
    }

    private void setLevelNumber() {
        Text tb = levelNumberText.transform.Find("Level number").GetComponent<Text>();
        string name = "Level " + levelNumber;
        tb.text = name.ToUpper();
    }

    public void _btnPause() { pause(); }
    public void _btnResume() { resume(); }
    public void _btnQuit() { quit(); }
    public void _btnRestart() { restart(); }
    public void _btnNextLevel() { nextLevel(); }


    private void OnApplicationQuit() {
        SaveSystem.SaveGame();
    }

    private void updatePlayerColor() {
        if (StaticVariables.hasChangedColorYet) {
            player.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
        }
    }

    private void showAchievements() {
        StaticVariables.pausedFromAchievements = true;
        audioManager.play("Achievement");
        achievementsToShow = achievementsToShow.Remove(achievementsToShow.Length - 1, 1);
        string[] achvs = achievementsToShow.Split('-');
        achievementCanvas.SetActive(true);
        fadingInAchievement = true;
        fadingAchievementTimer = achievementTime;
        achievementCanvas.transform.Find("Achievement").localScale = new Vector3(0f, 0f, 0f);
        achievementCanvas.transform.Find("Double Achievement").localScale = new Vector3(0f, 0f, 0f);
        achievementCanvas.transform.Find("Triple Achievement").localScale = new Vector3(0f, 0f, 0f);

        if (achvs.Length == 1) {
            achievementCanvas.transform.Find("Achievement").gameObject.SetActive(true);
            achievementCanvas.transform.Find("Double Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Triple Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Achievement").Find("Text").GetComponent<Text>().text = achvs[0];
        }
        else if (achvs.Length == 2){
            achievementCanvas.transform.Find("Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Double Achievement").gameObject.SetActive(true);
            achievementCanvas.transform.Find("Triple Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Double Achievement").Find("Text 1").GetComponent<Text>().text = achvs[0];
            achievementCanvas.transform.Find("Double Achievement").Find("Text 2").GetComponent<Text>().text = achvs[1];
        }
        else if (achvs.Length == 3) {
            achievementCanvas.transform.Find("Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Double Achievement").gameObject.SetActive(false);
            achievementCanvas.transform.Find("Triple Achievement").gameObject.SetActive(true);
            achievementCanvas.transform.Find("Triple Achievement").Find("Text 1").GetComponent<Text>().text = achvs[0];
            achievementCanvas.transform.Find("Triple Achievement").Find("Text 2").GetComponent<Text>().text = achvs[1];
            achievementCanvas.transform.Find("Triple Achievement").Find("Text 3").GetComponent<Text>().text = achvs[2];

        }
    }

    public void _btnCloseAchievement() {
        achievementCanvas.SetActive(false);
        StaticVariables.pausedFromAchievements = false;
    }

    public void _btnGoToAchievements() {
        StaticVariables.goingToAchievements = true;

        StaticVariables.pausedFromAchievements = false;
        quit();
    }


    private int countLevelsWith3Hearts() {
        int count = 0;
        foreach (int hearts in StaticVariables.heartsLeftPerLevel) {
            if (hearts == 3) { count++; }
        }
        return count;
    }

    private bool checkIfAchievementUnlockedYet(string text) {
        foreach(string achvt in StaticVariables.achievementsUnlocked.Split('-')) {
            if (achvt == text) { return true; }
        }
        return false;
    }

    private void addStringToAchievements(string text) {
        StaticVariables.achievementsUnlocked += ("-" + text);
    }

    private void save() {
        SaveSystem.SaveGame();
    }
}
