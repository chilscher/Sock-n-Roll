using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuCanvasController : MonoBehaviour {

    [Header("Canvases")]
    public GameObject homeCanvas;
    public GameObject creditsCanvas;
    public GameObject playCanvas;

    
    private GameObject homeRevPlayerBtn;
    private GameObject homeRevEnemyBtn;
    private GameObject playRevPlayerBtn;
    private GameObject playRevEnemyBtn;
    private GameObject creditsRevPlayerBtn;
    private GameObject creditsRevEnemyBtn;

    private Player player;
    private Enemy enemy;

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
        homeRevPlayerBtn = homeCanvas.transform.Find("Revive Player Button").gameObject;
        homeRevEnemyBtn = homeCanvas.transform.Find("Revive Enemy Button").gameObject;
        playRevPlayerBtn = playCanvas.transform.Find("Revive Player Button").gameObject;
        playRevEnemyBtn = playCanvas.transform.Find("Revive Enemy Button").gameObject;
        creditsRevPlayerBtn = creditsCanvas.transform.Find("Revive Player Button").gameObject;
        creditsRevEnemyBtn = creditsCanvas.transform.Find("Revive Enemy Button").gameObject;
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    private void Update() {
        homeRevPlayerBtn.SetActive(player.getIsDead());
        homeRevEnemyBtn.SetActive(enemy.getIsDead());
        playRevPlayerBtn.SetActive(player.getIsDead());
        playRevEnemyBtn.SetActive(enemy.getIsDead());
        creditsRevPlayerBtn.SetActive(player.getIsDead());
        creditsRevEnemyBtn.SetActive(enemy.getIsDead());
    }

    public void _btnMainMenu() {
        homeCanvas.SetActive(true);
        playCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void _btnLevelSelect() {
        playCanvas.SetActive(true);
        homeCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void _btnCredits() {
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

    public void _btnLoadLevel(int x) { SceneManager.LoadScene("Level " + x.ToString());}
}
