using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*	Holds a path - bunch of points
 *  
*/

namespace Zeltex.AI
{
    /// <summary>
    /// Makes the bot follow a path of movement.
    /// </summary>
	public class Pather : MonoBehaviour
    {
		public List<GameObject> PathPoints;
		public int PathIndex = 0;
		private Bot MyMovement;

		// Use this for initialization
		void Awake ()
        {
			MyMovement.OnReachTarget.AddEvent(OnReachTarget);	
			if (PathPoints.Count > 0)
            {
				MyMovement.MoveToPosition (PathPoints [PathIndex].transform.position);
			}
		}

		public void OnReachTarget()
        {
			if (PathPoints.Count > 0)
            {
				Debug.Log(PathIndex + " - Has reached new position: " + Time.time);
				PathIndex++;
				if (PathIndex >= PathPoints.Count) 
					PathIndex = 0;
				MyMovement.MoveToPosition (PathPoints [PathIndex].transform.position);
			}
		}
	}
}
