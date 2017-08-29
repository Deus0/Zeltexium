using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Credit: Mathias Wagner Nielsen
using System;

namespace NobleMuffins.TurboSlicer.Examples.Oranges
{
	[RequireComponent (typeof(LineRenderer), typeof(PseudoGraphic))]
	public class LineController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		private LineRenderer lineRenderer;

		public float threshold = 10.0f;
		public float z = 10f;

		public event EventHandler<LineDrawnEventArgs> LineDrawn;

		void IPointerDownHandler.OnPointerDown (PointerEventData eventData)
		{
			this.lineRenderer.enabled = true;
			this.lineRenderer.SetPosition (0, GetMouseWorldPos (eventData.pressPosition));
			this.StartCoroutine ("UpdateLineRenderer");
		}

		void IPointerUpHandler.OnPointerUp (PointerEventData eventData)
		{
			var delta = (eventData.pressPosition - eventData.position).magnitude;
			
			if(LineDrawn != null && delta > threshold) {
				LineDrawn(this, new LineDrawnEventArgs(eventData.pressPosition, eventData.position));
			}
			
			this.lineRenderer.enabled = false;
			this.StopCoroutine ("UpdateLineRenderer");
		}

		private void Awake ()
		{
			this.lineRenderer = this.GetComponent<LineRenderer> ();
		}

		private Vector3 GetMouseWorldPos (Vector3 screenPos)
		{
			screenPos.z = this.z;
			return Camera.main.ScreenToWorldPoint (screenPos);
		}

		private IEnumerator UpdateLineRenderer ()
		{
			while (true) {
				this.lineRenderer.SetPosition (1, this.GetMouseWorldPos (Input.mousePosition));
				yield return null;
			}
		}
	}
}