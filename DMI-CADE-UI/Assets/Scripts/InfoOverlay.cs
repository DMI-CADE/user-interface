using System;
using System.Collections;
using System.Collections.Generic;
using DmicInputHandler;
using TMPro;
using UnityEngine;

namespace Dmicade
{
    public class InfoOverlay : MonoBehaviour
    {
        public GameObject backdrop;
        public BorderedContainer textContainer;
        public TextMeshProUGUI infoText;
        public BorderedContainer interactionIndicator;

        public bool IsOpen { get; private set; }

        private Canvas _canvas;
        
        private void Awake()
        {
            textContainer.OnOpened += OverlayOpened;
            textContainer.OnClosed += OverlayClosed;

            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            textContainer.SetClosed(false);
            interactionIndicator.SetClosed(false);
        }

        void Update()
        {
            if (InputHandler.GetButtonDown(DmicButton.P1F) && IsOpen)
            {
                Disable();
            }
        }

        public void Enable(string appId)
        {
            infoText.text = ContentDataManager.Instance.GetApp(appId).moreInfoText;
            
            _canvas.enabled = true;
            textContainer.Open();
            FindObjectOfType<AudioManager>().Play("ButtonPress");
            FindObjectOfType<AudioManager>().Play("MoreInfoOpen");
        }

        public void Disable()
        {
            IsOpen = false;
            textContainer.Close();
            interactionIndicator.Close();
            FindObjectOfType<AudioManager>().Play("ButtonPress");
            FindObjectOfType<AudioManager>().Play("MoreInfoClose");
        }

        private void OverlayOpened()
        {
            interactionIndicator.Open();
            IsOpen = true;
        }
        
        private void OverlayClosed()
        {
            _canvas.enabled = false;
            DmicSceneManager.Instance.ChangeState(SceneState.InMenu);
        }
    }
}
