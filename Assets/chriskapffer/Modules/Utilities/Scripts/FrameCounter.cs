using UnityEngine;
using System.Collections;

namespace ChrisKapffer {
	
	/// <summary>
	/// This script prints the current framerate to the screen.
	/// Just attach it to any object in your scene and you're got to go.
	/// If you want to hide it call Singleton.Get<ScreenLog>().Visible = false;
	/// </summary>
	public class FrameCounter : MonoBehaviour {
		private int frameCount = 0;
		private float nextUpdate = 0.0f;

		void Update() {
			frameCount++;
			if (Time.time > nextUpdate) {
				Singleton.Get<ScreenLog>().Print("FPS", frameCount);
				nextUpdate = Time.time + 1.0f;
				frameCount = 0;
			}
		}
	}
}
