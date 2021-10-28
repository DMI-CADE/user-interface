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
        public BorderedContainer singlePlayerMode;
        public BorderedContainer coopMode;
        public BorderedContainer singleAndCoopMode;
        public BorderedContainer vsMode;

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
                case GameMode.SingleP: singlePlayerMode.Open(true);
                    break;
                case GameMode.Coop: coopMode.Open(true);
                    break;
                case GameMode.SingleAndCoop: singleAndCoopMode.Open(true);
                    break;
                case GameMode.Vs: vsMode.Open(true);
                    break;
            }
        }

        public void DeactivateAllModes()
        {
            singlePlayerMode.Close(true);
            coopMode.Close(true);
            singleAndCoopMode.Close(true);
            vsMode.Close(true);
        }
    }
}
