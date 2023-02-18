using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Return InputAxis from different controls like keyboard and mouse at the same time
    /// </summary>
    public struct MMV_Input
    {
        /// <summary>
        /// Get axis of two different inputs
        /// </summary>
        /// <param name="keyboard">
        /// Keyboard axis input
        /// </param>
        /// <param name="gamepad">
        /// Gamepad axis input
        /// </param>
        /// <param name="invertKeyboardAxis">
        /// Whether the keyboard input axis value should be inverted
        /// </param>
        /// <param name="invertGamepadAxis">
        /// Whether the gamepad input axis value should be inverted
        /// </param>
        /// <returns>
        /// Axis force
        /// </returns>
        public float AxisValue(string keyboard, string gamepad, out bool isUsingGamepad, bool invertKeyboardAxis = false, bool invertGamepadAxis = false)
        {
            var _keyboardNull = String.IsNullOrEmpty(keyboard);
            var _gamepadNull = String.IsNullOrEmpty(gamepad);

            var _keyboardAxis = !_keyboardNull ? Input.GetAxis(keyboard) : 0;
            var _gamepadAxis = !_gamepadNull ? Input.GetAxis(gamepad) : 0;

            if (invertKeyboardAxis) _keyboardAxis *= -1;
            if (invertGamepadAxis) _gamepadAxis *= -1;

            isUsingGamepad = _gamepadAxis != 0;

            return Mathf.Clamp(_keyboardAxis + _gamepadAxis, -1, 1);
        }

        /// <summary>
        /// Check if you are pricing any input (mouse / keyboard / gamepad)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// If is pressing
        /// </returns>
        public bool IsPressing(KeyCode a, KeyCode b)
        {
            return Input.GetKey(a) || Input.GetKey(b);
        }

        /// <summary>
        /// Check if you clicked on any input key (mouse / keyboard / gamepad)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>
        /// If it was clicked
        /// </returns>
        public bool Press(KeyCode a, KeyCode b)
        {
            return Input.GetKeyDown(a) || Input.GetKeyDown(b);
        }
    }
}
