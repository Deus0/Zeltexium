using UnityEngine;

namespace NobleMuffins.TurboSlicer.Guts
{
	public static class Helpers
	{
		public static Vector4 PlaneFromPointAndNormal(Vector3 point, Vector3 normal)
		{
			Vector4 plane = (Vector4) normal.normalized;
			plane.w = -(normal.x * point.x + normal.y * point.y + normal.z * point.z);
			return plane;
		}
	}
}

