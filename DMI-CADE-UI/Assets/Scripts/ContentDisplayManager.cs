using System;
using System.Collections;
using System.Collections.Generic;
using Dmicade;
using Pooling;
using UnityEngine;

namespace Dmicade
{
    public class ContentDisplayManager : MonoBehaviour
    {
        public Vector3 SelectedPosition;
        public float ElementDistance = 5;
        public Vector3 scrollDirection = Vector3.back;

        [Space(10)]

        [SerializeField] private ContentDataManager contentDataManager;
        
        private string[] _appOrder;
        private int _currentSelection = 0;
        private ContentDisplayElement[] _displayElements;

        // Start is called before the first frame update
        void Start()
        {
            // Use alphabetical order as default.
            _appOrder = contentDataManager.AppNames;
            Array.Sort(_appOrder);
            Debug.Log("Apps: " + String.Join(", ", _appOrder));
            
            FillDisplayElements();
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }

        private void FillDisplayElements()
        {
            // Add two additional possible upcoming elements.
            _displayElements = new ContentDisplayElement[contentDataManager.AppAmount + 2];
            
            for (int i = 0; i < _displayElements.Length; i++)
            {
                _displayElements[i] = ContentDisplayElementPool.Instance.Get();
            }
        }

        
    }
}
