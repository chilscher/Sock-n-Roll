//for Sock 'n Roll, copyright Cole Hilscher 2020

using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressDetection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    //handles the processing of when the player pushes or holds a button on the touch screen
    //one of these scripts gets attached to the punch and roll buttons
    //pressing these buttons sets some variables in StaticVariables, which the Player accesses to determine their next action
    public bool isPunchButton;
    public bool isRollButton;

    public void OnPointerDown(PointerEventData eventData) {
        if (isPunchButton) {
            StaticVariables.justPressedPunchButton = true;
        }
        else if (isRollButton) {
            StaticVariables.pressingRollButton = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (isPunchButton) {
            StaticVariables.justPressedPunchButton = false;
        }
        else if (isRollButton) {
            StaticVariables.pressingRollButton = false;
        }
    }

}