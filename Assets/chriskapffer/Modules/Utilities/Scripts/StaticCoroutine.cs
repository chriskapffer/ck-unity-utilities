using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChrisKapffer {
    
    /// <summary>
    /// There are cases in which you want to start a coroutine from somewhere outside of a MonoBehaviour.
    /// This class saves you the trouble of creating a MonoBehaviour just so you can start your coroutine.
    /// </summary>
    public static class StaticCoroutine {

        /// <summary>
        /// The behaviour instance.
        /// </summary>
        private static StaticCoroutineBehaviour _behaviourInstance = null;

        /// <summary>
        /// Gets the behaviour instance. This is a singleton implementation.
        /// </summary>
        /// <value>The behaviour instance.</value>
        private static StaticCoroutineBehaviour BehaviourInstance {
            get {
                lock (_lock) {
                    if (_behaviourInstance == null) {
                        string name = typeof(StaticCoroutineBehaviour).ToString();
                        var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
                        _behaviourInstance = go.AddComponent<StaticCoroutineBehaviour>();
                    }
                    return _behaviourInstance;
                }
            }
        }

        /// <summary>
        /// Used for thread safety
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// Starts the specified coroutine with the help of our private MonoBehaviour instance.
        /// </summary>
        /// <param name="coroutine">The coroutine you want to start.</param>
        public static Coroutine Start(IEnumerator coroutine) {
            return BehaviourInstance.StartForeignCoroutine(coroutine);
        }

        /// <summary>
        /// Stops the specified coroutine with the help of our private MonoBehaviour instance.
        /// </summary>
        /// <param name="coroutine">The coroutine you want to stop.</param>
        public static void Stop(IEnumerator coroutine) {
            BehaviourInstance.StopForeignCoroutine(coroutine);
        }

        /// <summary>
        /// Stops the specified coroutine with the help of our private MonoBehaviour instance.
        /// </summary>
        /// <param name="coroutine">The coroutine you want to stop.</param>
        public static void Stop(Coroutine coroutine) {
            BehaviourInstance.StopForeignCoroutine(coroutine);
        }

        /// <summary>
        /// Waits for the end of the current frame and then invokes the given action.
        /// </summary>
        /// <returns>A coroutine object to keep track of. E.g. when aborting the whole thing.</returns>
        /// <param name="action">The action to invoke after the current frame has finshed.</param>
        public static Coroutine DelayUntilEndOfFrame(Action action) {
            return BehaviourInstance.StartForeignCoroutine(FrameDelayedCoroutine(action, 0));
        }

        /// <summary>
        /// Waits the specified number of frames before invoking the given action.
        /// </summary>
        /// <returns>A coroutine object to keep track of. E.g. when aborting the whole thing.</returns>
        /// <param name="action">The action to invoke after the specified number of frames.</param>
        /// <param name="frames">Number of frames to wait before invoking the given action.</param>
        public static Coroutine DelayUsingFrames(Action action, int frames) {
            return BehaviourInstance.StartForeignCoroutine(FrameDelayedCoroutine(action, frames));
        }

        /// <summary>
        /// Waits the specified amount of time (in seconds) until invoking the given action.
        /// </summary>
        /// <returns>A coroutine object to keep track of. E.g. when aborting the whole thing.</returns>
        /// <param name="action">The action to invoke after the specified amount of time has passed.</param>
        /// <param name="time">Time to wait in seconds.</param>
        public static Coroutine DelayUsingTime(Action action, float time) {
            return BehaviourInstance.StartForeignCoroutine(TimeDelayedCoroutine(action, time));
        }

        /// <summary>
        /// A coroutine that waits a specified number of frames before invoking the given action.
        /// </summary>
        /// <returns>IEnumerator</returns>
        /// <param name="action">The action to invoke after the specified number of frames.</param>
        /// <param name="delay">Number of frames to wait for.</param>
        public static IEnumerator FrameDelayedCoroutine(Action action = null, int delay = 1) {
            if (delay == 0) {
                // no delay, wait for the end of current frame only
                yield return new WaitForEndOfFrame();
            } else {
                for (int i = 0; i < delay; i++) {
                    // keep on waiting
                    yield return null;
                }
            }
            if (action != null) {
                // now do your thing
                action.Invoke();
            }
        }

        /// <summary>
        /// A coroutine that waits a specified number of frames before manually starting a subroutine.
        /// </summary>
        /// <returns>IEnumerator</returns>
        /// <param name="func">The subroutine to start after the specified number of frames.</param>
        /// <param name="delay">Number of frames to wait for.</param>
        public static IEnumerator FrameDelayedSubroutine(Func<IEnumerator> func = null, int delay = 1) {
            if (delay == 0) {
                // no delay, wait for the end of current frame only
                yield return new WaitForEndOfFrame();
            } else {
                for (int i = 0; i < delay; i++) {
                    // keep on waiting
                    yield return null;
                }
            }
            if (func == null) {
                // nothing to do, stop here
                yield break;
            }
            var subroutine = func();
            if (subroutine != null) {
                // manually execute the subroutine
                while (subroutine.MoveNext()) {
                    yield return subroutine.Current;
                }
            }
        }

        /// <summary>
        /// A coroutine that waits a specified amount of time before invoking the given action.
        /// </summary>
        /// <returns>IEnumerator</returns>
        /// <param name="action">The action to invoke after the specified amount of time has passed.</param>
        /// <param name="delay">Time in seconds to wait for.</param>
        public static IEnumerator TimeDelayedCoroutine(Action action = null, float delay = 1) {
            yield return new WaitForSeconds(delay);
            if (action != null) {
                action.Invoke();
            }
        }

        /// <summary>
        /// A coroutine that waits a specified amount of time before manually starting a subroutine.
        /// </summary>
        /// <returns>IEnumerator</returns>
        /// <param name="func">The subroutine to start after the specified amount of time has passed.</param>
        /// <param name="delay">Time in seconds to wait for.</param>
        public static IEnumerator TimeDelayedSubroutine(Func<IEnumerator> func = null, float delay = 1) {
            yield return new WaitForSeconds(delay);
            if (func == null) {
                // nothing to do, stop here
                yield break;
            }
            var subroutine = func();
            if (subroutine != null) {
                // manually execute the subroutine
                while (subroutine.MoveNext()) {
                    yield return subroutine.Current;
                }
            }
        }

        /// <summary>
        /// This is our MonoBehaviour used to dispatch our coroutines
        /// </summary>
        private class StaticCoroutineBehaviour : MonoBehaviour {
            /// <summary>
            /// This is used to keep things sane.
            /// </summary>
            private bool dontStart;

            /// <summary>
            /// Starts a foreign coroutine.
            /// </summary>
            /// <returns>The coroutine object to be able to keep track of.</returns>
            /// <param name="coroutine">The coroutine you want to start.</param>
            public Coroutine StartForeignCoroutine(IEnumerator coroutine) {
                // make sure to not start the coroutine if not appropriate (e.g. when the app is quitting)
                if (dontStart) {
                    return null;
                }
                return StartCoroutine(coroutine);
            }

            /// <summary>
            /// Stops a foreign coroutine.
            /// </summary>
            /// <param name="coroutine">The coroutine you want to stop.</param>
            public void StopForeignCoroutine(IEnumerator coroutine) {
                StopCoroutine(coroutine);
            }

            /// <summary>
            /// Stops a foreign coroutine.
            /// </summary>
            /// <param name="coroutine">The coroutine you want to stop.</param>
            public void StopForeignCoroutine(Coroutine coroutine) {
                StopCoroutine(coroutine);
            }

            void OnEnable() {
                dontStart = false;
            }

            void OnDisable() {
                dontStart = true;
            }

            void OnApplicationQuit() {
                dontStart = true;
            }
        }
    }
}
