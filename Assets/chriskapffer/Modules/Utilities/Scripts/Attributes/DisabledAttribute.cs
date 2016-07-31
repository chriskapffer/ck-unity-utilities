using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChrisKapffer {
	/// <summary>
	/// This attribute makes the underlying property not being editable in the inspector.
	/// Use it together with [SerializeField] to be able to monitor private variables in the editor
	/// without being able to change them.
	/// </summary>
	public class DisabledAttribute : PropertyAttribute {

	}
	
	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(DisabledAttribute))]
	public class DisabledAttributeDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginDisabledGroup(true);
			// FIXME: doesn't work with list or array. While the elements are disabled the size of the
			// collection can be changed.
			EditorGUI.PropertyField(position, property);
			EditorGUI.EndDisabledGroup();
		}
	}
	#endif
}
