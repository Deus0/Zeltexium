//-----------------------------------------
//			Universal Coroutine
// Copyright (c) 2016 Jeroen van Pienbroek
//------------------------------------------
namespace UniversalCoroutine
{
	/// <summary>Enum for UpdateTypes</summary>
	public enum UpdateType { FIXED_UPDATE, LATE_UPDATE }

	/// <summary>Routine class to wait for the configured UpdateType</summary>
	public class WaitForUpdate: ManagedIEnumerator
	{
		/// <summary>The UpdateType of this WaitForUpdate instance</summary>
		private UpdateType updateType;

		/// <summary>Constructor of WaitForUpdate class that configures given UpdateType</summary>
		public WaitForUpdate(UpdateType updateType)
		{
			this.updateType = updateType;
		}

		/// <summary>Resets this WaitForUpdate instance</summary>
		public new void Reset()
		{
			CurrentState = RoutineState.ACTIVE;
		}

		public override bool MoveNext()
		{
			return IsActive;
		}

		public override string ToString()
		{
			return "WaitForUpdate: " + updateType;
		}
	}
}