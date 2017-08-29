using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

// also stop recording when the movement of the physics object has stopped
// and don't record that movement at all if it isn't different

// I need a debug line that shows positions when recording

namespace Zeltex.AnimationUtilities {
	public class ReverseMovement : MonoBehaviour 
	{
		[Tooltip("When reverseing has happened")]
		public UnityEvent OnEndReverse = new UnityEvent();

		public bool IsDebugMovement = false;

		static float MaxTimeRecording = 10f;
		public float BaseAutoReverse = 25;
		private float AutoReverseTime = 25;
		public float VariationAuto = 15;
		public float RecordRate = 0.1f;
		public bool IsPlayOnce = true;
		private float LastReversed;
		private List<Vector3> Positions = new List<Vector3>();
		private List<Quaternion> Rotations = new List<Quaternion>();
		private List<float> Times = new List<float>();
		private bool IsRecording = false;
		private float LastRecordedTime;
		public int PositionIndex = 0;
		private float TimeBegun = 0;
		public float ReverseTimeDilation = 0.666f;	// slows or speeds up the reverse animation
		private float TimeFinishedReverse = 0f;	// when did reverse finish
		public float TimePausedOnReverse = 12f;

		// Use this for initialization
		void Start () {
			BeginRecording();
		}
		void Clear() {
			Positions.Clear ();
			Rotations.Clear ();
			Times.Clear ();
		}

		// Update is called once per frame
		void Update () {
			if (Input.GetKeyDown(KeyCode.T)) {
				Flip();
			}
			if (!IsPlayOnce)
			if ((PositionIndex == -1 && Time.time-TimeFinishedReverse >= TimePausedOnReverse )|| IsRecording) {
				if (Time.time - LastReversed >= AutoReverseTime) {
					LastReversed = Time.time;
					Flip ();
					if (IsRecording) {
						// MutateReverse
					}
				}
			}
			if (IsRecording) {
				if (Time.time - LastRecordedTime >= RecordRate) {
					RecordPosition ();
				}
				if (IsDebugMovement)
				for (int i = 0; i < Positions.Count-1; i++) {
					Debug.DrawLine (Positions [i], Positions [i + 1], Color.blue);
				}
			} else {
				if (IsDebugMovement)
				for (int i = 0; i < Positions.Count-1; i++) {
					Debug.DrawLine (Positions [i], Positions [i + 1], Color.red);
				}
			}
		}
		void FixedUpdate() {
			AnimateReverse ();
		}

		public void SetAutoReverseTime(float NewAutoReverseTime) {
			AutoReverseTime = NewAutoReverseTime;
		}
		public float MutateReverseTime() {
			AutoReverseTime = BaseAutoReverse+Random.Range(-VariationAuto,VariationAuto);
			return AutoReverseTime;
		}
		private void BeginRecording() {
			IsRecording = true;
			Positions.Clear();
			Times.Clear();
			Rotations.Clear ();
			TimeBegun = Time.time;
			RecordPosition();
			if (gameObject.GetComponent<Rigidbody> ()) {
				gameObject.GetComponent<Rigidbody> ().isKinematic = false;
			}
		}
		// reverses the movement
		public void Reverse() {
			if (IsRecording) {
				RecordPosition ();	// record one last time
				IsRecording = false;
				PositionIndex = Positions.Count - 1;
				TimeBegun = Time.time;
				if (gameObject.GetComponent<Rigidbody> ()) {
					gameObject.GetComponent<Rigidbody> ().isKinematic = true;
				}
			}
		}
		private void SetMovement(bool NewRecordState) {
			if (NewRecordState != IsRecording) {
				if (!IsRecording) {
					BeginRecording();
				} 
				else 
				{
					Reverse();
				}
				IsRecording = NewRecordState;
			}
		}
		public void Flip() {
			if (IsRecording)
				Reverse ();
			else
				BeginRecording ();
		}
		private void AnimateReverse() {
			if (!IsRecording && PositionIndex != -1)
			if (PositionIndex >= 0 && PositionIndex < Times.Count)
			{
				float TimeSince = Time.time-TimeBegun;
				float TimeBetweenPositions = (Times[PositionIndex]-Times[PositionIndex-1])/ReverseTimeDilation;
				float DeltaTime = TimeSince/TimeBetweenPositions;
				transform.position = Vector3.Lerp(Positions[PositionIndex], Positions[PositionIndex-1], DeltaTime);	//DeltaTime);	// reverse positions for jittery look
				transform.rotation = Quaternion.Lerp(Rotations[PositionIndex], Rotations[PositionIndex-1], DeltaTime);

				if (TimeSince >= TimeBetweenPositions)
				{
					PositionIndex --;
					if (PositionIndex <= 0)  
					{
						//Debug.LogError("Ending Reverse Movement: " + Time.time);
						PositionIndex = -1;
						TimeFinishedReverse = Time.time;
						Clear();
						if (OnEndReverse != null) 
						{
							OnEndReverse.Invoke();
						}
					} else {
						TimeBegun = Time.time;
					}
				}
			}
		}
		// removes the recordings at the start if they are past a certain time
		public void RemoveRecordsAtTime() 
		{
			float TimeSince = Time.time - TimeBegun;
			if (TimeSince < MaxTimeRecording)
				return;
			// else
			TimeBegun = TimeSince-MaxTimeRecording;
			for (int i = Times.Count-1; i >= 0; i--) {
				//for (float i = 0; i < 1/RecordRate; i++) {
				//Time.time-TimeBegun >= MaxTimeRecording
				if (Times[i] >= MaxTimeRecording) 
				{
					int MyIndex = Mathf.FloorToInt (i);
					Times.RemoveAt (MyIndex);
					Positions.RemoveAt (MyIndex);
					Rotations.RemoveAt (MyIndex);
				}
			}
		}
		private void RecordPosition() {
			//RemoveRecordsAtTime();
			if (Positions.Count == 0 || Positions [Positions.Count - 1] != transform.position) {
				LastRecordedTime = Time.time;
				Times.Add (Time.time - TimeBegun);
				Positions.Add (transform.position);
				Rotations.Add (transform.rotation);
			}
		}
	}
}
