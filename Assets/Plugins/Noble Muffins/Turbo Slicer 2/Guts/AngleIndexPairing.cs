using System;

namespace NobleMuffins.TurboSlicer.Guts
{
	public struct AngleIndexPairing: IComparable<AngleIndexPairing>
	{
		public AngleIndexPairing (int index, float angle)
		{
			this.index = index;
			this.angle = angle;
		}

		public int index;
		public float angle;

		#region IComparable implementation

		public int CompareTo (AngleIndexPairing other)
		{
			return angle.CompareTo (other.angle);
		}

		#endregion
	}
}

