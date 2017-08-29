using UnityEngine;
using NobleMuffins.TurboSlicer;

namespace NobleMuffins.TurboSlicer.Examples.Oranges
{
	[RequireComponent (typeof(Sliceable))]
	public class ShatterOnClick : MonoBehaviour
	{
		public int shatterSteps = 3;

		void OnMouseUpAsButton ()
		{
			TurboSlicerSingleton.Instance.Shatter (gameObject, shatterSteps);
		}
	}
}