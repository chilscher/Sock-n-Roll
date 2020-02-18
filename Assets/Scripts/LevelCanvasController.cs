using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class LevelCanvasController : MonoBehaviour{

    [Header("Heart Icon")]
    public GameObject heartPrefab;
    public float gapBetweenHearts = 60f;

    [Header("Canvases")]
    public GameObject ingameCanvas;
    public GameObject pausedCanvas;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    
    private UnityEngine.UI.Text ingameLevelNumberTextBox;
    private UnityEngine.UI.Text pausedeLevelNumberTextBox;
    private UnityEngine.UI.Text winLevelNumberTextBox;
    private UnityEngine.UI.Text loseLevelNumberTextBox;
    

    [Header("Pause/Resume Key")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Tutorials and Menus")]
    public bool showHealth = true;

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

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        nextLevelBtn = winCanvas.transform.Find("Next Level Button").gameObject;
        audioManager = FindObjectOfType<AudioManager>();
        joystick = ingameCanvas.transform.Find("Fixed Joystick").gameObject;
        punchButton = ingameCanvas.transform.Find("Punch Button").gameObject;
        rollButton = ingameCanvas.transform.Find("Roll Button").gameObject;
        pauseButton = ingameCanvas.transform.Find("Pause Button").gameObject;
        levelNumberText = ingameCanvas.transform.Find("Level Number").gameObject;
        heartsPosition = ingameCanvas.transform.Find("Hearts Position").gameObject;
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

        if (showHealth) { drawAllHearts(); }
        

        getNextLevelName();
        doesNextLevelExist = Application.CanStreamedLevelBeLoaded(nextLevelName);
        hideNextLevelButton();
        
        if (!audioManager.isPlaying("Level Theme")) {
            audioManager.fadeIn("Level Theme");
        }

        ingameCanvas.transform.Find("Hearts Position").gameObject.SetActive(false);
        updatePlayerColor();
    }

    
    void Update(){
        if (showHealth && hearts.Count != player.getHP()) { redrawHearts(); }

        hitPauseKey();
        win();
        lose();

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
            h.transform.SetParent(canvas.transform);
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

            pauseButton.SetActive(false);
            pausedCanvas.SetActive(false);
            loseCanvas.SetActive(false);
            audioManager.stopAll();
            audioManager.play("Victory Jingle");
            StaticVariables.usingJoystick = false;
            if (StaticVariables.levelsBeaten < levelNumber) {
                StaticVariables.levelsBeaten = levelNumber;
            }
            if (player.HP == player.startingHP) {
                //something happens here
                //print(StaticVariables.levelsBeatenWithFullHP);
                string[] levels = StaticVariables.levelsBeatenWithFullHP.Split(',');
                bool hasBeatenBefore = false;
                foreach (string level in levels) {
                    if (level == (levelNumber + "")) {
                        hasBeatenBefore = true;
                    }
                }
                if (!hasBeatenBefore) {
                    StaticVariables.levelsBeatenWithFullHP += "," + levelNumber;
                }

                //print(StaticVariables.levelsBeatenWithFullHP);
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
}
