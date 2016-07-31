using System.Collections;
using ChrisKapffer;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorToggler : MonoBehaviour {

	public Color color;

	/// <summary>
	/// Example usage of input conditions, have a look in the inspector
	/// </summary>
	public InputCondition setNewColorCondition;
	public InputCondition setOriginalColorCondition;

	private Renderer _renderer;
	private Renderer Renderer {
		get {
			if (_renderer == null) {
				_renderer = GetComponent<Renderer>();
			}
			return _renderer;
		}
	}

	private Color originalColor;
	private bool usesOriginalColor;

	// Use this for initialization
	void Start () {
		originalColor = Renderer.material.color;
	}

	// Update is called once per frame
	void Update () {
		if (setNewColorCondition.IsMet()) {
			SetNewColor();
		}
		if (setOriginalColorCondition.IsMet()) {
			SetOriginalColor();
		}
	}

	public void SetNewColor() {
		Renderer.material.color = color;
		usesOriginalColor = false;
	}

	public void SetOriginalColor() {
		Renderer.material.color = originalColor;
		usesOriginalColor = true;
	}

	public void ToggleColor() {
		if (usesOriginalColor) {
			SetNewColor();
		} else {
			SetOriginalColor();
		}
	}
}
