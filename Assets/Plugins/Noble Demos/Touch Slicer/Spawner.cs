using UnityEngine;
using System.Collections;

namespace NobleMuffins.TurboSlicer.Examples.TouchSlicer
{
	public class Spawner : MonoBehaviour
	{
		public float minimumTimeToNextSpawn = 0.33f;
		public float maximumTimeToNextSpawn = 0.5f;
		public Object[] prefabs;
	
		public float targetHeight = 5f;
		public float lateralVariance = 1f;
	
		public float torque = 100f;
	
		private float countdown;

		// Use this for initialization
		void Start ()
		{
			ResetClock ();
		}

		void ResetClock ()
		{
			countdown = Mathf.Lerp (minimumTimeToNextSpawn, maximumTimeToNextSpawn, Random.value);
		}

		private static float speedToReachDesiredHeight (float h)
		{
			float g = -Physics.gravity.y;
			float i = Mathf.Sqrt ((h * g) / (1f - g / (2 * g)));
			return i;
		}
	
		// Update is called once per frame
		void Update ()
		{
			countdown -= Time.deltaTime;
		
			if (countdown < 0f) {
				Object selectedPrefab = prefabs [Random.Range (0, prefabs.Length)];
			
				Vector3 position = transform.position + new Vector3 (Random.value * 2f * lateralVariance - lateralVariance, 0, 0);
			
				GameObject go = GameObject.Instantiate (selectedPrefab, position, Quaternion.identity) as GameObject;
			
				Rigidbody rb = go.GetComponent<Rigidbody> ();
			
				if (rb != null) {
					rb.velocity = new Vector3 (0f, speedToReachDesiredHeight (targetHeight), 0f);
				
					rb.AddTorque (Random.insideUnitSphere * torque);
				}
			
				ResetClock ();
			}
		}
	}
}
