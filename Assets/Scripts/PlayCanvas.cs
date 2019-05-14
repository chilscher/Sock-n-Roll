using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayCanvas : MonoBehaviour {

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

    public void revivePlayer() {
        player.startReviving();
    }

    public void reviveEnemy() {
        enemy.startReviving();
    }

    public void back() {
        homeCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    public void level1() { SceneManager.LoadScene("Level 1"); }
    public void level2() { SceneManager.LoadScene("Level 2"); }
    public void level3() { SceneManager.LoadScene("Level 3"); }
    public void level4() { SceneManager.LoadScene("Level 4"); }
    public void level5() { SceneManager.LoadScene("Level 5"); }
    public void level6() { SceneManager.LoadScene("Level 6"); }
}
