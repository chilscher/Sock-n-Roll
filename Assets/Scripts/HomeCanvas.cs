using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeCanvas : MonoBehaviour {

    [Header("Canvases")]
    public GameObject creditsCanvas;
    public GameObject playCanvas;

    [Header("Reviving Buttons")]
    public GameObject revivePlayerButton;
    public GameObject reviveEnemyButton;

    private Player player;
    private Enemy enemy;

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        enemy = GameObject.Find("Enemy").GetComponent<Enemy>();
    }

    private void Update() {
        revivePlayerButton.SetActive(player.getIsDead());
        reviveEnemyButton.SetActive(enemy.getIsDead());
    }

    public void quit() {
        Application.Quit();
    }

    public void revivePlayer() {
        player.startReviving();
    }

    public void reviveEnemy() {
        enemy.startReviving();
    }

    public void credits() {
        creditsCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    public void play() {
        playCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
