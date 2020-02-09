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

    [Header("Options")]
    public bool canReviveEnemy = true;

    [Header("Back Key")]
    public KeyCode backKey = KeyCode.Escape;


    private GameObject homeRevPlayerBtn;
    private GameObject homeRevEnemyBtn;
    private GameObject playRevPlayerBtn;
    private GameObject playRevEnemyBtn;
    private GameObject creditsRevPlayerBtn;
    private GameObject creditsRevEnemyBtn;

    private Player player;
    private Enemy enemy;
    private AudioManager audioManager;

    //public bool isPressingButton = false;

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
        audioManager = FindObjectOfType<AudioManager>();
        homeRevPlayerBtn = homeCanvas.transform.Find("Revive Player Button").gameObject;
        homeRevEnemyBtn = homeCanvas.transform.Find("Revive Enemy Button").gameObject;
        playRevPlayerBtn = playCanvas.transform.Find("Revive Player Button").gameObject;
        playRevEnemyBtn = playCanvas.transform.Find("Revive Enemy Button").gameObject;
        creditsRevPlayerBtn = creditsCanvas.transform.Find("Revive Player Button").gameObject;
        creditsRevEnemyBtn = creditsCanvas.transform.Find("Revive Enemy Button").gameObject;
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);

        audioManager.fadeIn("Main Menu");


        //updateJoystickText();
        updateFlipControlsText();
    }

    private void Update() {
        hitBackKey();
        if (canReviveEnemy) {
            homeRevEnemyBtn.SetActive(enemy.getIsDead());
            playRevEnemyBtn.SetActive(enemy.getIsDead());
            creditsRevEnemyBtn.SetActive(enemy.getIsDead());
        }
        homeRevPlayerBtn.SetActive(player.getIsDead());
        playRevPlayerBtn.SetActive(player.getIsDead());
        creditsRevPlayerBtn.SetActive(player.getIsDead());

        //StaticVariables.isPressingButton = false;
    }

    public void _btnMainMenu() {
        settingsCanvas.SetActive(false);
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void _btnLevelSelect() {
        //StaticVariables.isPressingButton = true;
        settingsCanvas.SetActive(false);
        playCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void _btnCredits() {
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
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
        audioManager.fadeOutAll();
        SceneManager.LoadScene("Level " + x.ToString());
    }

    private void hitBackKey() {
        if (Input.GetKeyDown(backKey)) {
            if (homeCanvas.activeSelf == false) {
                homeCanvas.SetActive(true);
                playCanvas.SetActive(false);
                creditsCanvas.SetActive(false);
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
        
    }

    public void _btnSettings() {
        settingsCanvas.SetActive(true);
        creditsCanvas.SetActive(false);
        homeCanvas.SetActive(false);
        playCanvas.SetActive(false);
    }
    /*
    public void _btnJoystickToggle() {
        StaticVariables.useJoystick = !StaticVariables.useJoystick;
        updateJoystickText();
    }

    private void updateJoystickText() {
        string s = "DISABLED";
        if (StaticVariables.useJoystick) { s = "ENABLED"; }
        settingsCanvas.transform.Find("Toggle Joystick Button").Find("Text").gameObject.GetComponent<Text>().text = "TOGGLE JOYSTICK\nJOYSTICK " + s;
    }
    */
   public void _btnFlipControls() {
        StaticVariables.joystickOnRight = !StaticVariables.joystickOnRight;
        updateFlipControlsText();
    }

    private void updateFlipControlsText() {
        string s = "LEFT";
        if (StaticVariables.joystickOnRight) { s = "RIGHT"; }
        settingsCanvas.transform.Find("Flip Controls Button").Find("Text").gameObject.GetComponent<Text>().text = "FLIP CONTROLS\nJOYSTICK ON " + s;
    }
    
}
