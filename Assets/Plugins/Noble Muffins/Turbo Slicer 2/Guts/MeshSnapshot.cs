using UnityEngine;
using System.Collections.Generic;

namespace NobleMuffins.TurboSlicer.Guts
{
	public class MeshSnapshot
	{
		public const string RootKey = "845937cc-485f-4d0a-8b5f-15b479e257a8";
		
		public MeshSnapshot (object key,
			Vector3[] vertices, Vector3[] normals,
			Vector2[] coords, Vector2[] coords2,
			Vector4[] tangents, int[][] indices, Rect[] infillBySubmesh,
			Matrix4x4 rootToLocalTransformation)
		{
			this.key = key;
			this.vertices = vertices;
			this.normals = normals;
			this.coords = coords;
			this.coords2 = coords2;
			this.tangents = tangents;
			this.indices = indices;
			this.infillBySubmesh = infillBySubmesh;
			this.rootToLocalTransformation = rootToLocalTransformation;
		}

		public readonly object key;

		public readonly Matrix4x4 rootToLocalTransformation;

		public readonly Vector3[] vertices;
		public readonly Vector3[] normals;
		public readonly Vector2[] coords;
		public readonly Vector2[] coords2;
		public readonly Vector4[] tangents;

		public readonly int[][] indices;
		public readonly Rect[] infillBySubmesh;

		public MeshSnapshot WithVertices(Vector3[] figure) {
			return new MeshSnapshot(key, figure, normals, coords, coords2, tangents, indices, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithNormals(Vector3[] figure) {
			return new MeshSnapshot(key, vertices, figure, coords, coords2, tangents, indices, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithCoords(Vector2[] figure) {
			return new MeshSnapshot(key, vertices, normals, figure, coords2, tangents, indices, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithCoords2(Vector2[] figure) {
			return new MeshSnapshot(key, vertices, normals, coords, figure, tangents, indices, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithTangents(Vector4[] figure) {
			return new MeshSnapshot(key, vertices, normals, coords, coords2, figure, indices, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithIndices(int[][] figure) {
			return new MeshSnapshot(key, vertices, normals, coords, coords2, tangents, figure, infillBySubmesh, rootToLocalTransformation);
		}

		public MeshSnapshot WithInfillBySubmesh(Rect[] figure) {
			return new MeshSnapshot(key, vertices, normals, coords, coords2, tangents, indices, figure, rootToLocalTransformation);
		}
	}
}