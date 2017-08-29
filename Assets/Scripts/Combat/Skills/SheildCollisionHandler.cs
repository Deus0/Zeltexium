using UnityEngine;
using System.Collections;
namespace Zeltex.Combat 
{
	public class SheildCollisionHandler : MonoBehaviour 
	{
		public Sheild MySheild;
		// animating options
		[Header("Animation")]
		private Vector3 OriginalScale;
		private float AnimationSpeed = 4;
		private float Variation = 0.025f;

		void Start()
		{
			OriginalScale = transform.localScale;
		}
		void Update()
		{
			// every on sec decrease mana by 1 for activated sheild
			transform.localScale = OriginalScale + OriginalScale * (Variation * Mathf.Sin (Time.time * AnimationSpeed));
		}

		void OnTriggerEnter(Collider other) 
		{
			MySheild.UseTheForce (other.gameObject);
		}

		void OnTriggerStay(Collider other)
		{ 
			MySheild.UseTheForce (other.gameObject);
		}
	}
}
