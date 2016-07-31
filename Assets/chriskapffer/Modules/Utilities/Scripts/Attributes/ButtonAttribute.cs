using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChrisKapffer {
	/// <summary>
	/// This attribute can be used to create a button in the inspector.
	/// Due of its nature of being a PropertyAttribute you also need a property
	/// which doesn't really do anything execpt providing the button functionality.
	/// Typical usage may look like this:
	/// 
	/// [Button("NameOfMethodToInvoke", "ButtonCaption")]
	/// public bool buttonDummy;
	/// 
	/// The type of the property doesn't matter at all. You could also use a private
	/// property like this
	/// 
	/// [SerializeField, Button("NameOfMethodToInvoke", "ButtonCaption")]
	/// private bool buttonDummy;
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ButtonAttribute : PropertyAttribute {
		public readonly string targetMethodName;
		public readonly string buttonName;
		public readonly float buttonWidth;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChrisKapffer.ButtonAttribute"/> class.
		/// </summary>
		/// <param name="methodName">Name of the Method to execute on button press.</param>
		/// <param name="buttonName">(optional) Caption of the button shown in the inspector. If set to null it will use the method name instead.</param>
		/// <param name="width">(optional) Width of the button in pixels. If set to 0 it will use the full width of the inspector window.</param>
		public ButtonAttribute(string methodName, string buttonName = null, float width = 0) {
			this.targetMethodName = methodName;
			this.buttonName = buttonName ?? methodName;
			this.buttonWidth = width;
		}
	}
	
	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ButtonAttribute))]
	public class InspectorButtonPropertyDrawer : PropertyDrawer {
		private MethodInfo targetMethodInfo = null;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			ButtonAttribute inspectorButtonAttribute = (ButtonAttribute)attribute;
			float buttonWidth = inspectorButtonAttribute.buttonWidth;
			Rect buttonRect = position;
			if (buttonWidth > 0) {
				buttonRect = new Rect(position.x + (position.width - buttonWidth) * 0.5f, position.y, buttonWidth, position.height);
			}

			if (GUI.Button(buttonRect, inspectorButtonAttribute.buttonName)) {
				Object targetObject = property.serializedObject.targetObject;
				string targetMethodName = inspectorButtonAttribute.targetMethodName;
				
				if (targetMethodInfo == null) {
					targetMethodInfo = targetObject.GetType().GetMethod(targetMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				if (targetMethodInfo != null) {
					targetMethodInfo.Invoke(targetObject, null);
				} else {
					Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", targetMethodName, targetObject.name));
				}
			}
		}
	}
	#endif
}
