using UnityEngine;

//If you want to modify this file, CLONE it, and modify the CLONE!
//If you modify it in its current place, you may accidentally overwrite
//it during an update.
namespace NobleMuffins.TurboSlicer.Examples
{
	//This is a component which needs to be attached to a sliceable game object. It requires
	//the presence of a Sliceable component on the same object.
	[RequireComponent (typeof(Sliceable))]
	public class ExampleSliceHandler : MonoBehaviour
	{
		private Sliceable sliceable;

		void Awake() {
			//We assume this will work because, with the RequireComponent attribute, we asked the Unity editor
			//to ensure the presence of a Sliceable component on the same object.
			sliceable = gameObject.GetComponent<Sliceable>();
		}

		protected void OnEnable() {
			//This code "subscribes" to the Sliced event. In C# this is called a "multicast delegate".
			sliceable.Sliced += Sliceable_Sliced;
		}

		protected void OnDisable() {
			//We also want to unsubscribe when we're disabled, so that we don't receive the callback when
			//we're not supposed to.
			sliceable.Sliced -= Sliceable_Sliced;
		}

		void Sliceable_Sliced (object sender, NobleMuffins.TurboSlicer.SliceEventArgs e)
		{
			//When a slice occurs, this method will be called. This for loop will iterate through each part of the slice result.
			//As of Turbo Slicer 2.0, there should be exactly two slice results but we won't hard-code that because this might
			//change in the future.
			for(int i = 0; i < e.Parts.Length; i++) {
				var go = e.Parts[i];
				Debug.LogFormat(this, "Object '{0}' is the result of a slice!", go.name);
			}
		}
	}
}
