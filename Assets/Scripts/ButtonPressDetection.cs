using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressDetection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    private bool isDown;
    public bool isPunchButton;
    public bool isRollButton;

    public void OnPointerDown(PointerEventData eventData) {
        this.isDown = true;
        if (isPunchButton) {
            StaticVariables.justPressedPunchButton = true;
        }
        else if (isRollButton) {
            StaticVariables.pressingRollButton = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        this.isDown = false;
        if (isPunchButton) {
            StaticVariables.justPressedPunchButton = false;
        }
        else if (isRollButton) {
            StaticVariables.pressingRollButton = false;
        }
    }

    void Update() {
        if (!this.isDown) return;
    }

}