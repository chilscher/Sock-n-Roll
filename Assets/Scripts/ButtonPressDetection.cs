using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressDetection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    private bool isDown;
    public bool isPunchButton;
    public bool isRollButton;

    public void OnPointerDown(PointerEventData eventData) {
        this.isDown = true;
        StaticVariables.isPressingButton = true;
        if (isPunchButton) {
            //StaticVariables.pressingPunchButton = true;
            StaticVariables.justPressedPunchButton = true;
        }
        else if (isRollButton) {
            StaticVariables.pressingRollButton = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        this.isDown = false;
        StaticVariables.isPressingButton = false;
        if (isPunchButton) {
            //StaticVariables.pressingPunchButton = false;
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