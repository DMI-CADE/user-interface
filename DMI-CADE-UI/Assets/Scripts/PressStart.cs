using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dmicade
{
    public class PressStart : MonoBehaviour
    {
        public GameObject pressText;
        public GameObject startText;
        
        void Awake()
        {
            DisableTexts();
        }

        public void DisableTexts()
        {
            pressText.SetActive(false);
            startText.SetActive(false);
        }

        /// Toggles active texts. Activates pressText when both are disabled.
        public void ToggleSwitchTexts()
        {
            if (!pressText.activeSelf && !startText.activeSelf)
            {
                pressText.SetActive(true);
                return;
            }

            pressText.SetActive(startText.activeSelf);
            startText.SetActive(!pressText.activeSelf);
        }
    }
}
