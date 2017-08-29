using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities 
{
	// used to animate the lines on item inspect gui
	public class AnimateLineOnGui : AnimateLine 
	{
		RectTransform MyRect;

		// Use this for initialization
		void Start ()
		{
			MyRect = transform.GetComponent<RectTransform> ();	// the rect will be used for positioning the lines!
			CreateLines ();
		}

		// Update is called once per frame
		void Update () 
		{
			if (Target == null)
				return;
			AnimateLines ();
		}

		private void CreateLines()
		{
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
			MyLines.Add (CreateNewLine ());
		}

		private void AnimateLines()
		{
			Vector3[] MyCorners = new Vector3[4];
			MyRect.GetWorldCorners (MyCorners);
			MyTargetPositions.Clear ();
			MyFromPositions.Clear ();
			for (int i = 0; i < MyLines.Count; i++)
            {
				MyFromPositions.Add (MyCorners [i]);
				MyTargetPositions.Add (Target.transform.position);
			}
			UpdateLinePositionsTotal ();
		}
	}
}