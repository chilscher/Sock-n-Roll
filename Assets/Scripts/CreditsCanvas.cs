using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsCanvas : MonoBehaviour {

    [Header("Canvases")]
    public GameObject homeCanvas;

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

    public void back() {
        homeCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    public void revivePlayer() {
        player.startReviving();
    }

    public void reviveEnemy() {
        enemy.startReviving();
    }

}
