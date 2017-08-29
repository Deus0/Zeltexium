using System;
using UnityEngine;

namespace NobleMuffins.TurboSlicer.Examples.Oranges
{
	public class LineDrawnEventArgs : EventArgs
	{
		public LineDrawnEventArgs(Vector2 start, Vector2 end): base() {
			Start = start;
			End = end;
		}

		public Vector2 Start { get; private set; }
		public Vector2 End { get; private set; }
	}
}