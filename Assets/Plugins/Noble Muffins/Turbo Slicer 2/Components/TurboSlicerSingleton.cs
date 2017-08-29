using System;
using UnityEngine;

namespace NobleMuffins.TurboSlicer
{
	[RequireComponent(typeof(TurboSlicer))]
	public class TurboSlicerSingleton: MonoBehaviour
	{
		private static TurboSlicer instance;
		public static TurboSlicer Instance {
			get {
				if(instance == null) {
					var go = new GameObject("Turbo Slicer Singleton", typeof(TurboSlicer), typeof(TurboSlicerSingleton));
					instance = go.GetComponent<TurboSlicer>();
				}
				return instance;
			}
		}

		void Awake() {
			if(instance == null) {
				instance = gameObject.GetComponent<TurboSlicer>();
				GameObject.DontDestroyOnLoad(this);
			}
			else {
				GameObject.Destroy(gameObject);
			}
		}
	}
}

