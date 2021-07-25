using System;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using TMPro;
using UnityEngine;

public class InGameOverlay : MonoBehaviour
{
    public TextMeshProUGUI runningAppText;

    private void Awake()
    {
        string appName = ContentDataManager.Instance.GetApp(DmicSceneManager.Instance.LastRunningApp).displayName;
        runningAppText.text = $"Running {appName}...";
    }
}
