using UnityEngine;

namespace NobleMuffins.TurboSlicer.Examples
{
	[RequireComponent (typeof(Sliceable))]
	public class SoundWhenSliced : MonoBehaviour
	{
		public AudioClip clip;
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
			if (clip != null) {
				var go = new GameObject ();
			
				go.transform.position = transform.position;

				var source = go.AddComponent<AudioSource> ();

				source.clip = clip;
				source.Play ();
			
				GameObject.Destroy (go, clip.length);
			}
		}
	}
}

