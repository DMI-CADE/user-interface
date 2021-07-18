using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{
    [RequireComponent(typeof(ContentDisplayManager), typeof(CdmSyncTimer))]
    public class PressStartController : MonoBehaviour
    {
        public PressStart pressStartTexts;
        
        private ContentDisplayManager _cdm;
        private CdmSyncTimer _cdmTimer;
        private bool _toggleIsActive;
        private int _toggleTickSkip;

        private void Awake()
        {
            _cdm = GetComponent<ContentDisplayManager>();
            _cdmTimer = GetComponent<CdmSyncTimer>();

            _cdm.OnScrollEnable += StartToggle;
            _cdm.OnScrollDisable += StopToggle;
            _cdm.OnScrollStart += StopToggle;
            _cdm.OnScrollEnd += StartToggle;
            _cdm.OnScrollContinueStop += StartToggle;
            
            _cdmTimer.OnTick += ToggleTick;
        }

        private void StartToggle(Vector3 d)
        {
            _toggleTickSkip = 0;
            _toggleIsActive = true;
        }
        
        private void StartToggle() => StartToggle(Vector3.zero);
        
        private void StopToggle(Vector3 d)
        {
            _toggleTickSkip = 0;
            _toggleIsActive = false;
            pressStartTexts.DisableTexts();
        }
        
        private void StopToggle() => StopToggle(Vector3.zero);

        private void ToggleTick()
        {
            if (!_toggleIsActive) return;
            
            if(_toggleTickSkip == 1)
                pressStartTexts.ToggleSwitchTexts();
            else
                _toggleTickSkip++;
        }
    }
}
