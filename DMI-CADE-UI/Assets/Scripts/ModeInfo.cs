using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dmicade
{
    public class ModeInfo : MonoBehaviour
    {
        public GameObject singlePlayerMode;
        public GameObject coopMode;
        public GameObject singleAndCoopMode;
        public GameObject vsMode;

        private GameMode[] _availableModes;
        private bool _cycleModes;
        private int _displayedMode;
        
        private void Awake()
        {
            DeactivateAllModes();
        }

        public void UpdateModeInfo(GameMode[] modes)
        {
            _availableModes = modes;
            
            _cycleModes = modes.Length > 1;
            _displayedMode = 0;
            
            DisplayMode(modes[_displayedMode]);
        }

        public void ShowNextMode()
        {
            if (_cycleModes)
            {
                DisplayMode(_availableModes[_displayedMode]);
                _displayedMode = (_displayedMode + 1) % _availableModes.Length; // Increment afterwards to skip first tick.
            }
        }
        
        private void DisplayMode(GameMode mode)
        {
            DeactivateAllModes();
            
            switch (mode)
            {
                case GameMode.SingleP: singlePlayerMode.SetActive(true);
                    break;
                case GameMode.Coop: coopMode.SetActive(true);
                    break;
                case GameMode.SingleAndCoop: singleAndCoopMode.SetActive(true);
                    break;
                case GameMode.Vs: vsMode.SetActive(true);
                    break;
            }
        }

        private void DeactivateAllModes()
        {
            singlePlayerMode.SetActive(false);
            coopMode.SetActive(false);
            singleAndCoopMode.SetActive(false);
            vsMode.SetActive(false);
        }
    }
}
