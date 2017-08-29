//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using System;
using System.Collections;

namespace UniversalCoroutine
{
	/// <summary>Enum for RoutineStates</summary>
	public enum RoutineState { IDLE, ACTIVE, PAUSED }

	/// <summary>Subclass of IEnumerator that adds RoutineStates</summary>
	public abstract class ManagedIEnumerator: IEnumerator
	{
		/// <summary>The current RoutineState/summary>
		public RoutineState CurrentState { get; protected set; }
		/// <summary>The current object returned from the routine</summary>
		public virtual object Current { get { return null; } }
		/// <summary>Indicates if this routine is active</summary>
		public bool IsActive { get { return (CurrentState == RoutineState.ACTIVE); } }
		/// <summary>Indicates if this routine is idle</summary>
		public bool IsIdle { get { return (CurrentState == RoutineState.IDLE); } }

		/// <summary>Updates this routine. Returns false if done</summary>
		public abstract bool MoveNext();

		/// <summary>Stops this routine</summary>
		public virtual void Stop()
		{
			CurrentState = RoutineState.IDLE;
		}

		/// <summary>Pauses this routine if active</summary>
		public virtual void Pause()
		{
			if(CurrentState == RoutineState.ACTIVE)
			{
				CurrentState = RoutineState.PAUSED;
			}
		}

		/// <summary>Resume this routine if paused</summary>
		public virtual void Resume()
		{
			if(CurrentState == RoutineState.PAUSED)
			{
				CurrentState = RoutineState.ACTIVE;
			}
		}

		/// <summary>Default reset method for IEnumerator (not used)</summary>
		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
