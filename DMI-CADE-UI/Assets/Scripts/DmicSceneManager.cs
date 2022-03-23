using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UdsClient;

namespace Dmicade
{
    public enum SceneState { None,
        EnableMenu, // In the process of transitioning to 'InMenu'. Transitions to 'InMenu' in the next update call.
        InMenu, // In menu, scrolling.
        InfoOverlay, // Looking at the info overlay.
        StartingApp, // Waiting for the app to start. In the process of transitioning to 'InApp'.
        InApp,
        Idle
    }

    public class DmicSceneManager : MonoBehaviour
    {
        public static DmicSceneManager Instance;

        public ContentDisplayManager displayManager;
        public InfoOverlay infoOverlay;
        public LoadingIndicatorOverlay loadingOverlay;

        public string LastRunningApp { get; private set; } = null;

        /// Invoked when sending the msg to start app.
        public event Action<string> OnAppStarting;

        /// Invoked when app could not start.
        public event Action OnAppStartFailed;

        private SceneState _sceneState = SceneState.None;

        private const string DmicUdsPath = @"/tmp/dmicade_socket.s";
        private PmClient _pmClient;
        private Queue<string> _receivedMessages = new Queue<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            // Set referenced objects.
            Instance.displayManager = GameObject.FindWithTag("ContentDisplayManager")?
                .GetComponent<ContentDisplayManager>();
            Instance.infoOverlay = GameObject.FindWithTag("MoreInfoOverlay")?
                .GetComponent<InfoOverlay>();
            Instance.loadingOverlay = GameObject.FindWithTag("LoadingOverlay")?
                .GetComponent<LoadingIndicatorOverlay>();

            infoOverlay.gameObject.SetActive(true);

            // Only leave Instance alive.
            if (this != Instance) 
            {
                Destroy(gameObject);
                return;
            }

            _pmClient = new PmClient(DmicUdsPath);

            _pmClient.OnMessageReceived += MessageReceived;

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

                case SceneState.InMenu:
                    if (Input.GetKeyDown(KeyCode.V)) MessageReceived("idle_enter");
                    break;

                case SceneState.StartingApp:
                    // Simulate receiving messages
                    if (Input.GetKeyDown(KeyCode.B)) MessageReceived("app_started:true");
                    else if (Input.GetKeyDown(KeyCode.N)) MessageReceived("app_started:false");
                    break;

                case SceneState.InApp:
                    // Simulate received message: 'activate'
                    if (Input.GetKeyDown(KeyCode.N)) MessageReceived("activate");
                    break;

                case SceneState.Idle:
                    if (Input.GetKeyDown(KeyCode.V)) MessageReceived("activate");
                    break;
            }

            ProcessMessage();
        }

        /// Always call this method when the state should change. It invokes necessary functions for the state change.
        /// TODO doc
        public void ChangeState(SceneState state, string data=null)
        {
            // Debug.Log("ChangeState: " + state + " : " + data);
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

                case SceneState.InApp:
                    AppStarted();
                    break;

                case SceneState.InfoOverlay:
                    infoOverlay.Enable(data);
                    break;

                case SceneState.Idle:
                    EnterIdle();
                    break;
            }

            _sceneState = state;
        }

        private void StartApp(string appId)
        {
            Debug.Log("Start app: " + appId);
            LastRunningApp = appId;

            loadingOverlay.Enable();

            OnAppStarting?.Invoke(appId);
            
            FindObjectOfType<AudioManager>()?.Play("AppSelected"); // TODO fix scene loading issue

            // Send app selection to process manager.
            #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                _pmClient.Send($"start_app:{appId}");
            #endif
        }

        private void EnableMenu()
        {
            SceneManager.LoadScene("MainScene");
            // State changes in next update call for scroll activation.
        }

        private void DeactivateMenu()
        {
            
        }

        private void AppStarted()
        {
            loadingOverlay.Disable();
            SceneManager.LoadScene("InApp");
        }

        private void EnterIdle()
        {
            SceneManager.LoadScene("InIdle");
        }
        
        private void AppFailedToStart()
        {
            // TODO DisplayAppStartFail();
            loadingOverlay.Disable();

            OnAppStartFailed?.Invoke();

            ChangeState(SceneState.InMenu);
        }

        private void SetCrashMsg()
        {
            // TODO set crash msg 
        }

        private void MessageReceived(object source, MessageReceivedEventArgs args)
        {
            // Debug.Log($"received: {args.Msg}");
            _receivedMessages.Enqueue(args.Msg);
        }

        private void MessageReceived(string msg) => MessageReceived(null, new MessageReceivedEventArgs() {Msg = msg});
        
        private void ProcessMessage()
        {
            if (_receivedMessages.Count == 0) return;
            string msg = _receivedMessages.Dequeue();
            // Debug.Log($"Process msg: {msg}");
            
            switch (msg)
            {
                case "boot": // Initial startup.
                    break;

                case "app_started:true": // App start success.
                    // HideLoadingOverlay();
                    ChangeState(SceneState.InApp);
                    break;

                case "app_started:false": // App start failed.
                    AppFailedToStart();
                    break;

                case "app_not_found": // App not configured correctly.
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
                    // DeactivateMenu();
                    break;

                case "idle_enter": // Enter idle.
                    ChangeState(SceneState.Idle);
                    break;

                case "idle_exit": // Exit idle.
                    break;

                default:
                    Debug.LogWarning("Can not interpret message: " + msg);
                    break;
            }
        }

        private IEnumerator RunUdsClient()
        {
            #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                yield return _pmClient.Run();
            #endif
            yield return null;
        }

        private void OnDestroy()
        {
            _pmClient?.Disconnect();
        }
    }
}
