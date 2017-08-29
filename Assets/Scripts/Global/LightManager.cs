using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
	/// <summary>
	/// Manages cameras
	/// connects with billboard scripts
	/// </summary>
	public class LightManager : ManagerBase<LightManager>
	{
		[SerializeField]
		private bool IsSpawnOnStart;
		[SerializeField]
		private GameObject LightPrefab;
		private Light MainLight;

		private void Start()
		{
			if (IsSpawnOnStart)
			{
				if (LightPrefab)
				{
					MainLight = Instantiate(LightPrefab).GetComponent<Light>();
					MainLight.transform.SetParent(transform);
				}
				else
				{
					Debug.LogError("Set light prefab.");
				}
			}
		}
	}
}

