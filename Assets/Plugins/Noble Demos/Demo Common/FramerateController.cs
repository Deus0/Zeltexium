using UnityEngine;

namespace NobleMuffins.TurboSlicer.Examples
{
	public class FramerateController : MonoBehaviour
	{
		public int targetFramerate = 60;

		// Use this for initialization
		void Start ()
		{
			Application.targetFrameRate = targetFramerate;	
		}
	}
}

