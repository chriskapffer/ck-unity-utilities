using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChrisKapffer {

	// These are some general enums used with input processing. Most of them are self explanatory.
	// Look at the InputCondition class below to see these enums in action.

	public enum MouseButton {
		Left   = 0,
		Right  = 1,
		Middle = 2
	}

	public enum ButtonState {
		Up,
		Down,
		Pressed,
		Released,
	}

	/// <summary>
	/// Determines how to interpret the scroll wheel value
	/// </summary>
	public enum WheelValue {
		Ignored, // do not make the wheel part of the condition
		Zero, // wheel needs to be untouched to satisfy the condition
		Positive, // condition is met if wheel has just been scrolled up
		Negative, // condition is met if wheel has just been scrolled down
		AnyExceptZero, // condition is met if wheel has just been scrolled no matter the direction
	}

	/// <summary>
	/// Determines how the input condition is constructed
	/// </summary>
	public enum CollectionOperand {
		Any, // only one subcondition needs to true
		All // all subconditions need to be true
	}

	/// <summary>
	/// This class allows you to specify an input condition in the inspector. Instead of using Input.GetKey(KeyCode.WhatEver) use InputCondition.IsMet()
	/// </summary>
	[System.Serializable]
	public class InputCondition {

		private static readonly KeyCode[] osxDefaultModifierKeys = new KeyCode[] { KeyCode.LeftCommand, KeyCode.RightCommand };
		private static readonly KeyCode[] genericDefaultModifierKeys = new KeyCode[] { KeyCode.LeftControl, KeyCode.RightControl };

		public KeyCode[] keysUp;
		public KeyCode[] keysDown;
		public KeyCode[] keysPressed;
		public KeyCode[] keysReleased;

		/// <summary>
		/// If set to true, there will be no distinction between left and right alt, shift, control or command key
		/// </summary>
		public bool treatLeftRightAsSame;
		/// <summary>
		/// wether to check if the os specific modifier keys are currently held down
		/// </summary>
		public bool osDefaultModifierDown;

		public MouseButton[] buttonsUp;
		public MouseButton[] buttonsDown;
		public MouseButton[] buttonsPressed;
		public MouseButton[] buttonsReleased;

		/// <summary>
		/// if the mouse pointer is over a ugui element and ignoreIfOverUI is true all mouse related subconditions won't be processed
		/// </summary>
		public bool ignoreIfOverUI;

		/// <summary>
		/// Determines how to interpret the scroll wheel value
		/// </summary>
		public WheelValue wheelValue;

		/// <summary>
		/// This holds the current scroll wheel value in case some one wants to process it further. e.g. if the scroll amount was small or big
		/// </summary>
		/// <value>The scroll wheel value.</value>
		public float Value { get; private set; }

		/// <summary>
		/// Determines how the input condition is constructed. Either all subconditions have to be true or just one.
		/// </summary>
		public CollectionOperand match;

		/// <summary>
		/// Determines if the overall input condition is satisfied.
		/// </summary>
		/// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
		public bool IsMet() {
			if (match == CollectionOperand.Any) {
				return Any();
			} else if (match == CollectionOperand.All) {
				return All();
			}
			return false;
		}

		/// <summary>
		/// Determines if at least one subcondition is satisfied.
		/// </summary>
		/// <returns><c>true</c> if at least one subcondition is satisfied; otherwise, <c>false</c>.</returns>
		public bool Any() {
			// get the scroll wheel value, process it later
			var wheelDelta = Input.GetAxis("Mouse ScrollWheel");
			Value = wheelDelta;

			// check key conditions
			foreach (var key in keysUp) {
				if (!GetKeyCondition(key, Input.GetKey) && !GetKeyCondition(key, Input.GetKeyDown) && !GetKeyCondition(key, Input.GetKeyUp)) {
					return true;
				}
			}
			foreach (var key in keysDown) {
				if (GetKeyCondition(key, Input.GetKey)) {
					return true;
				}
			}
			foreach (var key in keysPressed) {
				if (GetKeyCondition(key, Input.GetKeyDown)) {
					return true;
				}
			}
			foreach (var key in keysReleased) {
				if (GetKeyCondition(key, Input.GetKeyUp)) {
					return true;
				}
			}

			// check key modifiers
			if (osDefaultModifierDown) {
				if (IsOsModifierDown()) {
					return true;
				}
			}

			// do not process mouse conditions if the pointer is over an ui element
			if (ignoreIfOverUI && IsPointerOverUI()) {
				return false;
			}

			// now check the scroll wheel status
			if (wheelValue != WheelValue.Ignored && IsWheelConditionMet(wheelDelta)) {
				return true;				
			}

			// check mouse button states
			foreach (int button in buttonsUp) {
				if (!Input.GetMouseButton(button) && !Input.GetMouseButtonDown(button) && !Input.GetMouseButtonUp(button)) {
					return true;
				}
			}
			foreach (int button in buttonsDown) {
				if (Input.GetMouseButton(button)) {
					return true;
				}
			}
			foreach (int button in buttonsPressed) {
				if (Input.GetMouseButtonDown(button)) {
					return true;
				}
			}
			foreach (int button in buttonsReleased) {
				if (Input.GetMouseButtonUp(button)) {
					return true;
				}
			}

			// if we've come that far there was not a single subcondition that was met
			return false;
		}

		/// <summary>
		/// Determines if all subcondition is satisfied.
		/// </summary>
		/// <returns><c>true</c> if all subcondition is satisfied; otherwise, <c>false</c>.</returns>
		public bool All() {
			// get the scroll wheel value, process it later
			var wheelDelta = Input.GetAxis("Mouse ScrollWheel");
			Value = wheelDelta;

			// check key conditions
			foreach (var key in keysUp) {
				if (GetKeyCondition(key, Input.GetKey) || GetKeyCondition(key, Input.GetKeyDown) || GetKeyCondition(key, Input.GetKeyUp)) {
					return false;
				}
			}
			foreach (var key in keysDown) {
				if (!GetKeyCondition(key, Input.GetKey)) {
					return false;
				}
			}
			foreach (var key in keysPressed) {
				if (!GetKeyCondition(key, Input.GetKeyDown)) {
					return false;
				}
			}
			foreach (var key in keysReleased) {
				if (!GetKeyCondition(key, Input.GetKeyUp)) {
					return false;
				}
			}

			// check key modifiers
			if (osDefaultModifierDown) {
				if (!IsOsModifierDown()) {
					return false;
				}
			}

			// do not process mouse conditions if the pointer is over an ui element
			if (ignoreIfOverUI && IsPointerOverUI()) {
				return true;
			}

			// now check the scroll wheel status
			if (wheelValue != WheelValue.Ignored && !IsWheelConditionMet(wheelDelta)) {
				return false;
			}
				
			// check mouse button states
			foreach (int button in buttonsUp) {
				if (Input.GetMouseButton(button) || Input.GetMouseButtonDown(button) || Input.GetMouseButtonUp(button)) {
					return false;
				}
			}
			foreach (int button in buttonsDown) {
				if (!Input.GetMouseButton(button)) {
					return false;
				}
			}
			foreach (int button in buttonsPressed) {
				if (!Input.GetMouseButtonDown(button)) {
					return false;
				}
			}
			foreach (int button in buttonsReleased) {
				if (!Input.GetMouseButtonUp(button)) {
					return false;
				}
			}

			// if we've come that far all subconditions have passed the test
			return true;
		}

		/// <summary>
		/// Determines if any of the os specific modifier keys are currently held down.
		/// </summary>
		/// <returns><c>true</c> if is a modifier key is down; otherwise, <c>false</c>.</returns>
		public static bool IsOsModifierDown() {
			#if UNITY_WEBGL
			if (OSXDetector.IsOSX == OSXDetector.Result.NotCheckedYet) {
				// Have a look at OSXDetector for more explanations
				Debug.LogError("OSXDetector hasn't performed its check yet. Call OSXDetector.CheckPlatform(); on application start!");
			}
			#endif
			var modifierKeys = OSXDetector.IsOSX == OSXDetector.Result.Yes ? osxDefaultModifierKeys : genericDefaultModifierKeys;
			return IsAnyKeyCurrentlyDown(modifierKeys);
		}

		/// <summary>
		/// Determines if the mouse pointer is over an ugui element.
		/// </summary>
		/// <returns><c>true</c> if the pointer is over ui; otherwise, <c>false</c>.</returns>
		public static bool IsPointerOverUI() {
			return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
		}

		/// <summary>
		/// Determines whether the current scroll wheel value meets our criteria.
		/// </summary>
		/// <returns><c>true</c> if the condition is met; otherwise, <c>false</c>.</returns>
		/// <param name="value">scroll wheel value.</param>
		private bool IsWheelConditionMet(float value) {
			float threshold = 0.01f;
			bool result = false;
			switch (wheelValue) {
				case WheelValue.Zero:
					result = Mathf.Abs(value) < threshold;
					break;
				case WheelValue.Negative:
					result = value <= -threshold;
					break;
				case WheelValue.Positive:
					result = value >= threshold;
					break;
				case WheelValue.AnyExceptZero:
					result = Mathf.Abs(value) >= threshold;
					break;
			}
			return result;
		}

		/// <summary>
		/// Checks if the given key meets the provided condition.
		/// If left and right keys are to be treated euqally <see cref="treatLeftRightAsSame"/>
		/// we have to check them both.
		/// </summary>
		/// <returns><c>true</c>, if key condition was met, <c>false</c> otherwise.</returns>
		/// <param name="key">KeyCode to perform the test with.</param>
		/// <param name="keyCondition">The condition to check.</param>
		private bool GetKeyCondition(KeyCode key, Func<KeyCode, bool> keyCondition) {
			if (!treatLeftRightAsSame) {
				return keyCondition(key);
			}
			if (key == KeyCode.LeftAlt || key == KeyCode.RightAlt) {
				return keyCondition(KeyCode.LeftAlt) || keyCondition(KeyCode.RightAlt);
			}
			if (key == KeyCode.LeftShift || key == KeyCode.RightShift) {
				return keyCondition(KeyCode.LeftShift) || keyCondition(KeyCode.RightShift);
			}
			if (key == KeyCode.LeftControl || key == KeyCode.RightControl) {
				return keyCondition(KeyCode.LeftControl) || keyCondition(KeyCode.RightControl);
			}
			if (key == KeyCode.LeftCommand || key == KeyCode.RightCommand) {
				return keyCondition(KeyCode.LeftCommand) || keyCondition(KeyCode.RightCommand);
			}
			if (key == KeyCode.LeftApple || key == KeyCode.RightApple) {
				return keyCondition(KeyCode.LeftApple) || keyCondition(KeyCode.RightApple);
			}
			if (key == KeyCode.LeftWindows || key == KeyCode.RightWindows) {
				return keyCondition(KeyCode.LeftWindows) || keyCondition(KeyCode.RightWindows);
			}
			return keyCondition(key);
		}

		/// <summary>
		/// Determines if any key of the given collection is currently down.
		/// </summary>
		/// <returns><c>true</c> if any key is down; otherwise, <c>false</c>.</returns>
		/// <param name="keys">Key collection.</param>
		private static bool IsAnyKeyCurrentlyDown(IEnumerable<KeyCode> keys) {
			foreach (var key in keys) {
				if (Input.GetKey(key)) {
					return true;
				}
			}
			return false;
		}
	}
}
