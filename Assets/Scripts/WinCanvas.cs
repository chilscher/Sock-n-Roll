using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCanvas : MonoBehaviour {

    private string nextLevelName;
    private bool doesNextLevelExist;
    public GameObject nextLevelButton;

    private void Start() {
        getNextLevelName();
        doesNextLevelExist = Application.CanStreamedLevelBeLoaded(nextLevelName);
        hideNextLevelButton();
    }
    public void quit() { SceneManager.LoadScene("Main Menu"); }

    public void nextLevel() {
        if (doesNextLevelExist) { SceneManager.LoadScene(nextLevelName); }
    }

    private void getNextLevelName() {
        string name = SceneManager.GetActiveScene().name;

        string num = name.Substring(name.Length - 1);
        int n = int.Parse(num);
        n++;
        nextLevelName = name.Substring(0, name.Length - 1) + n.ToString();
    }

    private void hideNextLevelButton() {
        if (!doesNextLevelExist) { nextLevelButton.SetActive(false); }
    }

}
