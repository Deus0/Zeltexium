using System;
using UnityEngine;
using System.Collections.Generic;
using NobleMuffins.TurboSlicer;
using NobleMuffins.TurboSlicer.Guts;

[DisallowMultipleComponent]
public class Sliceable : MonoBehaviour, ISliceable
{
	public bool currentlySliceable = true;
	public bool refreshColliders = true;
	public InfillConfiguration[] infillers = new InfillConfiguration[0];
	public bool channelNormals = true;
	public bool channelTangents = false;
	public bool channelUV2 = false;
	public bool shreddable = true;
	public UnityEngine.Object alternatePrefab = null;
	public bool alwaysCloneFromAlternate = false;

	public event EventHandler<SliceEventArgs> Sliced;

	public void RaiseSliced(params GameObject[] parts) {
		if(Sliced != null) {
			Sliced(this, new SliceEventArgs(parts));
		}
	}
		
	public void Slice(Vector3 positionInWorldSpace, Vector3 normalInWorldSpace)
	{
		if(currentlySliceable)
		{
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			
			Vector3 position = worldToLocalMatrix.MultiplyPoint3x4(positionInWorldSpace);
			Vector3 normal = worldToLocalMatrix.MultiplyVector(normalInWorldSpace).normalized;
			
			Vector4 planeInLocalSpace = Helpers.PlaneFromPointAndNormal(position, normal);
			
			TurboSlicerSingleton.Instance.Slice(gameObject, planeInLocalSpace, true);
		}
	}
	
	public void handleSlice( GameObject[] results )
	{
		AbstractSliceHandler[] handlers = gameObject.GetComponents<AbstractSliceHandler>();
		
		foreach(AbstractSliceHandler handler in handlers)
		{
			handler.handleSlice(results);
		}
	}
	
	public bool cloneAlternate( Dictionary<string,bool> hierarchyPresence )
	{
		if(alternatePrefab == null)
		{
			return false;
		}
		else if(alwaysCloneFromAlternate)
		{
			return true;
		}
		else
		{
			AbstractSliceHandler[] handlers = gameObject.GetComponents<AbstractSliceHandler>();
			
			foreach(AbstractSliceHandler handler in handlers)
			{
				if(handler.cloneAlternate( hierarchyPresence ))
				{
					return true;
				}
			}
		
			return false;
		}
	}
}
