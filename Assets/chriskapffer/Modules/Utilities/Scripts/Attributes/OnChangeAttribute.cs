using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChrisKapffer {
	/// <summary>
	/// This attribute enables you to invoke any method when the underlying property value changes
	/// </summary>
	public class OnChangeAttribute : PropertyAttribute {
		
		public readonly string[] callbackNames;
		public readonly bool canInvokeIfApplicationIsNotPlaying;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChrisKapffer.OnChangeAttribute"/> class.
		/// </summary>
		/// <param name="callbackNames">Names of the methods to invoke on property value change.</param>
		/// <param name="canInvokeIfApplicationIsNotPlaying">(optional) If set to <c>true</c> the target methods can be invoked even if not in play mode.</param>
		public OnChangeAttribute(string[] callbackNames, bool canInvokeIfApplicationIsNotPlaying = true) {
			this.callbackNames = callbackNames;
			this.canInvokeIfApplicationIsNotPlaying = canInvokeIfApplicationIsNotPlaying;
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChrisKapffer.OnChangeAttribute"/> class.
		/// </summary>
		/// <param name="callbackName">name ot the method to invoke on property value.</param>
		/// <param name="canInvokeIfApplicationIsNotPlaying">(optional) If set to <c>true</c> the target methods can be invoked even if not in play mode.</param>
		public OnChangeAttribute(string callbackName, bool canInvokeIfApplicationIsNotPlaying = true)
		: this(new string[] { callbackName }, canInvokeIfApplicationIsNotPlaying) { }
	}
	
	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(OnChangeAttribute))]
	public class OnChangeAttributeDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(position, property, label, true);
			if (EditorGUI.EndChangeCheck()) {
				if (!Application.isPlaying && !((OnChangeAttribute)attribute).canInvokeIfApplicationIsNotPlaying) {
					return;
				}
				// if the value of this property changed we try to find a setter or method
				// with a matching name and signature and invoke it
				if (BelongsToMonoBehaviour(property)) {
					object propValue = property.GetValue();
					Type propType = property.GetValueType();
					var targetObject = property.serializedObject.targetObject;
					// since it it possible to invoke multiple methods, we try to find each one of them
					foreach (var callbackName in ((OnChangeAttribute)attribute).callbackNames) {
						// first we try property setters (here property refers to the c# thing, not a SerializedProperty)
						if (!TrySetPropertyValue(targetObject, callbackName, propType, propValue)) {
							// if there was none we try to find and invoke a method
							if (!TryInvokeMethod(targetObject, callbackName, propType, propValue)) {
								Debug.LogWarning(string.Format("Inspector OnChange: Unable to find method or property {0} in {1}", callbackName, targetObject.name));
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Tries to invoke a property setter by its name.
		/// </summary>
		/// <returns><c>true</c>, if a matching property setter was found and invoked, <c>false</c> otherwise.</returns>
		/// <param name="targetObject">The monobehaviour the property belongs to.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="paramType">Type of the value we want to feed the setter with.</param>
		/// <param name="paramValue">The value we want to feed the setter with.</param>
		private static bool TrySetPropertyValue(object targetObject, string propertyName, Type paramType, object paramValue) {
			var propertyInfo = targetObject.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (propertyInfo != null) {
				// we did find a property with a matching name, now try invoke its setter
				var setterMethodInfo = propertyInfo.GetSetMethod(true);
				if (setterMethodInfo != null) {
					return TryInvokeMethod(targetObject, setterMethodInfo, paramType, paramValue);
				}
				Debug.LogWarning(string.Format("Inspector OnChange: Property '{0}' is read only or does not have a setter.", propertyName));
			}
			return false;
		}

		/// <summary>
		/// Tries to invoke a method by its name.
		/// </summary>
		/// <returns><c>true</c>, if a matching method was found and invoked, <c>false</c> otherwise.</returns>
		/// <param name="targetObject">The monobehaviour the method belongs to.</param>
		/// <param name="methodName">Name of the Method to invoke.</param>
		/// <param name="paramType">Type of the value we want to pass to the method.</param>
		/// <param name="paramValue">The value we want to pass to the method.</param>
		private static bool TryInvokeMethod(object targetObject, string methodName, Type paramType, object paramValue) {
			var methodInfo = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo != null) {
				// we did find a method with a matching name, now try to invoke it
				return TryInvokeMethod(targetObject, methodInfo, paramType, paramValue);
			}
			return false;
		}

		/// <summary>
		/// Tries to invoke a previously name matched method. No we have to check if the signature matches.
		/// If so, we finally invoke it.
		/// </summary>
		/// <returns><c>true</c>, if method was invoked, <c>false</c> otherwise.</returns>
		/// <param name="targetObject">The monobehaviour the method belongs to.</param>
		/// <param name="methodInfo">methodinfo object.</param>
		/// <param name="paramType">Type of the value we want to pass to the method.</param>
		/// <param name="paramValue">The value we want to pass to the method.</param>
		private static bool TryInvokeMethod(object targetObject, MethodInfo methodInfo, Type paramType, object paramValue) {
			var paramInfo = methodInfo.GetParameters();
			if (paramInfo.Length == 0) {
				// although it has no parameters we still invoke it. If the user has explicitly written its name out
				// then it is most likely that he/she wants to execute it and doesn't care for the changed value of the SerializedProperty
				methodInfo.Invoke(targetObject, new object[0]);
				return true;
			}
			if (paramInfo.Length == 1) {
				var requiredType = paramInfo[0].ParameterType;
				if (requiredType == paramType) {
					// the method parameter type matches with the type of the value of the SerializedProperty, awesome!
					methodInfo.Invoke(targetObject, new object[] { paramValue });
					return true;
				}
				if (requiredType.IsSubclassOf(paramType)) {
					if (paramType == typeof(Enum)) {
						// SerializedProperty can not determine the exact enum type. It only knows that its holding an enum.
						// So we try to get an enum value out of our string value of SerializedProperty and hope for the best
						try {
							methodInfo.Invoke(targetObject, new object[] { Enum.Parse(requiredType, paramValue as String) });
							return true;
						} catch(ArgumentException ex) {
							// the string could not be converted to a value of the required enum to call this method
							Debug.LogWarning(ex.Message);
						}
					} else {
						// FIXME: this might be dangerous, because we are downcasting without knowing what we are dealing with
						// better do nothing here unless there is a real use case for this
						//methodInfo.Invoke(targetObject, new object[] { paramValue });
					}

				}
			}
			if (paramInfo.Length == 2) {
				//TODO: nice to have: allow methods with two parameters, where the second one is a string. That way we could pass
				// the name of the SerializedProperty where the whole thing originated from.				
			}
			Debug.LogWarning(string.Format("Inspector OnChange: Method '{0}' does not match required signature.", methodInfo.Name));
			return false;
		}

		private static bool BelongsToMonoBehaviour(SerializedProperty property) {
			return property.serializedObject.targetObject.GetType().IsSubclassOf(typeof(MonoBehaviour));
		}
	}
	#endif
}
