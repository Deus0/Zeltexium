using UnityEngine;

namespace NobleMuffins.TurboSlicer.Examples
{
	[RequireComponent (typeof(Sliceable))]
	public class BurstApartWhenSliced : MonoBehaviour
	{		
		public float burstForce = 100f;
		private Sliceable sliceable;

		void Awake() {
			sliceable = gameObject.GetComponent<Sliceable>();
		}

		protected void OnEnable() {
			sliceable.Sliced += Sliceable_Sliced;
		}

		protected void OnDisable() {
			sliceable.Sliced -= Sliceable_Sliced;
		}

		void Sliceable_Sliced (object sender, NobleMuffins.TurboSlicer.SliceEventArgs e)
		{
			var centers = new Vector3[e.Parts.Length];
		
			for (int i = 0; i < e.Parts.Length; i++) {
				var colliders = e.Parts[i].GetComponentsInChildren<Collider>();
				for(int j = 0; j < colliders.Length; j++) {
					centers[i] += colliders[j].bounds.center;
				}
			}
		
			var center = Vector3.zero;
			for (int i = 0; i < centers.Length; i++)
				center += centers [i];
			center /= (float)centers.Length;
		
			for (int i = 0; i < e.Parts.Length; i++) {
				var go = e.Parts [i];
				var rb = go.GetComponent<Rigidbody> ();
				if (rb != null) {
					var v = centers [i] - center;
					v.Normalize ();
					v *= burstForce;
					rb.AddForce (v);
				}
			}
		}
	}
}

