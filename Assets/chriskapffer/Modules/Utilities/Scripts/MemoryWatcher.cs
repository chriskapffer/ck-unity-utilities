using UnityEngine;

namespace ChrisKapffer {

	/// <summary>
	/// This script prints the amount of allocated memory to the screen.
	/// Note, that it doesn't display the memory consumption of the whole application, but rather what mono uses, i.e. your game code. 
	/// </summary>
	public class MemoryWatcher : MonoBehaviour {
		public enum Unit {
			Kilobyte = 1,
			Megabyte = 2,
			Gigabyte = 3,
		}
		public enum Conversion {
			Decimal = 1000,
			Binary = 1024
		}

		/// <summary>
		/// Unit to display the used memory in (Kilo- Mega or Gigabyte)
		/// </summary>
		public Unit unit = Unit.Megabyte;

		public Conversion conversion = Conversion.Binary;

		/// <summary>
		/// Update interval
		/// </summary>
		public float timestep = 2.0f;

		private float nextUpdate = 0.0f;

		void Update() {
			if (Time.time > nextUpdate) {
				double memory = Convert(System.GC.GetTotalMemory(false), (float)conversion, (int)unit);
				Singleton.Get<ScreenLog>().Print("Memory", string.Format("{0:0.000} {1}", memory, unit.ToString()));
				nextUpdate = Time.time + timestep;
			}
		}
			
		private static double Convert(double value, float denominator, int ntimes) {
			return value / Mathf.Pow(denominator, ntimes);
		}
	}
}
