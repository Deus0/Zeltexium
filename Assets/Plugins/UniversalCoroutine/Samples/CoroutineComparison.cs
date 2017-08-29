using System.Collections;
using UnityEngine;
using UniversalCoroutine;

/// <summary>Small test class to show the difference between using Unity coroutines and Universal coroutines</summary>
public class CoroutineComparison: MonoBehaviour
{
	private void Start()
	{
		//StartTestUnityCoroutine();
		StartTestUniversalCoroutine();
	}

	private void StartTestUnityCoroutine()
	{
		StartCoroutine(TestUnityCoroutine());
	}

	private void StartTestUniversalCoroutine()
	{
		this.UniStartCoroutine(TestUniversalCoroutine());
	}

	#region UNITY
	private IEnumerator TestUnityCoroutine()
	{
		Debug.Log("Unity coroutine: wait 3 seconds");
		yield return new UnityEngine.WaitForSeconds(3);
		Debug.Log("Unity coroutine: waited 3 seconds");
		yield return StartCoroutine(TestUnitySubroutine());
		Debug.Log("Unity coroutine: done");
	}

	private IEnumerator TestUnitySubroutine()
	{
		Debug.Log("Unity subroutine: started");
		yield return new WaitForFixedUpdate();
		Debug.Log("Unity subroutine: in FixedUpdate() now");
		yield return null;
		Debug.Log("Unity subroutine: ended");
	}
	#endregion

	#region UNIVERSAL_COROUTINE
	private IEnumerator TestUniversalCoroutine()
	{
		Debug.Log("Universal coroutine: wait 3 seconds");
		yield return this.UniWaitForSeconds(3); //yield return CoroutineManager.WaitForSeconds(3)
		Debug.Log("Universal coroutine: waited 3 seconds");
		yield return this.UniStartCoroutine(TestUniversalSubroutine());
		Debug.Log("Universal coroutine: done");
	}

	private IEnumerator TestUniversalSubroutine()
	{
		Debug.Log("Universal subroutine: started");
		yield return this.UniWaitForFixedUpdate(); //yield return CoroutineManager.WaitForFixedUpdate()
		Debug.Log("Universal subroutine: in FixedUpdate() now");
		yield return null;
		Debug.Log("Universal subroutine: ended");
	}
	#endregion
}
