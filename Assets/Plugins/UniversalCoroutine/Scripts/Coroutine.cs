//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalCoroutine
{
	/// <summary>Routine class for a Coroutine</summary>
	public class Coroutine: ManagedIEnumerator
	{
		/// <summary>Stack of all routines/IEnumerators that run on this Coroutine instance</summary>
		private Stack<IEnumerator> routineStack;
		/// <summary>The current routine to be executed</summary>
		private IEnumerator currentRoutine;
		/// <summary>The subroutine if any (type casted version of current routine)</summary>
		private Coroutine subroutine;
		/// <summary>The WWW instance if any (will be executed before subroutine and currentRoutine)</summary>
		private WWW www;

		/// <summary>Indicates if this coroutine is a subroutine</summary>
		public bool IsSubroutine { get; set; }

		public override object Current
		{
			get
			{
				if(currentRoutine != null)
				{
					return currentRoutine.Current;
				}
				return null;
			}
		}

		public Coroutine()
		{
			routineStack = new Stack<IEnumerator>();
		}

		/// <summary>Resets this Coroutine and uses given routine</summary>
		public void Reset(IEnumerator routine)
		{
			routineStack.Push(routine);
			currentRoutine = routine;
			CurrentState = RoutineState.ACTIVE;
		}

		public override void Stop()
		{
			base.Stop();
			routineStack.Clear();
			currentRoutine = null;
			subroutine = null;
			www = null;
		}

		public override bool MoveNext()
		{
			bool wwwEnded = false;
			bool routineEnded = false;

			if(www != null)
			{
				wwwEnded = www.isDone;
			}
			else if(subroutine != null)
			{
				routineEnded = !subroutine.MoveNext();
				if(routineEnded)
				{
					subroutine = null;
				}
			}
			else if (currentRoutine != null)
			{
				routineEnded = !currentRoutine.MoveNext();
			}

			if(wwwEnded)
			{
				www = null;
				return MoveNext();
			}
			else if(routineEnded)
			{
				if(routineStack.Count > 0)
				{
					currentRoutine = routineStack.Pop();
					CheckSubroutine();
					return MoveNext();
				}
				else
				{
					routineStack.Clear();
					Stop(); //Ready to be reused
				}
			}
			else if (currentRoutine != null)
			{
				object current = currentRoutine.Current;
				if(current != null)
				{
					if(current is IEnumerator)
					{
						IEnumerator routine = (IEnumerator)current;
						routineStack.Push(currentRoutine); //Save for later
						currentRoutine = routine;
						CheckSubroutine();
					}
					else if(current is WWW)
					{
						www = (WWW)current;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>Checks if current routine is a subroutine and marks it as a subroutine if true</summary>
		public void CheckSubroutine()
		{
			subroutine = currentRoutine as Coroutine;
			if(subroutine != null)
			{
				subroutine.IsSubroutine = true;
			}
		}

		public override string ToString()
		{
			return "Coroutine";
		}
	}
}
