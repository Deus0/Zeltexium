using System;
using UnityEngine;

namespace NobleMuffins.TurboSlicer.Guts
{
	public static class MeshReducer
	{
		public static MeshSnapshot Strip(this MeshSnapshot source)
		{
			const int Unassigned = -1;
			
			var vertexCount = source.vertices.Length;
			var submeshCount = source.indices.Length;

			var transferTable = new int[vertexCount];

			for(int i = 0; i < transferTable.Length; i++) {
				transferTable[i] = Unassigned;
			}

			var targetIndex = 0;

			var targetIndexArrays = new int[submeshCount][];

			for(int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++) {
				var sourceIndices = source.indices[submeshIndex];
				var targetIndices = targetIndexArrays[submeshIndex] = new int[sourceIndices.Length];

				for(int i = 0; i < sourceIndices.Length; i++)
				{
					int requestedVertex = sourceIndices[i];

					int j = transferTable[requestedVertex];

					if(j == Unassigned)
					{
						j = targetIndex;
						transferTable[requestedVertex] = j;
						targetIndex++;
					}

					targetIndices[i] = j;
				}
			}

			var newVertexCount = targetIndex;

			Vector3[] targetVertices, targetNormals;
			Vector2[] targetCoords, targetCoords2;
			Vector4[] targetTangents;

			targetVertices = new Vector3[newVertexCount];

			for(int i = 0; i < transferTable.Length; i++)
			{
				int j = transferTable[i];
				if(j != Unassigned)
					targetVertices[j] = source.vertices[i];
			}

			targetCoords = new Vector2[newVertexCount];

			for(int i = 0; i < transferTable.Length; i++)
			{
				int j = transferTable[i];
				if(j != Unassigned)
					targetCoords[j] = source.coords[i];
			}

			if(source.tangents.Length > 0)
			{
				targetTangents = new Vector4[newVertexCount];
				for(int i = 0; i < transferTable.Length; i++)
				{
					int j = transferTable[i];
					if(j != Unassigned)
						targetTangents[j] = source.tangents[i];
				}
			}
			else {
				targetTangents = source.tangents;
			}

			if(source.normals.Length > 0)
			{
				targetNormals = new Vector3[newVertexCount];
				for(int i = 0; i < transferTable.Length; i++)
				{
					int j = transferTable[i];
					if(j != Unassigned)
						targetNormals[j] = source.normals[i];
				}
			}
			else {
				targetNormals = source.normals;
			}

			if(source.coords2.Length > 0)
			{
				targetCoords2 = new Vector2[newVertexCount];

				for(int i = 0; i < transferTable.Length; i++)
				{
					int j = transferTable[i];
					if(j != Unassigned)
						targetCoords2[j] = source.coords2[i];
				}
			}
			else {
				targetCoords2 = source.coords2;
			}

			return new MeshSnapshot(source.key, targetVertices, targetNormals, targetCoords, targetCoords2, targetTangents, targetIndexArrays, source.infillBySubmesh, source.rootToLocalTransformation);
		}
	}
}

