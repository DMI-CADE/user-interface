using System;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using UnityEngine;

[RequireComponent(typeof(ContentDisplayManager))]
public class InfoPanelController : MonoBehaviour
{
    public InfoPanel infoPanel;

    private ContentDataManager _contentDataManager;
    
    private void Awake()
    {
        _contentDataManager = ContentDataManager.Instance;

        ContentDisplayManager cdm = gameObject.GetComponent<ContentDisplayManager>();
        
        cdm.OnSelectionChange += SelectionChange;
    }

    private void SelectionChange(string selection)
    {
        infoPanel.StopVideo();
        
        var app = ContentDataManager.Instance.GetApp(selection);
        infoPanel.SetTitle(app.displayName);

        if (app.videos.Length > 0)
        {
            infoPanel.SetVideoUri(app.GetRandomVideo());
            infoPanel.PlayVideo();
        }

        infoPanel.SetBaseInfo(app);
    }
}
