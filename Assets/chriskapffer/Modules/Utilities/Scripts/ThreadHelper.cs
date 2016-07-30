using UnityEngine;
using System;
using System.Collections;
using System.Threading;

namespace ChrisKapffer {
    /// <summary>
    /// This class makes it easy to launch new threads. They can optionally be equipped with a callback method
    /// which gets invoked on the main thread once the custom thread has completed.
    /// It also allows you to dispatch an action delegate from another thread which then gets invoked on the main thread.
    /// </summary>
	public static class ThreadHelper {
        private static object _dispatcherAccessor = new object();

        /// <summary>
        /// A monobehaviour to execute callbacks on the main thread, which originated elsewhere
        /// </summary>
        private static CallbackDispatcher _dispatcher = null;
        private static CallbackDispatcher Dispatcher {
            get {
                lock(_dispatcherAccessor) {
                    if (_dispatcher == null) {
                        _dispatcher = new GameObject(typeof(CallbackDispatcher).Name).AddComponent<CallbackDispatcher>();
                    }
                    return _dispatcher;
                }
            }
        }

        /// <summary>
        /// Initializes the ThreadHelper. This has to be done on the main thread. Call this method if you want to use
        /// ThreadHelper.DispatchOnMain from within another thread.
        /// </summary>
        public static void Init() {
            _dispatcher = Dispatcher;
        }

        /// <summary>
        /// Determines if ThreadHelper initialized.
        /// </summary>
        /// <returns><c>true</c> if it is initialized; otherwise, <c>false</c>.</returns>
        public static bool IsInitialized() {
            return _dispatcher != null;
        }

        /// <summary>
        /// Launches a new thread which executes the provided task and optionally invokes a callback method after it has completed.
        /// </summary>
        /// <param name="task">The task to perform on another thread.</param>
        /// <param name="callback">(optional) code to execute after the operation has finished.</param>
		public static void DispatchAsync(Action task, Action callback = null) {
            // game objects can not be created outside of the main thread,
            // so ensure that our CallbackDispatcher is created before entering our custom thread
            if (!IsInitialized()) { Init(); }
			ThreadPool.QueueUserWorkItem(new WaitCallback( delegate(object t) {
				task.Invoke();
				if (callback != null) {
                    // enqueue for execution on the main thread
                    Dispatcher.Enqueue(callback);
				}
			}));
		}
		
        /// <summary>
        /// Launches a new thread which executes the provided task and optionally invokes a callback method after it has completed.
        /// The task can return a value through the callback delegate.
        /// </summary>
        /// <param name="task">The task to perform on another thread.</param>
        /// <param name="callback">(optional) code to execute after the operation has finished.</param>
        /// <typeparam name="T">The return type of the specified task.</typeparam>
		public static void DispatchAsync<T>(Func<T> task, Action<T> callback = null) {
            // game objects can not be created outside of the main thread,
            // so ensure that our CallbackDispatcher is created before entering our custom thread
            if (!IsInitialized()) { Init(); }
			ThreadPool.QueueUserWorkItem(new WaitCallback( delegate(object t) {
                // we have to ditch the type here to be able to enqueue the callback
                // but it gets unboxed later when invoking the callback
				object result = task.Invoke();
				if (callback != null) {
                    // enqueue the wrapped callback for execution on the main thread
                    Dispatcher.Enqueue(new Action<object>(obj => callback((T)obj)), result);
				}
			}));
		}

		public static void DispatchOnMain(Action task) {
            if (!IsInitialized()) {
                // game objects can not be created outside of the main thread and at this point it is already to late
                Debug.LogError("ThreadHelper was not initialized! You have to call ThreadHelper.Init before using ThreadHelper.DispatchOnMain.");
            } else {
                Dispatcher.Enqueue(task);
            }
		}

        /// <summary>
        /// This class ensures that all callbacks will execute on the main thread.
        /// </summary>
		private class CallbackDispatcher : MonoBehaviour {
            /// <summary>
            /// The callbacks accumulating from other threads. The Queue has to be synchronized in order to be thread safe.
            /// Unfortunately that means that we can't use generics any more.
            /// </summary>
			private Queue callbacks = Queue.Synchronized(new Queue());
			
            /// <summary>
            /// Enqueues an action delegate to be invoked on the main thread within the next update call.
            /// </summary>
            /// <param name="action">Action delegate without a parameter</param>
			public void Enqueue(Action action) {
				callbacks.Enqueue(new ActionWithOrWithoutParameter(action));
			}
		
            /// <summary>
            /// Enqueues an action delegate to be invoked on the main thread within the next update call.
            /// </summary>
            /// <param name="action">Action delegate with a single parameter</param>
            /// <param name="parameter">the parameter value</param>
			public void Enqueue(Action<object> action, object parameter) {
				callbacks.Enqueue(new ActionWithOrWithoutParameter(action, parameter));
			}
			
            /// <summary>
            /// Because Update runs on the main thread, this is a save place to invoke any callbacks which got enqueued from other threads.
            /// </summary>
			private void Update() {
				while(callbacks.Count > 0) {
					((ActionWithOrWithoutParameter)callbacks.Dequeue()).Invoke();
				}
			}
		}

        /// <summary>
        /// This class is a wrapper to provide a common interface for actions that either have one or zero parameters
        /// </summary>
		private class ActionWithOrWithoutParameter {
			Action<object> actionWithParameter;
			Action actionWithoutParameter;
			object parameter;

			public ActionWithOrWithoutParameter(Action action) {
				this.actionWithoutParameter = action;
			}

			public ActionWithOrWithoutParameter(Action<object> action, object parameter) {
				this.actionWithParameter = action;
				this.parameter = parameter;
			}

			public void Invoke() {
				if (actionWithoutParameter != null) {
					actionWithoutParameter.Invoke();
				} else if (actionWithParameter != null) {
					actionWithParameter.Invoke(parameter);
				}
			}
		}
	}
}
