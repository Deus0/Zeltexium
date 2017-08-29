using UnityEngine;
using System.Collections;

public class SplatterOnSlice : AbstractSliceHandler
{
	public Object particlePrefab;
	
	void Start()
	{
		if(particlePrefab == null)
		{
			Debug.LogWarning("SplatterOnSlice script needs to be connected with a particle effect prefab! Try the included 'splatter prefab' or a custom variant of your own.");
		}
	}
	
	public override void handleSlice( GameObject[] results )
	{
		if(particlePrefab != null)
		{
			Vector3 position = results[0].transform.position;
			
			GameObject.Instantiate(particlePrefab, position, Quaternion.identity);
		}
	}
	
}
