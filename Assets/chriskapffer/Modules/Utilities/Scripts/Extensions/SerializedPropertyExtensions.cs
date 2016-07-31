using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace ChrisKapffer {
	public static partial class SerializedPropertyExtensions {

		/// <summary>
		/// Gets the value of this serialized property.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="property">The property.</param>
		public static object GetValue(this SerializedProperty property) {
			object result = null;
			switch (property.propertyType) {
				case SerializedPropertyType.AnimationCurve:
					result = property.animationCurveValue;
					break;
				case SerializedPropertyType.ArraySize:
					result = property.intValue;
					break;
				case SerializedPropertyType.Boolean:
					result = property.boolValue;
					break;
				case SerializedPropertyType.Bounds:
					result = property.boundsValue;
					break;
				case SerializedPropertyType.Character:
					result = (char)property.intValue;
					break;
				case SerializedPropertyType.Color:
					result = property.colorValue;
					break;
				case SerializedPropertyType.Enum:
					// Caution: this only returns the string representation of the enum value
					// use Enum.Parse to get the actual enum value
					result = property.enumNames[property.enumValueIndex];
					break;
				case SerializedPropertyType.Float:
					result = property.floatValue;
					break;
				case SerializedPropertyType.Generic:
					result = property.GetGenericValue();
					break;
				case SerializedPropertyType.Gradient:
					result = property.GetGradientValue();
					break;
				case SerializedPropertyType.Integer:
					result = property.intValue;
					break;
				case SerializedPropertyType.LayerMask:
					result = property.intValue;
					break;
				case SerializedPropertyType.ObjectReference:
					result = property.objectReferenceValue;
					break;
				case SerializedPropertyType.Quaternion:
					result = property.quaternionValue;
					break;
				case SerializedPropertyType.Rect:
					result = property.rectValue;
					break;
				case SerializedPropertyType.String:
					result = property.stringValue;
					break;
				case SerializedPropertyType.Vector2:
					result = property.vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					result = property.vector3Value;
					break;
				case SerializedPropertyType.Vector4:
					result = property.vector4Value;
					break;
				default:
					result = null;
					break;
			}
			return result;
		}

		/// <summary>
		/// Gets the gradient value. Unfortunately there's no build in getter for the gradient value.
		/// So we have to use some reflection.
		/// </summary>
		/// <returns>The gradient value.</returns>
		/// <param name="property">The property to get the gradient value of.</param>
		public static Gradient GetGradientValue(this SerializedProperty property) {
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", bindingFlags);
			if (propertyInfo == null) {
				return null;
			}
			return propertyInfo.GetValue(property, null) as Gradient;
		}

		/// <summary>
		/// Gets the value of a generic type. Unfortunately there's no build in getter for this.
		/// So we have to use some reflection.
		/// </summary>
		/// <returns>The value of this property.</returns>
		/// <param name="property">The property to get the value of.</param>
		public static object GetGenericValue(this SerializedProperty property) {
			var genericType = property.GetValueType();
			if (genericType == null) {
				return null;
			}
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			object result = Activator.CreateInstance(genericType);
			SerializedProperty iterator = property.Copy();
			while (iterator.NextVisible(true) && iterator.propertyPath.StartsWith(property.name)) {
				if (iterator.depth > 1) {
					continue;
				}
				var fieldInfo = genericType.GetField(iterator.name, bindingFlags);
				if (fieldInfo != null && fieldInfo.FieldType == iterator.GetValueType()) {
					fieldInfo.SetValue(result, iterator.GetValue());
				}
			}
			return result;
		}

		/// <summary>
		/// Extension method to get the value of this enum-typed property.
		/// </summary>
		/// <returns>The enum value.</returns>
		/// <param name="property">The Property.</param>
		/// <param name="enumType">The type of the enum.</param>
		public static object GetEnumValue(this SerializedProperty property, Type enumType) {
			if (property.GetValueType() != typeof(Enum)) {
				throw new InvalidOperationException(string.Format("Property '{0}' is not an enum.", property.name));
			}
			return Enum.Parse(enumType, property.enumNames[property.enumValueIndex]);
		}

		/// <summary>
		/// Extension method to get a type object out of this property.
		/// </summary>
		/// <returns>The value type.</returns>
		/// <param name="property">The Property.</param>
		public static Type GetValueType(this SerializedProperty property) {
			Type result = typeof(object);
			switch (property.propertyType) {
				case SerializedPropertyType.AnimationCurve:
					result = typeof(AnimationCurve);
					break;
				case SerializedPropertyType.ArraySize:
					result = typeof(int);
					break;
				case SerializedPropertyType.Boolean:
					result = typeof(bool);
					break;
				case SerializedPropertyType.Bounds:
					result = typeof(Bounds);
					break;
				case SerializedPropertyType.Character:
					result = typeof(char);
					break;
				case SerializedPropertyType.Color:
					result = typeof(Color);
					break;
				case SerializedPropertyType.Enum:
					result = typeof(Enum);
					break;
				case SerializedPropertyType.Float:
					result = typeof(float);
					break;
				case SerializedPropertyType.Generic:
					result = GetGenericType(property.type);
					break;
				case SerializedPropertyType.Gradient:
					result = typeof(Gradient);
					break;
				case SerializedPropertyType.Integer:
					result = typeof(int);
					break;
				case SerializedPropertyType.LayerMask:
					result = typeof(int);
					break;
				case SerializedPropertyType.ObjectReference:
					result = typeof(UnityEngine.Object);
					break;
				case SerializedPropertyType.Quaternion:
					result = typeof(Quaternion);
					break;
				case SerializedPropertyType.Rect:
					result = typeof(Rect);
					break;
				case SerializedPropertyType.String:
					result = typeof(string);
					break;
				case SerializedPropertyType.Vector2:
					result = typeof(Vector2);
					break;
				case SerializedPropertyType.Vector3:
					result = typeof(Vector3);
					break;
				case SerializedPropertyType.Vector4:
					result = typeof(Vector4);
					break;
				default:
					result = typeof(object);
					break;
			}
			return result;
		}

		// 

		/// <summary>
		/// Cached list of assembly name templates. <see cref="GetAssemblyQualifiedNameTemplate"/>
		/// </summary>
		private static string[] _assemblyQualifiedNameTemplates = null;
		private static string[] AssemblyQualifiedNameTemplates {
			get {
				if (_assemblyQualifiedNameTemplates == null) {
					_assemblyQualifiedNameTemplates = AppDomain.CurrentDomain.GetAssemblies()
						.Select(a => GetAssemblyQualifiedNameTemplate(a.FullName)).ToArray();
				}
				return _assemblyQualifiedNameTemplates;
			}
		}

		/// <summary>
		/// Creates a 'template' out of an assembly name. This templates contains a placeholder for the type name to insert into.
		/// </summary>
		/// <returns>The template for a fully qualified type name, based on the provided assembly.</returns>
		/// <param name="fullAssemblyName">Full assembly name.</param>
		private static string GetAssemblyQualifiedNameTemplate(string fullAssemblyName) {
			var namespaceName = fullAssemblyName.Substring(0, fullAssemblyName.IndexOf(','));
			return string.Format("{0}.{{0}}, {1}", namespaceName, fullAssemblyName);
		}

		/// <summary>
		/// Gets the a generic type by its name.
		/// </summary>
		/// <returns>The generic type.</returns>
		/// <param name="typeName">The type name.</param>
		private static Type GetGenericType(string typeName) {
			Type result = Type.GetType(typeName);
			if (result != null) {
				return result;
			}
			// no luck so far, try to find the type by building a fully qualified name out of each assembly
			foreach (var nameTemplate in AssemblyQualifiedNameTemplates) {
				result = Type.GetType(string.Format(nameTemplate, typeName));
				if (result != null) {
					return result;
				}
			}
			Debug.LogWarning(string.Format("Unable to find type '{0}'.", typeName));
			return null;
		}
	}
}
#endif
