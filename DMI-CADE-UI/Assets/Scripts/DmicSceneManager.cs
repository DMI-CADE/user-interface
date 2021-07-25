using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using DmicInputHandler;
using UnityEngine;
using UnityEngine.SceneManagement;
using UdsClient;

namespace Dmicade
{
    public enum SceneState { None,
        EnableMenu, // In the process of transitioning to 'InMenu'. Transitions to 'InMenu' in the next update call.
        InMenu, // In menu, scrolling.
        InfoOverlay, // Looking at the info overlay.
        StartingApp, // Waiting for the app to start. In the process of transitioning to 'InGame'.
        InGame
    }
    
    public class DmicSceneManager : MonoBehaviour
    {
        public static DmicSceneManager Instance;

        public ContentDisplayManager displayManager;
        public InfoOverlay infoOverlay;

        public string LastRunningApp { get; private set; } = null;
        
        private SceneState _sceneState = SceneState.None;

        private const string DmicUdsPath = @"/tmp/dmicade_socket.s";
        private PmClient _pmClient;
        private Task _pmClientTask;
        
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
            if (this != Instance) 
            {
                Destroy(gameObject);
                return;
            }

            _pmClient = new PmClient(DmicUdsPath);

            _pmClient.MessageReceived += MessageReceived;

            StartCoroutine(nameof(RunUdsClient));
        }

        // Start is called before the first frame update
        void Start()
        {
            ChangeState(SceneState.InMenu);
        }
        
        // Update is called once per frame
        void Update()
        {
            switch (_sceneState)
            {
                case SceneState.EnableMenu:
                    // Activate menu in next update call to ensure scene is loaded.
                    ChangeState(SceneState.InMenu);
                    break;
                
                case SceneState.StartingApp:
                    // Simulate receiving message: 'app_started:true'
                    if (Input.GetKeyDown(KeyCode.B)) ChangeState(SceneState.InGame);
                    break;
                
                case SceneState.InGame:
                    // Simulate received message: 'activate'
                    if (Input.GetKeyDown(KeyCode.N)) ChangeState(SceneState.EnableMenu);
                    break;
            }
        }

        /// Always call this method when the state should change. It invokes necessary functions for the state change.
        /// TODO doc
        public void ChangeState(SceneState state, string data=null)
        {
            switch (state)
            {
                case SceneState.EnableMenu:
                    EnableMenu();
                    break;
                
                case SceneState.InMenu:
                    displayManager.EnableScroll();
                    break;
                
                case SceneState.StartingApp:
                    StartApp(data);
                    break;
                
                case SceneState.InGame:
                    AppStarted();
                    break;
                
                case SceneState.InfoOverlay:
                    infoOverlay.Enable(data);
                    break;
            }

            _sceneState = state;
        }

        private void StartApp(string appId)
        {
            Debug.Log("Start app: " + appId);
            LastRunningApp = appId;

            // TODO ShowLoadingOverlay();

            // Send app selection to process manager.
            #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                _pmClient.Send($"start_app:{appId}");
            #endif
        }

        private void EnableMenu()
        {
            SceneManager.LoadScene("MainScene");
        }

        private void DeactivateMenu()
        {
            
        }

        private void AppStarted()
        {
            SceneManager.LoadScene("InGame");
        }
        
        private void AppFailedToStart()
        {
            // TODO DisplayAppStartFail();
            // TODO HideLoadingOverlay();
            ChangeState(SceneState.InMenu);
        }

        private void SetCrashMsg()
        {
            // TODO set crash msg 
        }

        private void MessageReceived(object source, MessageReceivedEventArgs args) 
        {
            switch (args.Msg)
            {
                case "boot": // Initial startup.
                    break;

                case "app_started:true": // App start success.
                    // HideLoadingOverlay();
                    ChangeState(SceneState.InGame);
                    break;

                case "app_started:false": // App start failed.
                    AppFailedToStart();
                    break;

                case "game_not_found": // App not configured correctly.
                    AppFailedToStart();
                    break;

                case "app_closed": // App closed by menu button or on its own.
                    break;

                case "app_crashed": // App closed by menu button or on its own.
                    SetCrashMsg();
                    break;

                case "activate": // Activate app selection menu.
                    ChangeState(SceneState.EnableMenu);
                    break;

                case "deactivate": // Deactivate app selection.
                    DeactivateMenu();
                    break;

                case "idle_enter": // Enter idle.
                    break;

                case "idle_exit": // Exit idle.
                    break;

                default:
                    Debug.LogWarning("Can not interpret message: " + args.Msg);
                    break;
            }
        }

        private IEnumerator RunUdsClient()
        {
            #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                _pmClient.Run(120);
            #endif
            yield return null;
        }
    }
}
