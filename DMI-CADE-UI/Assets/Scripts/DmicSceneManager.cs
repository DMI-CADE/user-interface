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
    public enum SceneState { None, EnableMenu, Scroll, InfoOverlay, StartApp, InGame }
    
    public class DmicSceneManager : MonoBehaviour
    {
        public static DmicSceneManager Instance;

        public ContentDisplayManager displayManager;
        public InfoOverlay infoOverlay;

        public string LastRunningApp { get; private set; } = null;
        
        private SceneState _sceneState = SceneState.None;
        
        private string _dmicUdsPath = @"/tmp/dmicade_socket.s";
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

            _pmClient = new PmClient(_dmicUdsPath);

            _pmClient.MessageReceived += MessageReceived;

            _pmClientTask = Task.Run(() => _pmClient.Run(120));
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
                
                case SceneState.StartApp: 
                    StartApp(data);
                    break;

                case SceneState.InGame:
                    
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
            _pmClient.Send($"start_app:{appId}");
        }

        private void EnableMenu()
        {
            ChangeState(SceneState.EnableMenu);
        }

        private void AppFailedToStart()
        {
            // TODO DisplayAppStartFail();
            // TODO HideLoadingOverlay();
            ChangeState(SceneState.Scroll);
        }

        private void AppCrashed()
        {

        }

        private void MessageReceived(object source, MessageReceivedEventArgs args) 
        {
            switch (args.Msg)
            {
                case "boot": // Initial startup.
                    break;

                case "app_started:true": // App start success.
                    // TODO HideLoadingOverlay();
                    SceneManager.LoadScene("InGame");
                    break;

                case "app_started:false": // App start failed.
                    AppFailedToStart();
                    break;

                case "game_not_found": // App not configured correctly.
                    AppFailedToStart();
                    break;

                case "app_closed": // App closed by menu button or on its own.
                    EnableMenu();
                    break;

                case "app_crashed": // App closed by menu button or on its own.
                    AppCrashed();
                    break;

                case "activate": // Activate app selection menu.
                    EnableMenu();
                    break;

                case "deactivate": // Deactivate app selection.
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
    }
}
