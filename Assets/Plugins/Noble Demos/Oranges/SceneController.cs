using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace NobleMuffins.TurboSlicer.Examples.Oranges
{
	public class SceneController : MonoBehaviour
	{
		public new Camera camera;
		
		public LineController lineController;

		void OnEnable() {
			lineController.LineDrawn += LineController_LineDrawn;
		}

		void OnDisable() {
			lineController.LineDrawn -= LineController_LineDrawn;
		}

		void LineController_LineDrawn (object sender, LineDrawnEventArgs e)
		{
			var sliceables = GameObject.FindObjectsOfType<Sliceable>();
			for(int i = 0; i < sliceables.Length; i++) {
				var sliceable = sliceables[i];
				try {
					TurboSlicerSingleton.Instance.SliceByLine(sliceable.gameObject, camera, e.Start, e.End, true);
				} catch(Exception ex) {
					Debug.LogException(ex, this);
				}
			}
		}

		public void OnTapReset() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}
}
