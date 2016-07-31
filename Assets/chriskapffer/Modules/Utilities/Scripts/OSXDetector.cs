using System;
using UnityEngine;

namespace ChrisKapffer {
	/// <summary>
	/// Determines if the app is running on a mac. That is usefull to know if you have different keyboard layouts.
	/// For example while on windows you want to use the control and on osx you want to use the command key.
	/// Due to the nature of the determination process on webGL there are a couple of different options:
	/// 1) You do not target webGL at all. Use this code:
	/// 	if (OSXDetector.IsOSX == Result.Yes) { doYourThing(); }
	/// 2) You are targeting webGL (either exclusivly or other platforms as well). Use either
	/// 	a)
	/// 		while (OSXDetector.IsOSX == Result.NotCheckedYet) { simplyWaitOrUseACoroutineToAvoidBlockingTheApp(); }
	/// 		if (OSXDetector.IsOSX == Result.Yes) { doYourThing(); }
	/// 	b)
	/// 		OSXDetector.CheckPlatform((result) => {
	/// 			if (OSXDetector.IsOSX == Result.Yes) { doYourThing(); }
	/// 		})
	/// 	c)
	/// 		// call this once, e.g. on Start()
	/// 		OSXDetector.CheckPlatform();
	/// 		// call this throughout your code when you are sure that the check completed
	/// 		if (OSXDetector.IsOSX == Result.Yes) { doYourThing(); }
	/// </summary>
	public class OSXDetector {
		public enum Result {
			No,
			Yes,
			NotCheckedYet,
			Undeterminable,
		};

		private static Result _isOSX = Result.NotCheckedYet;
		public static Result IsOSX {
			get {
				if (_isOSX == Result.NotCheckedYet) {
					#if !UNITY_WEBGL
					// if we are not targeting webGL we simply ask unity which platform we are on, easy peasy
					_isOSX = (Application.platform == RuntimePlatform.OSXPlayer
						   || Application.platform == RuntimePlatform.OSXWebPlayer
						   || Application.platform == RuntimePlatform.OSXDashboardPlayer
						   || Application.platform == RuntimePlatform.OSXEditor)
						? Result.Yes : Result.No;
					#else
					CheckPlatform();
					#endif
				}
				return _isOSX;
			}
			private set {
				_isOSX = value;
			}
		}

		#if !UNITY_WEBGL || UNITY_EDITOR
		/// <summary>
		/// Checks the platform the app is running on.
		/// </summary>
		/// <param name="onPlatformCheckFinished">(optional) a callback which tells us, wether we are on osx or not.</param>
		public static void CheckPlatform(Action<Result> onPlatformCheckFinished = null) {
			// So here we are either on webGL but inside the editor or not on webGL at all. We could have excluded this method
			// for none webGL platforms altogether, but that would mean conditional compilation on your part (if you are targeting
			// webGL and other platforms). so we keept it and also use it in the editor case, where you don't have access to
			// external javascript calls.
			if (IsOSX == Result.NotCheckedYet) {
				// this will only happen if we are target webGL and are in the editor
				IsOSX = (Application.platform == RuntimePlatform.OSXEditor) ? Result.Yes : Result.No;
			}
			if (onPlatformCheckFinished != null) {
				onPlatformCheckFinished.Invoke(IsOSX);
			}
		}
		#else // UNITY_WEBGL && !UNITY_EDITOR
		/// <summary>
		/// Checks the platform the app is running on.
		/// </summary>
		/// <param name="onPlatformCheckFinished">(optional) a callback which tells us, wether we are on osx or not.</param>
		public static void CheckPlatform(Action<Result> onPlatformCheckFinished = null) {
			// So here we are targeting webGL and are not in the editor any more. That means we have to do some trickery by
			// calling external javascript. Unfortunately we javascript response is not instantaneous, thus the need for an
			// action delegate. Also we need a mono behaviour to handle the javascript messaging.

			// NOTICE: do not use IsOSX here, because then you'll be stuck in a loop
			if (_isOSX == Result.NotCheckedYet) {
				if (detectorBehaviour == null) {
					// create our behaviour which handles the communication with external javascript
					detectorBehaviour = new GameObject("OSXDetectorBehaviour").AddComponent<OSXDetectorBehaviour>();
					detectorBehaviour.CheckPlatform((result) => {
						// don't forget to set our static property in order to avoid this process in the future
						IsOSX = result;
						if (onPlatformCheckFinished != null) {
							onPlatformCheckFinished.Invoke(result);
						}
					});
				} else {
					// we already triggered the determination process but haven't recieved a result yet
					if (onPlatformCheckFinished != null) {
						// there is another callback reciever, update the delegate accordingly
						detectorBehaviour.onPlatformCheckFinished = (result) => {
							detectorBehaviour.onPlatformCheckFinished.Invoke(result);
							onPlatformCheckFinished.Invoke(result);
						};
					}
				}
			} else {
				// we already know our platform
				if (onPlatformCheckFinished != null) {
					onPlatformCheckFinished.Invoke(IsOSX);
				}
			}
		}

		/// <summary>
		/// We need a MonoBehaviour in order to recieve messages from external javacsript
		/// </summary>
		private static OSXDetectorBehaviour detectorBehaviour = null;

		private class OSXDetectorBehaviour : MonoBehaviour {
			
			public Action<Result> onPlatformCheckFinished = null;

			/// <summary>
			/// Performs the platform check.
			/// </summary>
			/// <param name="onPlatformCheckFinished">a delegate to be called, once the result is known.</param>
			public void CheckPlatform(Action<Result> onPlatformCheckFinished) {
				// store the delegate for later use
				this.onPlatformCheckFinished = onPlatformCheckFinished;
				Application.ExternalEval(@"
					var isOSX = 'no';
					if (typeof navigator.platform !== 'undefined') {
						isOSX = (navigator.platform.toLowerCase().indexOf('mac') !== -1) ? 'yes' : 'no';
					} else if (typeof navigator.appVersion !== 'undefined') {
						isOSX = (navigator.appVersion.toLowerCase().indexOf('mac') !== -1) ? 'yes' : 'no';
					} else {
						isOSX = 'undeterminable';
					}
					if (typeof SendMessage === 'function') {
						SendMessage('" + name + @"', 'WebGLBrowserCheckedForOSX', isOSX);
					}
				");
			}

			/// <summary>
			/// The method that gets called from javascript. After invoking the stored delegate it destroys the whole game object.
			/// </summary>
			/// <param name="isOSX">platform check result as string.</param>
			public void WebGLBrowserCheckedForOSX(string isOSX) {
				var result = (OSXDetector.Result)System.Enum.Parse(typeof(OSXDetector.Result), isOSX, true);
				if (onPlatformCheckFinished != null) {
					onPlatformCheckFinished.Invoke(result);
				}
				// remove this game object, because we did our job and are not needed anymore :(
				Destroy(this.gameObject);
			}
		}
		#endif
	}
}
