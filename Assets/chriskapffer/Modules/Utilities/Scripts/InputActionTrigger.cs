using UnityEngine;
using UnityEngine.Events;

namespace ChrisKapffer {

	/// <summary>
	/// This simply bundles an input codition and a unity event together and gives the whole thing a nice name.
	/// </summary>
	[System.Serializable]
	public class ConditionActionPair {
		public string name;
		public InputCondition condition;
		public UnityEvent action;
	}

	/// <summary>
	/// This is a hub where you can define several input conditions and actions which will be performed if their corresponding condition is met.
	/// </summary>
	public class InputActionTrigger : MonoBehaviour {

		public ConditionActionPair[] pairs;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			// check all conditions and invoke the corresponding action if the condition is met.
			foreach (var pair in pairs) {
				if (pair.condition.IsMet()) {
					if (pair.action != null) {
						pair.action.Invoke();
					}
				}
			}
		}
	}
}
