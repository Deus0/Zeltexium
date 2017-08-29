using UnityEngine;
using System.Collections.Generic;
using NobleMuffins.TurboSlicer;

[RequireComponent(typeof(Sliceable))]
public abstract class AbstractSliceHandler : MonoBehaviour
{
	private Sliceable sliceable;

	void Awake() {
		sliceable = gameObject.GetComponent<Sliceable>();
	}
	
	void OnEnable() {
		sliceable.Sliced += Sliceable_Sliced;
	}

	void OnDisable() {
		sliceable.Sliced += Sliceable_Sliced;
	}

	void Sliceable_Sliced (object sender, SliceEventArgs e)
	{
		handleSlice(e.Parts);
	}
	
	public virtual void handleSlice( GameObject[] results )
	{
		//Do nothing
	}
	
	public virtual bool cloneAlternate ( Dictionary<string,bool> hierarchyPresence )
	{
		return true;
	}
}
