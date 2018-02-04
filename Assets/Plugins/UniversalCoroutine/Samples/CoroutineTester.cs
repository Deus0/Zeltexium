//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using System.Collections;
using UnityEngine;
using UniversalCoroutine;

/// <summary>Class to test coroutine functionality</summary>
public class CoroutineTester: MonoBehaviour
{
	/// <summary>String format for blue text in logs</summary>
	private string BLUE_TEXT_FORMAT = "<color=blue>{0}</color>";
	/// <summary>String format for green text in logs</summary>
	private string GREEN_TEXT_FORMAT = "<color=green>{0}</color>";

	#region UNITY
	private void Start()
	{
		CoroutineManager.StartCoroutine(Test());
	}
	#endregion

	/// <summary>Starts the main test coroutine</summary>
	public void StartCoroutines()
	{
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Starting test coroutine");
		this.UniStartCoroutine(Test());
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Started test coroutine");
	}

	/// <summary>Stops all active coroutines</summary>
	public void StopCoroutines()
	{
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Stopping all coroutines");
		this.UniStopAllCoroutines();
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Stopped all coroutines");
	}

	/// <summary>Pauses all active coroutines</summary>
	public void PauseCoroutines()
	{
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Pausing all coroutines");
		this.UniPauseAllCoroutines();
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Paused all coroutines");
	}

	/// <summary>Resumes all paused coroutines</summary>
	public void ResumeCoroutines()
	{
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Resuming all coroutines");
		this.UniResumeAllCoroutines();
		Debug.LogFormat(GREEN_TEXT_FORMAT, "Resumed all coroutines");
	}

	/// <summary>Main test coroutine method</summary>
	private IEnumerator Test()
	{
		yield return null;
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting for 5 seconds");
		yield return this.UniWaitForSeconds(5);
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting done");
		yield return this.UniStartCoroutine(WaitForUpdateTest());
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting for 4 seconds");
		yield return this.UniWaitForSeconds(4);
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting done");
		yield return this.UniStartCoroutine(BreakTest());
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting for 3 seconds");
		yield return this.UniWaitForSeconds(3);
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Waiting done");
		yield return this.UniStartCoroutine(WWWTest());
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Finished");
	}

	/// <summary>Coroutine method to test the different WaitForUpdate options</summary>
	private IEnumerator WaitForUpdateTest()
	{
		yield return null;
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Normal Update");
		yield return this.UniWaitForFixedUpdate();
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Fixed Update");
		yield return this.UniWaitForLateUpdate();
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Late Update");
	}

	/// <summary>Coroutine method to test the yield break option</summary>
	private IEnumerator BreakTest()
	{
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Start BreakTest()");
		yield return null;
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Gonna break");
		yield break;
		//Debug.LogFormat(BLUE_TEXT_FORMAT, "End BreakTest()"); //This shouldn't be called, because of yield break
	}

	/// <summary>Coroutine method to test the WWW option</summary>
	private IEnumerator WWWTest()
	{
		Debug.LogFormat(BLUE_TEXT_FORMAT, "Start WWWTest()");
		using(WWW www = new WWW("www.google.com"))
		{
			yield return www;
			Debug.LogFormat(BLUE_TEXT_FORMAT, "WWW Result: " + www.text);
		}
		Debug.LogFormat(BLUE_TEXT_FORMAT, "End WWWTest()");
	}
}