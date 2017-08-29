using System;
using UnityEngine;

namespace NobleMuffins.TurboSlicer
{
	public class SliceEventArgs: EventArgs
	{		
		public SliceEventArgs (GameObject[] parts): base()
		{
			Parts = parts;
		}

		public GameObject[] Parts { get; private set; }
	}
}

