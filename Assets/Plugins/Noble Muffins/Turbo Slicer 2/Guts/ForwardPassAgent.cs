using UnityEngine;
using System.Collections.Generic;

namespace NobleMuffins.TurboSlicer.Guts {
	public class ForwardPassAgent : MonoBehaviour {
		public IEnumerable<MeshSnapshot> Snapshots { get; set; }
	}
}