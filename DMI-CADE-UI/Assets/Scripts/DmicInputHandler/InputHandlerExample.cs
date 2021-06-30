using UnityEngine;
using DmicInputHandler;

public class InputHandlerExample : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (InputHandler.GetButtonDown(DmicButton.P1A))
        {
            Debug.Log("Pressed: P1A");
        }
        
        if (InputHandler.GetButtonUp(DmicButton.P1A))
        {
            Debug.Log("Released: P1A");
        }
    }
}
