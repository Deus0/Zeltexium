using UnityEngine;
using System.Collections.Generic;
using NobleMuffins.TurboSlicer;

namespace NobleMuffins.TurboSlicer.Examples.TouchSlicer
{
	public class TargetSlicer : MonoBehaviour
	{
		public new Camera camera;
	
		public float minimumSpeedForSlicing = 1f;
		public int sampleCount = 3;
		public float cooldownTime = 0.1f;
	
		private const int mouseIndex = -1;
		private readonly Dictionary<int,Vector2> priorPositionByIndex = new Dictionary<int, Vector2> ();
		private readonly Dictionary<int,float> cooldownClockByTouchIndex = new Dictionary<int, float> ();
	
		// Update is called once per frame
		void Update ()
		{	
			bool touchesAreDown = Input.touchCount > 0;
			bool mouseIsDown = (Input.GetMouseButton (0) || Input.GetButton ("Fire1")) && !touchesAreDown;
		
			List<int> observedIndices = new List<int> (8);
		
			IList<Target> targets = null;
			Vector3[] targetCirclesByIndex = null;
		
			if (mouseIsDown || touchesAreDown) {
				targets = Target.targets;
		
				targetCirclesByIndex = DeriveCirclesFromTargets (targets);
			}
		
			if (touchesAreDown) {
				foreach (Touch t in Input.touches) {
					if (t.phase == TouchPhase.Moved) {
						int index = t.fingerId;
					
						ProcessInput (index, t.position, targets, targetCirclesByIndex);
					
						observedIndices.Add (index);
					}
				}
			}
	
			if (mouseIsDown) {
				ProcessInput (mouseIndex, Input.mousePosition, targets, targetCirclesByIndex);
			
				observedIndices.Add (mouseIndex);
			}
		
			List<int> removalQueue = new List<int> ();
		
			foreach (int key in priorPositionByIndex.Keys) {
				bool continuesToBeObserved = observedIndices.Contains (key);
			
				if (!continuesToBeObserved) {
					removalQueue.Add (key);
				}
			}
		
			foreach (int key in removalQueue) {
				priorPositionByIndex.Remove (key);
			}
		
			List<int> cooldownClockKeys = new List<int> (cooldownClockByTouchIndex.Keys);
		
			for (int i = 0; i < cooldownClockKeys.Count; i++) {
				int key = cooldownClockKeys [i];
				float f = cooldownClockByTouchIndex [key];
				f -= Time.deltaTime;
				if (f < 0f)
					cooldownClockByTouchIndex.Remove (key);
				else
					cooldownClockByTouchIndex [key] = f;
			}
		}

		private void ProcessInput (int key, Vector2 currentPosition, IList<Target> targets, Vector3[] targetCirclesByIndex)
		{
			if (priorPositionByIndex.ContainsKey (key) && !cooldownClockByTouchIndex.ContainsKey (key)) {
				Vector2 priorPosition = priorPositionByIndex [key];
			
				float pixelsPerSecond = (currentPosition - priorPosition).magnitude / Time.deltaTime;
			
				bool maySlash = pixelsPerSecond > minimumSpeedForSlicing;
			
				List<GameObject> objectsToSlice = new List<GameObject> ();
			
				if (maySlash) {	
					for (int i = 0; i < targetCirclesByIndex.Length; i++) {
						Vector3 circle = targetCirclesByIndex [i];
					
						float deltaSquared = (currentPosition.x - circle.x) * (currentPosition.x - circle.x) +
						                    (currentPosition.y - circle.y) * (currentPosition.y - circle.y);
					
						bool fallsWithin = deltaSquared < (circle.z * circle.z);
					
						if (fallsWithin)
							objectsToSlice.Add (targets [i].gameObject);
					}
				}
			
				foreach (GameObject go in objectsToSlice) {
					//REMINDER: The current and prior positions here convey where on the screen the touch
					//is and where it came from, rather than the position of the object itself.
				
					TurboSlicerSingleton.Instance.SliceByLine (go, camera, currentPosition, priorPosition, true);
				}
			
				if (objectsToSlice.Count > 0) {
					cooldownClockByTouchIndex [key] = cooldownTime;
				}
			}

			priorPositionByIndex [key] = currentPosition;
		}

		private Vector3[] DeriveCirclesFromTargets (IList<Target> allTargets)
		{
			Vector3 cameraPosition = camera.transform.position;
		
			Vector3[] circles = new Vector3[ allTargets.Count ];
		
			for (int i = 0; i < allTargets.Count; i++) {
				Vector3 positionInThreeSpace = allTargets [i].transform.position;
			
				Quaternion q = Quaternion.LookRotation (positionInThreeSpace - cameraPosition);
				Matrix4x4 m = Matrix4x4.TRS (Vector3.zero, q, Vector3.one);

				Vector3 _size = allTargets [i].GetComponent<Collider> ().bounds.size;
				float size = Mathf.Max (_size.x, _size.y, _size.z) / 2f;
			
				Vector3 upFrom = m.MultiplyVector (Vector3.up) * size + positionInThreeSpace;
			
				Vector3 positionInTwoSpace = camera.WorldToScreenPoint (positionInThreeSpace);
				Vector3 upFromPositionInTwoSpace = camera.WorldToScreenPoint (upFrom);
			
				float radius = (positionInTwoSpace - upFromPositionInTwoSpace).magnitude;
			
				Vector3 circle = new Vector3 (positionInTwoSpace.x, positionInTwoSpace.y, radius);
			
				circles [i] = circle;
			}
		
			return circles;
		}
	}
}
