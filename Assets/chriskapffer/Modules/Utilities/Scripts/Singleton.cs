using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChrisKapffer {

    /// <summary>
    /// This is actually a Multiton. The purpose is to be able to access any MonoBehaviour in the same way you would access a Singleton, without
    /// having it to implement an interface or to be subclassed. Use it like this: Singleton.Get<MyCoolBehaviour>()
    /// But be careful. With freedom comes responsibility! Don't throw this around in your code base like crazy. And make sure to not create
    /// other instances of a specific MonoBehaviour once you settled on using it as a singleton.
    /// </summary>
	public class Singleton {
        
        /// <summary>
        /// The instances of singletons of different types. Each one is unique, but they are all stored here in one place.
        /// </summary>
		private static Dictionary<Type, MonoBehaviour> _instances;

        /// <summary>
        /// For the programmer who likes to keep it quite.
        /// </summary>
        private static bool loggingEnabled = false;

        /// <summary>
        /// Used for thread safety
        /// </summary>
		private static object _lock = new object();

        /// <summary>
        /// Get the single instance of your MonoBehaviour subclass. The instance gets created on the fly if it didn't exist before.
        /// An error will occur if there are two instances of this type.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour subclass you want to get an instance of.</typeparam>
		public static T Get<T>() where T : MonoBehaviour {
			lock(_lock) {
				if (_instances == null) {
					_instances = new Dictionary<Type, MonoBehaviour>();
				}

				MonoBehaviour instance;
				if (!_instances.TryGetValue(typeof(T), out instance) || instance == null) {
                    // This type of MonoBehaviour hasn't been registered yet. Try to find it in the scene
					T[]sceneObjects = GameObject.FindObjectsOfType(typeof(T)) as T[];

					if (sceneObjects.Length == 0) {
                        // There is no object of this type. Create one.
						GameObject go = new GameObject();
						go.name = typeof(T).ToString() + " (singleton)";

						instance = go.AddComponent<T>();
						if (instance is IDynamicallyCreatedBehaviour) {
                            // Allow the newly created instance to initialize what ever it needs to function properly.
							((IDynamicallyCreatedBehaviour)instance).OnDynamicCreation();
						}
                        Log("[Singleton] A new instance of " + typeof(T) + " was created.");
                    } else if (sceneObjects.Length == 1) {
                        instance = sceneObjects[0];
                        Log("[Singleton] Register " + typeof(T) + " as singleton.");
                    } else {
                        instance = sceneObjects[0];
                        Log("[Singleton] There are multiple instances of " + typeof(T) + ".", true);
                    }
                    // add the instance to our collection of registered singletons
					_instances[typeof(T)] = instance;
				}

				return instance as T;
			}
		}

        /// <summary>
        /// Prints some log message, if logging is enabled
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="error">(optional) Make this an error if desired.</param>
        private static void Log(string message, bool error = false) {
            if (!loggingEnabled) {
                return;
            }
            if (error) {
                Debug.LogError(message);
            } else {
                Debug.Log(message);
            }
        }
	}

    /// <summary>
    /// Use this interface for any MonoBehaviour subclass which needs to do some custom initialization when created
    /// </summary>
    public interface IDynamicallyCreatedBehaviour {
        void OnDynamicCreation();
    }
}
