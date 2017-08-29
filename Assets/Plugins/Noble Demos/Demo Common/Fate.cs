using UnityEngine;

namespace NobleMuffins.TurboSlicer.Examples
{
	public class Fate : MonoBehaviour
	{
		public float lifetime = 3f;

		void Start ()
		{
			GameObject.Destroy (gameObject, lifetime);
		}
	}
}

