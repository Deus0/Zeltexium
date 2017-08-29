//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using UnityEngine;

namespace UniversalCoroutine
{
	/// <summary>Routine class to wait for the configured amount of seconds</summary>
	public class WaitForSeconds: ManagedIEnumerator
	{
		/// <summary>The total seconds to wait</summary>
		private float seconds;
		/// <summary>The current seconds counter</summary>
		private float currentSeconds;
		/// <summary>The last time of an Update</summary>
		private float lastTime;

		/// <summary>Resets this WaitForSeconds instance and uses given seconds</summary>
		public void Reset(float seconds)
		{
			this.seconds = seconds;
			currentSeconds = 0;
			lastTime = Time.realtimeSinceStartup;
			CurrentState = RoutineState.ACTIVE;
		}

		public override bool MoveNext()
		{
			currentSeconds += (Time.realtimeSinceStartup - lastTime);
			if(currentSeconds >= seconds)
			{
				Stop(); //Ready to be reused
				return false;
			}
			lastTime = Time.realtimeSinceStartup;
			return true;
		}

		public override void Resume()
		{
			base.Resume();
			lastTime = Time.realtimeSinceStartup;
		}

		public override string ToString()
		{
			return "WaitForSeconds: " + seconds;
		}
	}
}