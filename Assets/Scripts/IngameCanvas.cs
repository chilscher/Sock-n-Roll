using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IngameCanvas : MonoBehaviour{

    [Header("Heart Icon")]
    public GameObject heartPrefab;
    public Vector2 heartPos;
    public float gapBetweenHearts = 60f;

    [Header("Canvases")]
    public GameObject pausedCanvasGO;
    public GameObject winCanvasGO;
    public GameObject loseCanvasGO;
    public UnityEngine.UI.Text levelNumberTextBox;

    [Header("Pause/Resume Key")]
    public KeyCode pauseKey = KeyCode.Escape;

    private Player player;
    private List<GameObject> hearts;
    private PausedCanvas pausedCanvasC;
    

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player.setIngameCanvas(this);

        setLevelNumberBox();

        pausedCanvasC = pausedCanvasGO.GetComponent<PausedCanvas>();
        pausedCanvasGO.SetActive(true);
        drawHearts();
        pausedCanvasC.setResumeKey(pauseKey);
        pausedCanvasC.setIngameCanvasGO(gameObject);
        pausedCanvasGO.SetActive(false);
        
        winCanvasGO.SetActive(false);

    }

    
    void Update(){
        if (Input.GetKeyDown(pauseKey)) {hitPauseButton();}

        win();
        lose();
    }

    private void drawHearts() {
        int heartAmount = player.getHP();
        hearts = new List<GameObject>();
        float firstX = heartPos.x;
        float firstY = heartPos.y;
        float firstZ = 0f;

        for (int i = 0; i<heartAmount; i++) {
            float x = firstX + (gapBetweenHearts * i);
            float y = firstY;
            float z = firstZ;

            Vector3 pos = new Vector3(x,y,z);
            GameObject h = Instantiate(heartPrefab);
            h.transform.SetParent(transform);
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            hearts.Add(h);
        }

        pausedCanvasC.drawHearts(heartAmount, new Vector3(firstX, firstY, firstZ), gapBetweenHearts, heartPrefab);
    }

    public void loseHeart() {
        GameObject h = hearts[hearts.Count - 1];
        hearts.RemoveAt(hearts.Count - 1);
        Destroy(h);
        pausedCanvasC.loseHeart();
    }

    public void hitPauseButton() {pause();}

    private void pause() {
        Time.timeScale = 0f;
        pausedCanvasGO.SetActive(true);
        gameObject.SetActive(false);
    }

    public void win() {
        if (player.hasWonYet()) {
            winCanvasGO.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void lose() {
        if (player.hasLostYet()) {
            loseCanvasGO.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void setLevelNumberBox() {
        string name = SceneManager.GetActiveScene().name;
        levelNumberTextBox.text = name.ToUpper();
    }


}
