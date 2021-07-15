using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{
    public class CdmSyncTimer : MonoBehaviour
    {
        public float tickDelay = 1f;
        
        public event Action OnTick;

        private float _timeStamp;

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= _timeStamp + tickDelay)
            {
                _timeStamp = Time.time;
                
                OnTick?.Invoke();
            }
        }
    }
}
