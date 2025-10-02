using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    public UnityEvent zPressDown;
    public UnityEvent xPressDown;
    public UnityEvent cPressDown;
    public UnityEvent leftClickPressDown;

    public void OnZ(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Debug.Log("Z key pressed");
            zPressDown.Invoke();
        }
    }
    public void OnX(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Debug.Log("X key pressed");
            xPressDown.Invoke();
        }
    }

    public void OnC(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Debug.Log("C key pressed");
            cPressDown.Invoke();
        }
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("Left Click key pressed");
            leftClickPressDown.Invoke();
        }
    }

}
