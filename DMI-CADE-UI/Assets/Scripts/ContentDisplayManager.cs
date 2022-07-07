using System;
using System.Linq;
using DmicInputHandler;
using Pooling;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dmicade
{
    enum ScrollState { Stop, Accel, Decel, Continue }
    public enum ScrollDir { None = 0, Forward = -1, Backwards = 1 }
    
    public class ContentDisplayManager : MonoBehaviour
    {
        [Header("Setup")]
        public GameObject selectedElementAnchor;
        [Min(0f)] public float elementSpacing = 7f;
        [Tooltip("Amount of generated display elements (excluding overlap). When 0 uses the amount of available apps."), 
         Min(0)] public int initialElementsAmount = 0;
        [Tooltip("Overlapping elements per side."),
         Range(1,5)] public int overlappingElements = 2;

        [Header("Scroll Settings")]
        public Vector3 scrollDirection = Vector3.forward;
        public float accelerationTime = 1f;
        public LeanTweenType accelerationType;
        public float decelerationTime = 1f;
        public LeanTweenType decelerationType;
        public float inputBuffer;
        public LeanTweenType alphaBlendType;
        
        [Space(20)]
        private ContentDataManager _contentDataManager; // Reference to singleton.

        public event Action OnScrollEnable;
        public event Action OnScrollDisable;
        
        /// Invoked when scroll starts after stopping.
        public event Action<Vector3> OnScrollStart;
        
        /// Invoked when scrolling starts decelerating.
        public event Action<Vector3> OnScrollEnd;
        
        /// Invoked when instead of decelerating, the scrolling continues with linear speed.
        public event Action<Vector3> OnScrollContinueOnEnd;
        
        /// Invoked when continued scrolling but then stopping at a selection.
        public event Action OnScrollContinueStop;
        
        /// Invoked when still continue scrolling linearly after deceleration also continued linearly.
        public event Action<Vector3> OnScrollContinueOnStart;
        
        /// Invoked when the selected element changes. 
        public event Action<string> OnSelectionChange;
        
        private string[] _appOrder;
        private Vector3 _selectedElementPosition;
        private Vector3 _rearmostAnchor;
        private Vector3 _foremostAnchor;
        private int _selectedElement = 0;
        private int _selectedData = 0;
        private ContentDisplayElement[] _displayElements;
        private ScrollState _scrollState = ScrollState.Stop;
        private Vector3 _moveIncrementDistance = Vector3.zero;
        private ScrollDir _currentScrollDir = ScrollDir.None;
        private float _inputTimeStamp = 0f;
        private ScrollDir _queuedScrollDir = ScrollDir.None;
        private bool _scrollActive = false;

        private void Awake()
        {
            _contentDataManager = ContentDataManager.Instance;
            _selectedElementPosition = selectedElementAnchor.transform.position;
            selectedElementAnchor.SetActive(false);
            
            // Use alphabetical order as default.
            _appOrder = _contentDataManager.AppNames;
            Array.Sort(_appOrder);
            Debug.Log("App Order: " + String.Join(", ", _appOrder));

            _selectedData = Array.IndexOf(_appOrder, DmicSceneManager.Instance.LastRunningApp);
            _selectedData = _selectedData < 0 ? 0 : _selectedData; // When 'LastRunningApp' not found/is null start at 0.
        }

        // Start is called before the first frame update
        void Start()
        {
            FillDisplayElements();
            InitialDisplayElementsSetup();

            _rearmostAnchor = _selectedElementPosition - (elementSpacing * (overlappingElements + 1)) * scrollDirection;
            _foremostAnchor = _selectedElementPosition +
                           (elementSpacing * (_displayElements.Length - overlappingElements)) * scrollDirection;
            OnSelectionChange?.Invoke(_appOrder[_selectedData]);
        }
        
        // Update is called once per frame
        void Update()
        {
            if (!_scrollActive) return;
            
            // Scroll forward.
            if (InputHandler.GetButtonDown(DmicButton.P1Up))
            {
                _inputTimeStamp = Time.time;
                _queuedScrollDir = ScrollDir.Forward;
                
                if (_scrollState == ScrollState.Stop)
                {
                    StartMovement(ScrollDir.Forward);
                }
            }
            // Scroll backwards.
            else if (InputHandler.GetButtonDown(DmicButton.P1Down))
            {
                _inputTimeStamp = Time.time;
                _queuedScrollDir = ScrollDir.Backwards;
                
                if (_scrollState == ScrollState.Stop)
                {
                    StartMovement(ScrollDir.Backwards);
                }
            }
            // Show more info.
            else if (_scrollState == ScrollState.Stop && InputHandler.GetButtonDown(DmicButton.P1F))
            {
                if (SelectionHasAdditionalInfo())
                {
                    DisableScroll();
                    DmicSceneManager.Instance.ChangeState(SceneState.InfoOverlay, _appOrder[_selectedData]);
                }
            }
            // Start app.
            else if (_scrollState == ScrollState.Stop && 
                     (InputHandler.GetButtonDown(DmicButton.P1Start) || InputHandler.GetButtonDown(DmicButton.P2Start)))
            {
                DisableScroll();
                DmicSceneManager.Instance.ChangeState(SceneState.StartingApp, _appOrder[_selectedData]);
            }
        }

        public string GetSelectedApp() => _appOrder[_selectedData];

        public void EnableScroll()
        {
            OnScrollEnable?.Invoke();
            _scrollActive = true;
        }

        public void DisableScroll()
        {
            OnScrollDisable?.Invoke();
            _scrollActive = false;
        }

        public bool SelectionHasAdditionalInfo()
        {
            return _contentDataManager.GetApp(GetSelectedApp()).moreInfoText.Length > 0;
        }
        
        /// TODO doc
        private void MoveAllDisplayElements(Vector3 moveIncrementDistance, LeanTweenType easeType, float time,
            Action callback=null)
        {
            for (int i = 0; i < _displayElements.Length; i++)
            {
                if (i == _selectedElement)
                    _displayElements[i].Move(moveIncrementDistance, easeType, time, callback);
                else
                    _displayElements[i].Move(moveIncrementDistance, easeType, time);
            }
        }

        private void AdvanceSelection(ScrollDir scrollDir)
        {
            _selectedElement = Mod(_selectedElement + -1 * (int) scrollDir, _displayElements.Length);
            //Debug.Log("_selectedElement: " + _selectedElement);
            _selectedData = Mod(_selectedData + -1 * (int) scrollDir, _contentDataManager.AppAmount);
            //Debug.Log("_selectedData: " + _selectedData);
            
            OnSelectionChange?.Invoke(_appOrder[_selectedData]);
        }

        /// TODO doc
        private void StartMovement(ScrollDir scrollDir)
        {
            _currentScrollDir = scrollDir;
            
            AdvanceSelection(scrollDir);
            UpdateHighlightingContrast(_currentScrollDir);
            UpdateEdgeElement(scrollDir);

            _scrollState = ScrollState.Accel;
            _moveIncrementDistance = (int) scrollDir * elementSpacing / 2 * scrollDirection;

            MoveAllDisplayElements(_moveIncrementDistance, accelerationType, accelerationTime, UpdateMovement);
            
            OnScrollStart?.Invoke(_moveIncrementDistance);
        }

        /// Meant to be invoked once after the action of current _moveState is done. TODO doc
        private void UpdateMovement()
        {
            // Acceleration done.
            if (_scrollState == ScrollState.Accel)
            {
                // Continue
                if (CheckKeepScrolling())
                {
                    _scrollState = ScrollState.Continue;
                    MoveAllDisplayElements(_moveIncrementDistance, LeanTweenType.linear, decelerationTime,
                        UpdateMovement);
                    
                    OnScrollContinueOnEnd?.Invoke(_moveIncrementDistance);
                }
                // Default: decelerate
                else
                {
                    _scrollState = ScrollState.Decel;
                    MoveAllDisplayElements(_moveIncrementDistance, decelerationType, decelerationTime, UpdateMovement);
                    
                    OnScrollEnd?.Invoke(_moveIncrementDistance);
                }
            }
            
            // Continue scrolling done.
            else if (_scrollState == ScrollState.Continue)
            {
                if (CheckKeepScrolling())
                {
                    _scrollState = ScrollState.Accel;
                    AdvanceSelection(_currentScrollDir);
                    UpdateHighlightingContrast(_currentScrollDir);
                    UpdateEdgeElement(_currentScrollDir);
                    MoveAllDisplayElements(_moveIncrementDistance, LeanTweenType.linear, decelerationTime,
                        UpdateMovement);
                        
                    OnScrollContinueOnStart?.Invoke(_moveIncrementDistance);
                }
                else
                {
                    // Input buffered
                    if (_inputTimeStamp + inputBuffer >= Time.time)
                    {
                        StartMovement(_queuedScrollDir);
                    }
                    else
                    {
                        OnScrollContinueStop?.Invoke();
                        _scrollState = ScrollState.Stop;
                    }
                }
            }

            // Deceleration done.
            else if (_scrollState == ScrollState.Decel)
            {
                // Input buffered
                if (_inputTimeStamp + inputBuffer >= Time.time)
                {
                    StartMovement(_queuedScrollDir);
                } 
                
                // Default: stop at selection
                else
                    _scrollState = ScrollState.Stop;
            }
        }

        private bool CheckKeepScrolling()
        {
            return InputHandler.GetButton(DmicButton.P1Up) && _currentScrollDir == ScrollDir.Forward ||
                   InputHandler.GetButton(DmicButton.P1Down) && _currentScrollDir == ScrollDir.Backwards;
        }

        /// <summary>
        ///  Updates display element hue highlighting. Call after selection advanced.
        /// </summary>
        /// <param name="scrollDir"></param>
        private void UpdateHighlightingContrast(ScrollDir scrollDir)
        {
            if (scrollDir == ScrollDir.Forward)
            {
                _displayElements[Mod(_selectedElement - 1, _displayElements.Length)].Darken(accelerationTime);
                _displayElements[_selectedElement].Brighten(accelerationTime);
            }
            else if (scrollDir == ScrollDir.Backwards)
            {
                _displayElements[Mod(_selectedElement + 1, _displayElements.Length)].Darken(accelerationTime);
                _displayElements[_selectedElement].Brighten(accelerationTime);
            }
        }

        /// Updates after advancing selected-values. TODO doc
        private void UpdateEdgeElement(ScrollDir scrollDir)
        {
            int movedElement = _selectedElement - overlappingElements;
            if (scrollDir == ScrollDir.Forward)
            {
                // Enable and disable elements.
                //Debug.Log("--Disappear: " + Mod(_selectedElement - 1, _displayElements.Length));
                DisappearElement(Mod(_selectedElement - 1, _displayElements.Length));
                //_displayElements[Mod(_selectedElement - 2, _displayElements.Length)].gameObject.SetActive(false);
                
                int appearingElement = Mod(_selectedElement - (2 * overlappingElements) - 1, _displayElements.Length);
                //_displayElements[Mod(appearingElement + 1, _displayElements.Length)].gameObject.SetActive(true);
                //Debug.Log("--Appear: " + appearingElement);
                AppearElement(appearingElement);

                // Move element.
                movedElement = Mod(movedElement - 1, _displayElements.Length);
                _displayElements[movedElement].transform.position = _foremostAnchor;
            } 
            else if (scrollDir == ScrollDir.Backwards)
            {
                // Enable and disable elements.
                //Debug.Log("--Disappear: " + Mod(_selectedElement - (2 * overlappingElements), _displayElements.Length));
                DisappearElement(Mod(_selectedElement - (2 * overlappingElements), _displayElements.Length));
                //_displayElements[Mod(_selectedElement - (2 * overlappingElements) + 1, _displayElements.Length)].gameObject.SetActive(false);
                
                
                //_displayElements[Mod(_selectedElement - overlappingElements + 1, _displayElements.Length)].gameObject.SetActive(true);
                //Debug.Log("--Appear: " + _selectedElement);
                AppearElement(_selectedElement);
                
                        
                // Move element.
                movedElement = Mod(movedElement, _displayElements.Length);
                _displayElements[movedElement].transform.position = _rearmostAnchor;
            }
            
            ReadyDisplayElement(movedElement);
        }

        private void AppearElement(int index)
        {
            LeanTween.alpha(_displayElements[index].rectTransform, 1f, accelerationTime + decelerationTime)
                .setEase(alphaBlendType);
        }
        
        private void DisappearElement(int index)
        {
            LeanTween.alpha(_displayElements[index].rectTransform, 0f, accelerationTime + decelerationTime);
        }
        
        /// <summary>
        /// Fills the <see cref="_displayElements"/> array with display elements.
        /// </summary>
        private void FillDisplayElements()
        {
            // On <2, default to amount of loaded apps (min. 2). 
            int amount = initialElementsAmount > 1 ? initialElementsAmount : Math.Max(_contentDataManager.AppAmount, 2);
            
            // Add additional overlapping elements elements.
            _displayElements = new ContentDisplayElement[amount + 2 * overlappingElements];
            
            for (int i = 0; i < _displayElements.Length; i++)
            {
                _displayElements[i] = ContentDisplayElementPool.Instance.Get();
            }
        }

        /// <summary>
        /// The initial setup for the display elements. Sets positions and equips them with data references. 
        /// </summary>
        /// <para>Only works if <see cref="_selectedData"/> and <see cref="_selectedElement"/> are 0.</para>
        private void InitialDisplayElementsSetup()
        {
            int visibleElements = _displayElements.Length - 2 * overlappingElements;

            /*Debug.Log("contentDataManager.AppAmount: " + contentDataManager.AppAmount +
                      "\n_selectedData: " + _selectedData +
                      "\n_displayElements.Length: " + _displayElements.Length +
                      "\n_selectedElement: " + _selectedElement +
                      "\noverlappingElements: " + overlappingElements +
                      "\nvisibleElements: " + visibleElements);*/
            
            for (int i = 0; i < _displayElements.Length; i++)
            {
                // Position
                float distance = elementSpacing * i;

                // Set second half of the overlapping elements behind the camera.
                if (i >= (visibleElements + overlappingElements))
                    distance = elementSpacing * (i - _displayElements.Length);
                
                _displayElements[i].transform.position = _selectedElementPosition + (scrollDirection * distance);
                // TODO rotation

                // Data
                // Skip data setup when no data is present.
                if (_contentDataManager.AppAmount == 0) break;
                
                ReadyDisplayElement(i);
                
                bool isOverlap = i >= visibleElements;
                if (isOverlap)
                {
                    InitOverlappingElement(i);
                }
                else
                {
                    // InitVisibleElement
                    // _displayElements[i].gameObject.SetActive(true);
                }
                _displayElements[i].gameObject.SetActive(true);
                _displayElements[i].name = _displayElements[i].name + " " + i;

                // Set darkened hue
                _displayElements[i].Darken(0f);
            }

            _displayElements[_selectedElement].Brighten(0f);

            //_displayElements[_displayElements.Length - 1].gameObject.SetActive(true); // Activate elem directly behind cam.
        }

        /// TODO doc
        private void InitOverlappingElement(int index)
        {
            Color c = _displayElements[index].logoImage.color;
            c.a = .0f;
            _displayElements[index].logoImage.color = c;
        }

        /// <summary>
        /// Prepares a display element to get displayed.
        /// </summary>
        /// <param name="elemIndex">The index of the element to ready.</param>
        private void ReadyDisplayElement(int elemIndex)
        {
            int dataIndex = GetDataIndex(elemIndex);
            string appId = _appOrder[dataIndex];
            DmicAppData data = _contentDataManager.GetApp(appId);
            
            Image img = _displayElements[elemIndex].logoImage;
            img.sprite = data.logoSprite;
            img.preserveAspect = true;
        }

        /// <summary>Traditional modulo operation.</summary>
        // Traditional method: <c>n - mod * (int) Math.Floor((double) n / mod)</c> requires too much casting for my taste. 
        private static int Mod(int n, int mod) => ((n %= mod) < 0) ? n+mod : n;

        /// <summary>Returns the relative data index for a given display element index.</summary>
        /// <para>
        /// Relies on <see cref="_selectedElement"/>, <see cref="_selectedData"/>, <see cref="overlappingElements"/> to
        /// be set correctly i.e. realistic in relation to the array lengths of <see cref="_displayElements"/> and
        /// loaded app data in <see cref="_contentDataManager"/>.
        /// </para>
        /// <param name="elementIndex">the element index.</param>
        /// <returns>The according data index.</returns>
        private int GetDataIndex(int elementIndex)
        {
            int distFromSelectedElem = Mod((elementIndex - _selectedElement), _displayElements.Length);
            
            // 1 if elements are behind camera, 0 if not.
            int inBack =  distFromSelectedElem / (_displayElements.Length - overlappingElements);
            
            // Go backwards for elements behind the camera. 
            int result = distFromSelectedElem - _displayElements.Length * inBack;

            // Move along data array.
            result = Mod(_selectedData + result, _contentDataManager.AppAmount);

            return result;
        }
    }
}
