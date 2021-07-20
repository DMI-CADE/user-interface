using System;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using DmicInputHandler;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dmicade
{
    public enum SceneState { None, EnableMenu, Scroll, InfoOverlay, InGame }
    
    public class DmicSceneManager : MonoBehaviour
    {
        public static DmicSceneManager Instance;

        public ContentDisplayManager displayManager;
        public InfoOverlay infoOverlay;

        public string LastRunningApp { get; private set; } = null;
        
        private SceneState _sceneState = SceneState.None;
        
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            // Set referenced objects.
            Instance.displayManager = GameObject.FindWithTag("ContentDisplayManager")
                .GetComponent<ContentDisplayManager>();
            Instance.infoOverlay = GameObject.FindWithTag("MoreInfoOverlay")
                .GetComponent<InfoOverlay>();

            infoOverlay.gameObject.SetActive(true);
            
            // Only leave Instance alive.
            if (this != Instance) Destroy(gameObject);
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
                case SceneState.InGame:
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        EnableMenu();
                    }
                    break;
                
                case SceneState.EnableMenu:
                    ChangeState(SceneState.Scroll);
                    break;
            }
        }

        public void ChangeState(SceneState state, string data=null)
        {
            switch (state)
            {
                case SceneState.EnableMenu:
                    SceneManager.LoadScene("MainScene");
                    break;
                
                case SceneState.Scroll:
                    displayManager.EnableScroll();
                    break;
                
                case SceneState.InfoOverlay:
                    infoOverlay.Enable(data);
                    break;
                
                case SceneState.InGame:
                    StartApp(data);
                    break;
            }

            _sceneState = state;
        }

        private void StartApp(string appId)
        {
            Debug.Log("Start app: " + appId);
            LastRunningApp = appId;
            SceneManager.LoadScene("InGame");
        }

        private void EnableMenu()
        {
            ChangeState(SceneState.EnableMenu);
        }
    }
}
