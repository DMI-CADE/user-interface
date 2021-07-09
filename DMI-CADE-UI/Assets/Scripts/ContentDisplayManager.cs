using System;
using Pooling;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Dmicade
{
    public class ContentDisplayManager : MonoBehaviour
    {
        [Header("Scroll Settings")]
        public Vector3 selectedElementPosition;
        public Vector3 scrollDirection = Vector3.forward;
        public float elementSpacing = 5;
        [Tooltip("Amount of generated display elements (excluding overlap). When 0 uses the amount of available apps."), 
         Min(0)] public int elementsAmount = 0;
        [Tooltip("Overlapping elements per side."),
         SerializeField, Range(1,5)] private int overlappingElements = 2;

        [Space(10)]

        [SerializeField] private ContentDataManager contentDataManager;
        
        private string[] _appOrder;
        private int _selectedElement = 0;
        private int _selectedData = 0;
        private ContentDisplayElement[] _displayElements;

        // Start is called before the first frame update
        void Start()
        {
            // Use alphabetical order as default.
            _appOrder = contentDataManager.AppNames;
            Array.Sort(_appOrder);
            Debug.Log("App Order: " + String.Join(", ", _appOrder));
            
            FillDisplayElements();
            
            InitialDisplayElementsSetup();
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }

        /// <summary>
        /// Fills the <see cref="_displayElements"/> array with display elements.
        /// </summary>
        private void FillDisplayElements()
        {
            // On 0, default to amount of loaded apps.
            int amount = elementsAmount > 0 ? elementsAmount : contentDataManager.AppAmount;
            
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
                bool isOverlap = i >= visibleElements;
                
                ReadyDisplayElement(i);
                
                if (!isOverlap)
                {
                    _displayElements[i].gameObject.SetActive(true);
                }
                //_displayElements[i].name = _displayElements[i].name + " " + i;
            }
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
