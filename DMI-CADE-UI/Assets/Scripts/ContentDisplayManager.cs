using System;
using DmicInputHandler;
using Pooling;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dmicade
{
    enum ScrollState { Stop, Accel, Decel, Continue }
    enum ScrollDir { None = 0, Forward = -1, Backwards = 1 }
    
    public class ContentDisplayManager : MonoBehaviour
    {
        [Header("Setup")]
        public Vector3 selectedElementPosition;
        [Min(0f)] public float elementSpacing = 7f;
        [Tooltip("Amount of generated display elements (excluding overlap). When 0 uses the amount of available apps."), 
         Min(0)] public int initalElementsAmount = 0;
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
        
        [SerializeField] private ContentDataManager contentDataManager;
        
        private string[] _appOrder;
        private Vector3 rearmostAnchor;
        private Vector3 foremostAnchor;
        private int _selectedElement = 0;
        private int _selectedData = 0;
        private ContentDisplayElement[] _displayElements;
        private ScrollState _scrollState = ScrollState.Stop;
        private Vector3 _moveIncrementDistance = Vector3.zero;
        private float _inputTimeStamp = 0f;
        private ScrollDir _queuedScrollDir = ScrollDir.None;

        // Start is called before the first frame update
        void Start()
        {
            // Use alphabetical order as default.
            _appOrder = contentDataManager.AppNames;
            Array.Sort(_appOrder);
            Debug.Log("App Order: " + String.Join(", ", _appOrder));
            
            FillDisplayElements();
            InitialDisplayElementsSetup();
            
            rearmostAnchor = selectedElementPosition - (elementSpacing * (overlappingElements + 1)) * scrollDirection;
            foremostAnchor = selectedElementPosition +
                             (elementSpacing * (_displayElements.Length - overlappingElements)) * scrollDirection;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (InputHandler.GetButtonDown(DmicButton.P1Up))
            {
                _inputTimeStamp = Time.time;
                _queuedScrollDir = ScrollDir.Forward;
                
                if (_scrollState == ScrollState.Stop)
                {
                    StartMovement(ScrollDir.Forward);
                }
            }
            else if (InputHandler.GetButtonDown(DmicButton.P1Down))
            {
                _inputTimeStamp = Time.time;
                _queuedScrollDir = ScrollDir.Backwards;
                
                if (_scrollState == ScrollState.Stop)
                {
                    StartMovement(ScrollDir.Backwards);
                }
            }
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

        /// TODO doc
        private void StartMovement(ScrollDir scrollDir)
        {
            
            // Advance selection.
            _selectedElement = Mod(_selectedElement + -1 * (int) scrollDir, _displayElements.Length);
            //Debug.Log("_selectedElement: " + _selectedElement);
            _selectedData = Mod(_selectedData + -1 * (int) scrollDir, contentDataManager.AppAmount);
            //Debug.Log("_selectedData: " + _selectedData);
            
            UpdateEdgeElement(scrollDir);

            _scrollState = ScrollState.Accel;
            _moveIncrementDistance = (float) scrollDir * elementSpacing / 2 * scrollDirection;

            MoveAllDisplayElements(_moveIncrementDistance, accelerationType, accelerationTime, UpdateMovement);
        }

        /// Meant to be invoked once after the action of current _moveState is done. TODO doc
        private void UpdateMovement()
        {
            if (_scrollState == ScrollState.Accel)
            {
                // Continue
                if (false && InputHandler.GetButton(DmicButton.P1Up))
                {
                    MoveAllDisplayElements(_moveIncrementDistance, decelerationType, decelerationTime, UpdateMovement); 
                }
                
                // Default: decelerate
                else
                {
                    _scrollState = ScrollState.Decel;

                    MoveAllDisplayElements(_moveIncrementDistance, decelerationType, decelerationTime, UpdateMovement);
                }
            }

            else if (_scrollState == ScrollState.Decel)
            {
                
                // Input buffered
                if (_inputTimeStamp + inputBuffer >= Time.time || InputHandler.GetButton(DmicButton.P1Up) || InputHandler.GetButton(DmicButton.P1Down))
                {
                    StartMovement(_queuedScrollDir);
                } 
                
                // Default: stop at selection
                else
                    _scrollState = ScrollState.Stop;
                
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
                _displayElements[movedElement].transform.position = foremostAnchor;
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
                _displayElements[movedElement].transform.position = rearmostAnchor;
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
            int amount = initalElementsAmount > 1 ? initalElementsAmount : Math.Max(contentDataManager.AppAmount, 2);
            
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
                
                _displayElements[i].transform.position = selectedElementPosition + (scrollDirection * distance);
                // TODO rotation

                // Data
                // Skip data setup when no data is present.
                if (contentDataManager.AppAmount == 0) break;
                
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
            }
            
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
            DmicAppData data = contentDataManager.GetApp(appId);
            
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
        /// loaded app data in <see cref="contentDataManager"/>.
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
            result = Mod(_selectedData + result, contentDataManager.AppAmount);

            return result;
        }
    }
}
