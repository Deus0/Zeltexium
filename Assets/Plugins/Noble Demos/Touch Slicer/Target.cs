using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NobleMuffins.TurboSlicer.Examples.TouchSlicer
{
	public class Target : MonoBehaviour
	{
		private static readonly List<Target> _targets = new List<Target> ();

		public static IList<Target> targets { get { return _targets.AsReadOnly (); } }

		[HideInInspector] public new Transform transform;
		[HideInInspector] public new Renderer renderer;

		void Awake ()
		{
			transform = GetComponent<Transform> ();
			renderer = GetComponent<Renderer> ();
		}

		void OnEnable ()
		{
			_targets.Add (this);
		}

		void OnDisable ()
		{
			_targets.Remove (this);
		}
	}
}
