using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseCanvas : MonoBehaviour {

    public void retry() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void quit() { SceneManager.LoadScene("Main Menu"); }

}
