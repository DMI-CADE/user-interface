using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingIndicatorOverlay : MonoBehaviour
{
    public TextMeshProUGUI loadingChar;

    public string[] loadingChars = {"|", "/", "-", "\\"};
    public float loadingAnimFrameDuration = 0.1f;

    private Canvas _canvas;
    private int _indicatorState;
    private bool _indicatorActive;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
    }

    public void Enable()
    {
        _indicatorActive = true;
        _canvas.enabled = true; 
        StartCoroutine(nameof(AnimateLoadingChar));
    }

    public void Disable()
    {
        _canvas.enabled = false;
        _indicatorActive = false;
    }

    public IEnumerator AnimateLoadingChar()
    {
        do
        {
            _indicatorState = (_indicatorState + 1) % loadingChars.Length;

            loadingChar.text = loadingChars[_indicatorState];

            yield return new WaitForSeconds(loadingAnimFrameDuration);
        } while (_indicatorActive);
    }
}
