using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private LoadingIndicatorOverlay loadingIndicatorOverlay;

    // Start is called before the first frame update
    void Start()
    {
        loadingIndicatorOverlay.Enable();
        overlayCanvas.enabled = true;
        StartCoroutine(LoadMainSceneAsync());
    }

    IEnumerator LoadMainSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");

        while(!asyncLoad.isDone) {
            yield return null;
        }

        overlayCanvas.enabled = false;

        SceneManager.UnloadSceneAsync("StartScene");
    }
}
