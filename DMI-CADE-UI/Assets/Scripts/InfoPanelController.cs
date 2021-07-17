using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{

    [RequireComponent(typeof(ContentDisplayManager))]
    public class InfoPanelController : MonoBehaviour
    {
        public InfoPanel infoPanel;

        [Header("Drag Options")] 
        public float dragBuildupTime = 0.1f;
        public float dragReliefTime = 0.1f;
        public LeanTweenType dragBuildupEaseType;
        public LeanTweenType dragReliefEaseType;
        public float dragStrength = 0.1f;
        public float rotateAmount;

        private ContentDataManager _contentDataManager;
        private ContentDisplayManager _cdm;
        private CdmSyncTimer _syncTimer;
        private Vector3 _infoPanelBasePos;
        private float _infoPanelBaseYRot;

        private void Awake()
        {
            _contentDataManager = ContentDataManager.Instance;

            _cdm = gameObject.GetComponent<ContentDisplayManager>();
            _syncTimer = gameObject.GetComponent<CdmSyncTimer>();

            _infoPanelBasePos = infoPanel.transform.position;
            _infoPanelBaseYRot = infoPanel.transform.rotation.eulerAngles.y;

            _cdm.OnSelectionChange += SelectionChange;
            _cdm.OnScrollStart += EngageDrag;
            _cdm.OnScrollEnd += DisengageDrag;
            _cdm.OnScrollContinueStop += DisengageDrag;

            _syncTimer.OnTick += infoPanel.ShowNextGameMode;
        }

        private void SelectionChange(string selection)
        {
            infoPanel.StopVideo();

            var app = _contentDataManager.GetApp(selection);
            infoPanel.SetTitle(app.displayName);

            if (app.videos.Length > 0)
            {
                infoPanel.SetVideoUri(app.GetRandomVideo());
                infoPanel.PlayVideo();
            }

            infoPanel.SetBaseInfo(app);
            
            infoPanel.SetGameModes(app.parsedGameModes);
        }

        private void EngageDrag(Vector3 distance)
        {
            LeanTween.cancel(infoPanel.gameObject);
            LeanTween.move(infoPanel.gameObject, dragStrength * distance.normalized + _infoPanelBasePos, dragBuildupTime)
                .setEase(dragBuildupEaseType);
            float rotateDirFactor = Math.Sign(Vector3.Dot(distance, _cdm.scrollDirection));
            LeanTween.rotateY(infoPanel.gameObject, _infoPanelBaseYRot + rotateAmount * rotateDirFactor, dragBuildupTime)
                .setEase(dragBuildupEaseType);
        }

        private void DisengageDrag(Vector3 distance)
        {
            LeanTween.cancel(infoPanel.gameObject);
            LeanTween.move(infoPanel.gameObject, _infoPanelBasePos, dragReliefTime).setEase(dragReliefEaseType);
            LeanTween.rotateY(infoPanel.gameObject, _infoPanelBaseYRot, dragReliefTime).setEase(dragReliefEaseType);
        }
        
        private void DisengageDrag() => DisengageDrag(Vector3.zero);
        
    }
}
