using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausedCanvas : MonoBehaviour{
    
    [Header("Canvases")]
    public UnityEngine.UI.Text levelNumberTextBox;

    private Player player;
    private List<GameObject> hearts;
    private KeyCode resumeKey;
    private GameObject ingameCanvasGO;

    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        setLevelNumberBox();
    }

    
    void Update(){
        if (Input.GetKeyDown(resumeKey)) { resume(); }
    }
    
    public void resume() {
        Time.timeScale = 1f;
        ingameCanvasGO.SetActive(true);
        gameObject.SetActive(false);
    }

    public void drawHearts(int amount, Vector3 firstHeartPos, float gap, GameObject heartPrefab) {
        hearts = new List<GameObject>();

        for (int i = 0; i < amount; i++) {
            float x = firstHeartPos.x + (gap * i);
            float y = firstHeartPos.y;
            float z = firstHeartPos.z;

            Vector3 pos = new Vector3(x, y, z);
            GameObject h = Instantiate(heartPrefab);
            h.transform.SetParent(transform);
            RectTransform rt = h.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            hearts.Add(h);
        }

    }

    public void loseHeart() {
        GameObject h = hearts[hearts.Count - 1];
        hearts.RemoveAt(hearts.Count - 1);
        Destroy(h);
    }

    public void quit() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void setResumeKey(KeyCode k) { resumeKey = k; }

    private void setLevelNumberBox() {
        string name = SceneManager.GetActiveScene().name;
        levelNumberTextBox.text = name.ToUpper();
    }

    public void setIngameCanvasGO(GameObject g) {ingameCanvasGO = g;}

    public void restart() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
