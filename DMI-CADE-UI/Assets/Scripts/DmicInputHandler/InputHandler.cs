using System.Collections.Generic;
using UnityEngine;

namespace DmicInputHandler {

    public enum DmicButton
    {
        P1Start, P1Coin,
        P1Up, P1Down, P1Left, P1Right,
        P1A, P1B, P1C, P1D, P1E, P1F,
        P2Start, P2Coin,
        P2Up, P2Down, P2Left, P2Right,
        P2A, P2B, P2C, P2D, P2E, P2F
    }

    public enum DmicAxis
    {
        P1X, P1Y,
        P2X, P2Y
    }

    public class InputHandler
    {
        // public bool anyKey { get; private set; } = false; TODO add anyKey functionality
        // public bool anyKeyDown { get; private set; } = false; TODO add anyKeyDown functionality

        // TODO add final mappings
        // Keyboard mappings for development.
        private static readonly Dictionary<DmicButton, KeyCode> ButtonMappings = new Dictionary<DmicButton, KeyCode>
        {
            {DmicButton.P1Up,    KeyCode.W},
            {DmicButton.P1Down,  KeyCode.S},
            {DmicButton.P1Left,  KeyCode.A},
            {DmicButton.P1Right, KeyCode.D},
            
            {DmicButton.P1Start, KeyCode.Alpha1},
            {DmicButton.P1Coin,  KeyCode.Alpha5},
            {DmicButton.P1A, KeyCode.U},
            {DmicButton.P1B, KeyCode.I},
            {DmicButton.P1C, KeyCode.O},
            {DmicButton.P1D, KeyCode.J},
            {DmicButton.P1E, KeyCode.K},
            {DmicButton.P1F, KeyCode.L},
            
            {DmicButton.P2Up,    KeyCode.UpArrow},
            {DmicButton.P2Down,  KeyCode.DownArrow},
            {DmicButton.P2Left,  KeyCode.LeftArrow},
            {DmicButton.P2Right, KeyCode.RightArrow},
            
            {DmicButton.P2Start, KeyCode.Alpha2},
            {DmicButton.P2Coin,  KeyCode.Alpha6},
            {DmicButton.P2A, KeyCode.R},
            {DmicButton.P2B, KeyCode.T},
            {DmicButton.P2C, KeyCode.Z},
            {DmicButton.P2D, KeyCode.F},
            {DmicButton.P2E, KeyCode.G},
            {DmicButton.P2F, KeyCode.H},
        };
        
        // TODO doc
        public static bool GetButton(DmicButton button) => Input.GetKey(ButtonMappings[button]);

        // TODO doc
        public static bool GetButtonDown(DmicButton button) => Input.GetKeyDown(ButtonMappings[button]);

        // TODO doc
        public static bool GetButtonUp(DmicButton button) => Input.GetKeyUp(ButtonMappings[button]);
        
        // TODO axis support
        
        // TODO pad support
    }
}
