using System.Collections;
using System.Collections.Generic;
using ChrisKapffer;
using UnityEngine;

/// <summary>
/// This class' purpose is to demonstrate some custom editor attributes
/// </summary>
public class EditorAttributesDemo : MonoBehaviour {

	/// <summary>
	/// An example on how to use OnChangeAttribute
	/// If _cubeCount changes we invoke the setter
	/// </summary>
    [SerializeField, OnChange("CubeCount")]
	private int _cubeCount = 4;
    public int CubeCount {
        get {
            return _cubeCount;
        }
        set {
			value = Mathf.Clamp(value, 0, 10000);
            if (_cubeCount != value) {
                _cubeCount = value;
                UpdateCubes();
            }
        }
    }
		
	/// <summary>
	/// This is again a property setter that gets called.
	/// Unfortunately its not possible to use multiple property drawers simultaneously
	/// Otherwhise you'd be able to skip the setter and use [OnChange("UpdateCubes"), Range(1, 100)] instead.
	/// </summary>
	[OnChange("UpdateCubes")]
	public int _cubesPerLine = 4;
	public int CubesPerLine {
		get {
			return _cubesPerLine;
		}
		set {
			value = Mathf.Clamp(value, 1, 100);
			if (_cubesPerLine != value) {
				_cubesPerLine = value;
				UpdateCubes();
			}
		}
	}

	/// <summary>
	/// OnChangeAttribute can also invoke multiple targets. They can be methods and property setters
	/// which themselves can be public, protected or private. But be aware that this is only used in the
	/// editor. If you'd deploy this code and set cubeSpacing from another class nothing will ever happen!
	/// </summary>
	[OnChange(new string[]{"UpdateCubes", "LogToConsole"})]
	public float cubeSpacing = 1.5f;

	/// <summary>
	/// Demonstration of the DisabledAttribute. Think of it as readonly inspector values.
	/// </summary>
	[SerializeField, Disabled]
	private int initialCubeCount = 4;

	/// <summary>
	/// This is a more useful example for a DisabledAttribute. You can see the instance id in the
	/// inspector after entering playmode. But its not possible to change it.
	/// </summary>
	[SerializeField, Disabled]
	private int instanceId;

	/// <summary>
	/// This is an example of the ButtonAttribute. The inspector will show a button instead of a variable
	/// You can specify the method to execute, the button caption and its width.
	/// </summary>
    [Button("OnClickResetButton", "Reset")]
    public bool editorButtonDummy;

	// Use this for initialization
	void Start () {
		instanceId = GetInstanceID();
		// this is only here to get rid of the non used warning of instanceId
		Debug.Log(string.Format("Instance id of {0}: {1}", GetType().Name, instanceId));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
    private void UpdateCubes() {
		int count = CubeCount;
		var cubes = new List<Transform>();
		foreach (Transform child in transform) {
			cubes.Add(child);
		}

		if (cubes.Count != count) {
			if (cubes.Count > count) {
				// remove unwanted cubes
				for (int i = cubes.Count - 1; i >= count; i--) {
					DestroyImmediate(cubes[i].gameObject);
					cubes.RemoveAt(i);
				}
			} else {
				// create additional cubes
				for (int i = cubes.Count; i < count; i++) {
					var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.name = "Cube: " + i;
					cube.transform.parent = transform;
					cubes.Add(cube.transform);
				}
			}
		}

		count = cubes.Count;
		// position them nicely
		for (int i = 0; i < count; i++) {
			cubes[i].localPosition = new Vector3(
				cubeSpacing * (i % CubesPerLine - (Mathf.Min(count, CubesPerLine) - 1) / 2f),
				cubeSpacing * (i / CubesPerLine - (count-1) / CubesPerLine / 2f),
				0);
		}
    }

	private void LogToConsole(float value) {
		Debug.Log(string.Format("Cube spacing is now at {0} units", value));
	}

    private void OnClickResetButton() {
		CubeCount = initialCubeCount;
		Debug.Log("Cube count got reset to its initial value of " + initialCubeCount);
    }
}
