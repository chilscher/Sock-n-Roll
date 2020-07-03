//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelCanvasController : MonoBehaviour{
    //controls the operation of all levels in the game, such as the visual display of buttons and information on canvases, transitioning between scenes, and more
    
    //visual display of player hearts on screen
    [Header("Heart Icon")]
    public GameObject heartPrefab;
    public float gapBetweenHearts = 60f;

    [Header("Canvases")]
    public GameObject ingameCanvas;
    public GameObject pausedCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    public GameObject achievementCanvas;

    //the keyboard key to pause the game, for use in testing
    [Header("Pause/Resume Key")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Tutorials")]
    public bool showHealth = true; //determines whether to show the player's health on screen

    [Header("Achievement Zoom-In Time")]
    public float achievementTime = 0.5f; //time for the achievement pop-up to appear
    //other variables relating to showing achievements
    private bool fadingInAchievement = false;
    private float fadingAchievementTimer = 0f;
    private string achievementsToShow = "";

    private Player player;
    private List<GameObject> hearts; //the on-screen hearts

    //variables dealing with the ability to jump to the next level from the win canvas
    private string nextLevelName;
    private bool doesNextLevelExist;
    private GameObject nextLevelBtn;

    //variables determining the state of the game
    private bool isPaused = false;
    private bool isPlaying = true;
    private bool onWinScreenYet = false;
    private bool onLoseScreenYet = false;

    private AudioManager audioManager;
    private int levelNumber;

    //on-screen controls and display
    private GameObject joystick;
    private GameObject punchButton;
    private GameObject rollButton;
    private GameObject pauseButton;
    private GameObject levelNumberText;
    private GameObject heartsPosition;

    void Start() {
        //resize canvases to fit the display screen size
        ingameCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        pausedCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        winCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        loseCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);
        achievementCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(900, StaticVariables.resolutionMultiplier);

        //set variables
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        nextLevelBtn = winCanvas.transform.Find("Next Level Button").gameObject;
        audioManager = FindObjectOfType<AudioManager>();
        joystick = ingameCanvas.transform.Find("Fixed Joystick").gameObject;
        punchButton = ingameCanvas.transform.Find("Punch Button").gameObject;
        rollButton = ingameCanvas.transform.Find("Roll Button").gameObject;
        pauseButton = ingameCanvas.transform.Find("Pause Button").gameObject;
        levelNumberText = ingameCanvas.transform.Find("Level Number").gameObject;
        heartsPosition = ingameCanvas.transform.Find("Level Number").Find("Hearts Position").gameObject;

        //flip the on-screen controls, if that is toggled in settings
        flipControls();

        //get the current level number
        levelNumber = Int32.Parse(SceneManager.GetActiveScene().name.Split(' ')[1]);
        if (levelNumber < 3) { 
            rollButton.SetActive(false); //the player cannot roll in levels 1 or 2
        }
        setLevelNumber(); //display the current level number on screen

        //show only the main game canvas, hide all others
        ingameCanvas.SetActive(true);
        pausedCanvas.SetActive(false);
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
        achievementCanvas.SetActive(false);
        //show player hearts
        if (showHealth) { drawHearts(); }
        
        //determine if the next level exists, and set up the "next level" button appropriately
        getNextLevelName();
        doesNextLevelExist = Application.CanStreamedLevelBeLoaded(nextLevelName);
        hideNextLevelButton();
        
        //play the level theme
        if (!audioManager.isPlaying("Level Theme")) {
            audioManager.fadeIn("Level Theme");
        }

        //hide the hearts positioning object
        ingameCanvas.transform.Find("Level Number").Find("Hearts Position").gameObject.SetActive(false);

        //apply the current skin
        updatePlayerColor();

        //set variables for some achievements
        StaticVariables.punchCount = 0;
        StaticVariables.enemiesPunched = new List<Enemy>();

    }

    
    void Update(){
        //show the appropriate amount of player hp left
        if (showHealth && hearts.Count != player.getHP()) { redrawHearts(); }

        //handle pausing, winning, or losing, based on player variables or inputs
        hitPauseKey();
        win();
        lose();

        //handle the achievement pop-up, if the player has just unlocked an achievement
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

        //if this is the player's first time out of bounds, unlock the relevant achievement
        if (StaticVariables.isOutOfBounds && !StaticVariables.hasBeenOutOfBounds) {
            StaticVariables.hasBeenOutOfBounds = true;
            string t = "GET OUT OF BOUNDS";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        
        //check to see if the player unlocked an achievement
        if (StaticVariables.enemiesPunchedSimultaneously.Count >= 3) {
            string t = "HIT 3 ENEMIES\nWITH ONE PUNCH";
            if (!checkIfAchievementUnlockedYet(t)) {
                achievementsToShow += (t + "-");
                addStringToAchievements(t);
            }
        }
        
        //show the achievements pop-up if there are any new achievements the player just unlocked
        if (achievementsToShow != "") {
            showAchievements();
            achievementsToShow = "";
            save();
        }
    }

    // ---------------------------------------------------
    //BUTTON-PUSHING FUNCTIONS
    // ---------------------------------------------------
    
    public void _btnPause() { pause(); }
    public void _btnResume() { resume(); }
    public void _btnQuit() { quit(); }
    public void _btnRestart() { restart(); }
    public void _btnNextLevel() { nextLevel(); }

    public void _btnCloseAchievement() {
        //return to the game, and hide the achievement pop-up
        achievementCanvas.SetActive(false);
        StaticVariables.pausedFromAchievements = false;
    }

    public void _btnGoToAchievements() {
        //leave the current level and jump right to the achievement screen on the main menu
        StaticVariables.goingToAchievements = true;

        StaticVariables.pausedFromAchievements = false;
        quit();
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT DISPLAY VISUALS
    // ---------------------------------------------------
    
    private void flipControls() {
        //flips all controls to the other side of the screen
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

    private void drawHearts() {
        //show the current number of hearts on the ingame canvas
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
            h.transform.SetParent(ingameCanvas.transform.Find("Level Number"));
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            h.transform.localScale *= ingameCanvas.transform.localScale.x;
            hearts.Add(h);
        }
    }

    private void redrawHearts() {
        //remove all hearts on screen, and draw new ones. used when the player's number of hearts changes
        foreach (GameObject heart in hearts) { Destroy(heart); }
        hearts = new List<GameObject>();
        drawHearts();
    }

    private void hideNextLevelButton() {
        //if the next level does not exist, hide the next-level button from the win canvas
        if (!doesNextLevelExist) { nextLevelBtn.SetActive(false); }
    }

    private void setLevelNumber() {
        //sets the text that denotes which level this is
        Text tb = levelNumberText.transform.Find("Level number").GetComponent<Text>();
        string name = "Level " + levelNumber;
        tb.text = name.ToUpper();
    }

    private void showAchievements() {
        //when the player has unlocked a new achievement, enter a pseudo-pause state
        //this is different from the normal pause state because time continues in this state
        //that is why a lot of other scripts reference StaticVariables.pausedFromAchievements to continue operating

        //compiles a list of achievements that the player has just unlocked, with a max of 3
        //begin the achievement pop-up process and start the achievement jingle
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
        else if (achvs.Length == 2) {
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

    // ---------------------------------------------------
    //FUNCTIONS THAT PAUSE/RESUME THE GAME
    // ---------------------------------------------------
    
    private void pause() {
        //pause the game. This involves stopping the passage of time for the game
        //this is different from when the game is paused for an achievement pop-up
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
        //if the player presses the pause key on their keyboard, pause the game
        //used for testing
        if (Input.GetKeyDown(pauseKey)) {
            if (isPlaying) { pause(); }
            else if (isPaused) { resume(); }
        }
    }

    private void resume() {
        //continue playing, specifically resume from the pause menu
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

    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH WINNING AND LOSING A LEVEL
    // ---------------------------------------------------
    
    private void win() {
        //if the player object has fulfilled the win conditions, start the win process
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
        //checks a bunch of achievements, called when the player wins the level
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
        //if the player has lost, start the losing process
        if (player.hasLostYet() && !onLoseScreenYet) {
            onLoseScreenYet = true;
            isPaused = false;
            isPlaying = false;
            loseCanvas.SetActive(true);

            pauseButton.SetActive(false);
            pausedCanvas.SetActive(false);
            winCanvas.SetActive(false);
            audioManager.fadeIn("Defeat Jingle");

            //check to see if the player has just unlocked an achievement
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

    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH QUITTING, RESTARTING THE LEVEL, OR PROCEEDING TO THE NEXT LEVEL
    // ---------------------------------------------------

    private void quit() {
        //go back to the main menu
        if (StaticVariables.globalAudioScale > 0) {
            if (Time.timeScale == 0) { audioManager.resumeWithMusicFadeout(); } //if the game was paused, then fade out the music from its already quieter level
            else { audioManager.fadeOutAll(); } //if the game was not paused, fade out the music like normal
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void restart() {
        //start the current level over again
        audioManager.stopPausableSounds();
        Time.timeScale = 1f;
        if (StaticVariables.globalAudioScale > 0) {
            audioManager.resume();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void nextLevel() {
        //proceed to the next level
        if (doesNextLevelExist) {
            if (StaticVariables.globalAudioScale > 0) {
                audioManager.fadeOutAll();
                audioManager.resume();
            }
            SceneManager.LoadScene(nextLevelName);
        }
    }

    private void getNextLevelName() {
        //get the name of the next level, by incrementing the current level by 1
        string name = SceneManager.GetActiveScene().name;
        string[] words = name.Split(' ');
        string num = words[words.Length - 1];
        int n = int.Parse(num) + 1;
        words[words.Length - 1] = n.ToString();
        string newName = string.Join(" ", words);
        nextLevelName = newName;
    }
    
    private void OnApplicationQuit() {
        SaveSystem.SaveGame();
    }

    private void save() {
        SaveSystem.SaveGame();
    }

    // ---------------------------------------------------
    //OTHER MISCELLANEOUS FUNCTIONS: APPLYING THE PLAYER SKIN AND CALCULATING ACHIEVEMENT PROGRESS
    // ---------------------------------------------------
    
    private void updatePlayerColor() {
        //change the player's color to match whichever they had picked from the achievements menu
        if (StaticVariables.hasChangedColorYet) {
            player.transform.Find("Model").GetComponent<Renderer>().sharedMaterial = StaticVariables.playerMat;
        }
    }
    
    private int countLevelsWith3Hearts() {
        //count how many levels the player has beaten with 3 hearts remaining, used in achievement calculation
        int count = 0;
        foreach (int hearts in StaticVariables.heartsLeftPerLevel) {
            if (hearts == 3) { count++; }
        }
        return count;
    }

    private bool checkIfAchievementUnlockedYet(string text) {
        //checks if the player has unlocked an achievement with the name TEXT
        foreach(string achvt in StaticVariables.achievementsUnlocked.Split('-')) {
            if (achvt == text) { return true; }
        }
        return false;
    }

    private void addStringToAchievements(string text) {
        //adds TEXT to the list of achievements the player has unlocked
        StaticVariables.achievementsUnlocked += ("-" + text);
    }
}
