using UnityEngine;
using System.Collections.Generic;

namespace ChrisKapffer {

	/// <summary>
	/// This class enables you to print stuff on the screen instead to the console.
	/// It uses the legacy ui to keep it lightweight.
	/// 
	/// Use it like Singleton.Get<ScreenLog>().Print("key", value);
	/// </summary>
	[RequireComponent(typeof (GUIText))]
	public class ScreenLog : MonoBehaviour, IDynamicallyCreatedBehaviour {

		/// <summary>
		/// Set or get the visibility of the screen log.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible {
			get {
				return GuiText.enabled;
			}
			set {
				GuiText.enabled = value;
			}
		}

		/// <summary>
		/// The GUIText component.
		/// </summary>
		private GUIText _guiText;
		private GUIText GuiText {
			get {
				if (_guiText == null) {
					_guiText = GetComponent<GUIText>();
				}
				return _guiText;
			}
		}

		/// <summary>
		/// Stores key value pairs to be able to override previously logged
		/// variables (identified by a string key) with a new value
		/// </summary>
		private IDictionary<string, string> lines;

		/// <summary>
		/// Some styling when created dynamically via Singleton.Get<ScreenLog>()
		/// To customize this you can always create a single instance of ScreenLog
		/// in the editor and set all its properties in the inspector.
		/// </summary>
		public void OnDynamicCreation() {
			transform.position = new Vector3(0.1f, 0.9f, 0.0f);
			var guiText = GuiText;
			guiText.fontSize = 10 + Screen.height / 100;
			guiText.color = Color.magenta;
			guiText.enabled = true;
		}

		/// <summary>
		/// Toggles the visibility of the screen log.
		/// </summary>
		public void Toggle() {
			var guiText = GuiText;
			guiText.enabled = !guiText.enabled;
		}

		/// <summary>
		/// Prints a string to the screen. Typically you use key value pairs
		/// but this allows you to print an arbitrary string like a heading.
		/// If you want to print or log a variables value use
		/// Print(string key, string message) instead.
		/// </summary>
		/// <param name="key">The string to print.</param>
		public void Print(string key) {
			Print(key, string.Empty);
		}

		/// <summary>
		/// Print something on the screen, e.g. a variable name and its value.
		/// It will appear on the screen as "<Key>: <Value>"
		/// If the same key is reused on subsequent calles the "value" part will be overriden,
		/// thus being able to monitor a variable over time.
		/// </summary>
		/// <param name="key">A key to identify this message in order to change it later.</param>
		/// <param name="message">Some arbitrary string content.</param>
		public void Print(string key, string message) {
			if (lines == null) {
				lines = new Dictionary<string, string>();
			}
			lines[key] = message;
			var guiText = GuiText;
			guiText.text = string.Empty;
			foreach(var kv in lines) {
				guiText.text += kv.Key + ": " + kv.Value + "\n";
			}
		}

		// the following are just convenience methods for some commonly used types
		// to avoid casting to and formatting of string
		#region convenience methods

		public void Print(string key, bool value) {
			Print(key, value.ToString());
		}

		public void Print(string key, int value) {
			Print(key, value.ToString());
		}

		public void Print(string key, double value) {
			Print(key, value, 3);
		}

		public void Print(string key, double value, int precision) {
			Print(key, value.ToString("F" + precision.ToString()));
		}

		public void Print(string key, Vector2 value) {
			Print(key, value, 3);
		}

		public void Print(string key, Vector2 value, int precision) {
			Print(key, value.ToString("F" + precision.ToString()));
		}

		public void Print(string key, Vector3 value) {
			Print(key, value, 3);
		}

		public void Print(string key, Vector3 value, int precision) {
			Print(key, value.ToString("F" + precision.ToString()));
		}

		public void Print(string key, Quaternion value) {
			Print(key, value, 3);
		}
		
		public void Print(string key, Quaternion value, int precision) {
			Print(key, value.ToString("F" + precision.ToString()));
		}

		#endregion
	}
}
