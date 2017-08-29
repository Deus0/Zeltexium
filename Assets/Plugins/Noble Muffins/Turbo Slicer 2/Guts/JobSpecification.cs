using UnityEngine;
using System.Collections.Generic;

namespace NobleMuffins.TurboSlicer.Guts
{
	public class JobSpecification
	{
		public JobSpecification(GameObject subject, IEnumerable<MeshSnapshot> data, Vector4 planeInLocalSpace, bool channelTangents, bool channelNormals, bool channelUV2, int shatterStep, bool destroyOriginal) {
			Subject = subject;
			Data = data;
			PlaneInLocalSpace = planeInLocalSpace;
			ChannelTangents = channelTangents;
			ChannelUV2 = channelUV2;
			ChannelNormals = channelNormals;
			DestroyOriginal = destroyOriginal;
			ShatterStep = shatterStep;
		}

		public GameObject Subject { get; private set; }

		public bool ChannelTangents { get; private set; }

		public bool ChannelUV2 { get; private set; }

		public bool ChannelNormals { get; private set; }

		public bool DestroyOriginal { get; private set; }

		public IEnumerable<MeshSnapshot> Data { get; private set; }

		public Vector4 PlaneInLocalSpace { get; private set; }

		public int ShatterStep { get; private set; }
	}
}