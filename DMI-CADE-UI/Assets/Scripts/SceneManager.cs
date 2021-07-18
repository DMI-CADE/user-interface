using System;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using DmicInputHandler;
using UnityEngine;

namespace Dmicade
{
    public enum SceneState { None, Scroll, InfoOverlay, InGame }
    
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance;

        public ContentDisplayManager displayManager;
        public InfoOverlay infoOverlay;

        private SceneState _sceneState = SceneState.None;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            infoOverlay.gameObject.SetActive(true);
        }

        // Start is called before the first frame update
        void Start()
        {
            ChangeState(SceneState.Scroll);
        }

        // Update is called once per frame
        void Update()
        {
            switch (_sceneState)
            {
                case SceneState.InfoOverlay:
                    
                    break;
            }
        }

        public void ChangeState(SceneState state, string data=null)
        {
            switch (state)
            {
                case SceneState.Scroll: 
                    displayManager.EnableScroll();
                    break;
                
                case SceneState.InfoOverlay:
                    infoOverlay.Enable(data);
                    break;
            }

            _sceneState = state;
        }
    }
}
