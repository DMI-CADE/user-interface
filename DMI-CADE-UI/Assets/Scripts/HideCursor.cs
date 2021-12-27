using UnityEngine;

public class HideCursor : MonoBehaviour
{
    private void Awake()
    {
        #if UNITY_STANDALONE
            Cursor.visible = false;
        #endif
    }
}
