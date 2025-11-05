using UnityEngine;

public static class InputHelper
{
    public static bool GetAnyButtonDown()
    {
        return Input.GetMouseButtonDown(0) || Input.touchCount > 0;
    }
}
