using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Guis.Maker;

namespace Zeltex.Sound
{
	//[System.Serializable]

	/// <summary>
	/// Renders a curve! Used for sound editor!
	/// </summary>
    [ExecuteInEditMode]
	public class CurveRenderer : MonoBehaviour
	{
		#region Variables
        [Header("Debug")]
		public Zound MyZound = new Zound();
		public AnimationCurve MyCurve;  // used for outputting sound!
		public int RenderSampleCount = 256;
		public bool IsScaleUp = false;
		//public int VertCount = 20;
		// Render Data
		public List<Vector3> MyCurvePoints = new List<Vector3>();
		public float LineWidth = 0.01f;
        public float GridWidth = 0.01f;

        public float PositionZ = -5;
		// public float MultiplierX = 1;
		private RectTransform MyRect;
        [Header("WaveLines")]
        [SerializeField]
        private Material LineMaterial;
        [SerializeField]
        private Color LineBeginColor = Color.yellow;
        [SerializeField]
        private Color LineEndColor = Color.red;
        [SerializeField]
        private LineRenderer MyWaveLine;
        // Grid
        [SerializeField]
        private LineRenderer GridLineX;
        [SerializeField]
        private LineRenderer GridLineY;
        [SerializeField]
        private Color GridBeginColor = Color.yellow;
        [SerializeField]
        private Color GridEndColor = Color.red;
        [SerializeField, HideInInspector]
        private GameObject MyGridX;
        [SerializeField, HideInInspector]
        private GameObject MyGridY;
        [Header("References")]
		public SoundMaker MySoundMaker;
		public GameObject Piano;
		//public int samplerate = 44100;
		//public float Frequency = 440;
		[Header("Testing")]
		public bool IsTest = false;
		public int TestFrequency = 500;
		public float TestTime = 3;
		public int TestSamples = 1024;
		public float TestCurveFreqency = 5;
		int StartIndex = 0;
        [Header("More Tests")]
        public bool IsRefreshGrid;
        public bool IsRefreshWave;
        #endregion

        #region Mono

        void Start()
		{
			MyRect = GetComponent<RectTransform>();
			InitiateLineRenderer();
			InitiateGrid();
		}

		void Update()
		{
			//TransformCurve();
            if (IsRefreshGrid)
            {
                IsRefreshGrid = false;
                InitiateGrid();
            }
            if (IsRefreshWave)
            {
                IsRefreshWave = false;
                if (MyWaveLine)
                {
                    DestroyImmediate(MyWaveLine);
                }
                MyZound.ReplaceWithCurve(MyCurve);
                //InitiateLineRenderer();
                OnUpdateCurve();
            }

			if (IsTest)
			{
				if (Input.GetKey(KeyCode.DownArrow))
				{
					StartIndex++;
					StartIndex = Mathf.Clamp(StartIndex, 0, MyZound.GetSize() - 41);
				}
				if (Input.GetKey(KeyCode.UpArrow))
				{
					StartIndex--;
					StartIndex = Mathf.Clamp(StartIndex, 0, MyZound.GetSize() - 41);
				}
				if (Input.GetKeyDown(KeyCode.T))
				{
					TestCurve();
				}
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				MySoundMaker.GetComponent<AudioSource>().Play();
			}
		}


		public void TestCurve()
		{
			MyZound.SetSamples(new float[TestSamples]);
			MyZound.Frequency = TestFrequency;
			MyZound.TimeLength = TestTime;
			for (int i = 0; i < TestSamples; i++)
			{
				MyZound.SetSample(i, Mathf.Sin((float)i * 2f * Mathf.PI * TestCurveFreqency));
			}
			UpdateSound();
		}


		public void OnGUI()
		{
			if (IsTest)
			{
				GUI.contentColor = new Color(55, 100, 200);  // Apply Red color to Button
				for (int i = StartIndex; i < StartIndex + 40; i++)
				{
					GUILayout.Label(i + " [" + MyZound.GetSample(i).ToString() + "]");
				}
			}
		}
		#endregion

		#region UI
		public void UseInput(Slider MySlider)
		{
			MyZound.Frequency = (int)MySlider.value;
		}
		#endregion

		#region CurveRendering

		/// <summary>
		/// Initiate line for our curve!
		/// </summary>
		private void InitiateLineRenderer()
		{
            if (MyWaveLine == null)
            {
                MyWaveLine = GetComponent<LineRenderer>();
            }
			if (MyWaveLine == null)
			{
				MyWaveLine = gameObject.AddComponent<LineRenderer>();
				MyWaveLine.material = LineMaterial;
                MyWaveLine.material.renderQueue = 6000;
                MyWaveLine.material.renderQueue = 6000;

                MyWaveLine.startColor = LineBeginColor;
				MyWaveLine.endColor = LineEndColor;
				MyWaveLine.startWidth = LineWidth;
				MyWaveLine.endWidth = LineWidth;
			}
		}

		/// <summary>
		/// Updates the curve points
		/// Creates a new AnimationCurve(MyCurve) Data
		/// Then updates the LineRenderer - Vertex Count - and Points List(MyCurvePoints)
		/// </summary>
		public void OnUpdateCurve()
		{
			MyCurvePoints.Clear();
			// The curve is just the lesser version!
			MyCurve = new AnimationCurve();
			float[] ReducedCurveSamples = MyZound.GetReducedData(RenderSampleCount);    //RenderSampleCount
			for (int i = 0; i < RenderSampleCount; i++)
			{
                //MyZound.TimeLength
                float MyTime = ((float)(i) / (float)(RenderSampleCount)) * MyZound.TimeLength;
                MyCurve.AddKey(new Keyframe(MyTime, ReducedCurveSamples[i]));
			}

			float HighestPoint = 0.001f;
			if (IsScaleUp)
			{
				for (int i = 0; i < MyCurve.keys.Length; i++)
				{
					if (Mathf.Abs(MyCurve.keys[i].value) > HighestPoint)
					{
						HighestPoint = Mathf.Abs(MyCurve.keys[i].value);
					}
				}
			}
			else
			{
				HighestPoint = 1;   // default 1 or 2
			}
			float TimeEnds = MyCurve.keys[MyCurve.keys.Length - 1].time;
			//Debug.Log("Updating Curve Renderer - Time: " + TimeEnds + 
			//    ": keys: " + MyCurve.keys.Length
			//     + ": highestPoint: " + HighestPoint);
			for (int i = 0; i < MyCurve.keys.Length; i++)
			{
				Vector3 MyPosition = new Vector3(
						(-0.5f + ((MyCurve.keys[i].time) / TimeEnds)),    // / 2
						(MyCurve.keys[i].value / HighestPoint) / 2, // MyCurve.Evaluate(
					PositionZ);
				MyCurvePoints.Add(MyPosition);
            }
            InitiateLineRenderer();
            if (MyWaveLine)
			{
                Debug.Log("Updating wave line with: " + MyCurvePoints.Count + " positions.");
                MyWaveLine.positionCount = MyCurvePoints.Count;
                MyWaveLine.SetPositions(MyCurvePoints.ToArray());
                MyWaveLine.startWidth = LineWidth;
				MyWaveLine.endWidth = LineWidth;
				ShowCorneredGrid();
			}
			else
			{
				Debug.LogError("No MyWaveLine In curverenderer");
			}
            TransformCurve();
        }

		/// <summary>
		/// Transforms the curve in relation to the transform its attached to (our 3d gui!)
		/// </summary>
		public void TransformCurve()
		{
			List<Vector3> TransformedCurve = new List<Vector3>();
			for (int i = 0; i < MyCurvePoints.Count; i++)
			{
				Vector3 MyPosition = MyCurvePoints[i];
				MyPosition.x *= MyRect.GetWidth();  // 0 to 1! 
				MyPosition.y *= MyRect.GetHeight();
				MyPosition = transform.TransformPoint(MyPosition);
				TransformedCurve.Add(MyPosition);
			}
			//MyWaveLine.positionCount = TransformedCurve.Count;
			MyWaveLine.SetPositions(TransformedCurve.ToArray());//(i, MyPosition);
			ShowCorneredGrid();
		}
        #endregion

        #region Grid

        /// <summary>
        /// Creates the grid lines
        /// </summary>
        private void InitiateGrid()
		{
            if (MyGridX)
            {
                MyGridX.Die();
            }
            if (MyGridY)
            {
                MyGridY.Die();
            }
            // Grid
            MyGridX = new GameObject();
            MyGridX.name = "GridAxisX";
            MyGridX.transform.SetParent(transform);
			MyGridY = new GameObject();
            MyGridY.name = "GridAxisY";
            MyGridY.transform.SetParent(transform);
			GridLineX = MyGridX.AddComponent<LineRenderer>();
			GridLineY = MyGridY.AddComponent<LineRenderer>();
			GridLineX.material = LineMaterial;
			GridLineY.material = LineMaterial;
            GridLineX.material.renderQueue = 6000;
            GridLineY.material.renderQueue = 6000;

            GridLineX.startWidth = GridWidth;
			GridLineX.endWidth = GridWidth;
			GridLineY.startWidth = GridWidth;
			GridLineY.endWidth = GridWidth;

            GridLineX.startColor = GridBeginColor;
            GridLineX.endColor = GridEndColor;
            GridLineY.startColor = GridBeginColor;
            GridLineY.endColor = GridEndColor;
            GridLineX.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
			GridLineY.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
			OnUpdateCurve();
		}

		/// <summary>
		/// Our Grid Points!
		/// </summary>
		void ShowCentredGrid()
		{
			if (GridLineX && GridLineY)
			{
				// Set Grid Points
				Vector3 PositionX = transform.TransformPoint(new Vector3(-0.5f * MyRect.GetWidth(), 0, PositionZ));
				GridLineX.SetPosition(0, PositionX);
				PositionX = transform.TransformPoint(new Vector3(0.5f * MyRect.GetWidth(), 0, PositionZ));
				GridLineX.SetPosition(1, PositionX);
				Vector3 PosY = transform.TransformPoint(new Vector3(0, -0.5f * MyRect.GetHeight(), PositionZ));
				GridLineY.SetPosition(0, PosY);
				PosY = transform.TransformPoint(new Vector3(0, 0.5f * MyRect.GetHeight(), PositionZ));
				GridLineY.SetPosition(1, PosY);
			}
		}
		void ShowCorneredGrid()
		{
			if (GridLineX && GridLineY)
			{
				// Set Grid Points
				Vector3 PositionX = transform.TransformPoint(new Vector3(-0.5f * MyRect.GetWidth(), 0, PositionZ));
				GridLineX.SetPosition(0, PositionX);
				PositionX = transform.TransformPoint(new Vector3(0.5f * MyRect.GetWidth(), 0, PositionZ));
				GridLineX.SetPosition(1, PositionX);
				Vector3 PosY = transform.TransformPoint(new Vector3(-0.5f * MyRect.GetWidth(), -0.5f * MyRect.GetHeight(), PositionZ));
				GridLineY.SetPosition(0, PosY);
				PosY = transform.TransformPoint(new Vector3(-0.5f * MyRect.GetWidth(), 0.5f * MyRect.GetHeight(), PositionZ));
				GridLineY.SetPosition(1, PosY);
			}
		}
#endregion

#region AudioClip

		/// <summary>
		/// Used internally by sound maker
		/// </summary>
		public void UpdateCurve(AudioClip MyAudioClip, bool IsFromSoundMaker)
		{
			if (MyAudioClip != null)
			{
				MyZound.UseAudioClip(MyAudioClip);
				if (IsFromSoundMaker == false)
				{
					MySoundMaker.SetSelected(MyAudioClip);  // if called from generator or something
				}
				// Update piano
				for (int i = 0; i < Piano.transform.childCount; i++)
				{
					Piano.transform.GetChild(i).GetComponent<AudioSource>().clip = MyAudioClip;
				}
				OnUpdateCurve();
			}
			else
			{
				Debug.LogError("Audio Clip is null inside curve renderer");
			}
		}

		/// <summary>
		/// Change our audio clip into the curve data!
		/// </summary>
		public void UpdateCurve(AudioClip MyAudioClip)
		{
		}

		/// <summary>
		/// Generate a audio clip from our wave
		/// </summary>
		public void UpdateSound()
		{
			StartCoroutine(WaitForClip(MyZound.GenerateAudioClip()));
		}

		/// <summary>
		/// Waits until clip is loaded, then updates the curve renderer
		/// </summary>
		/// <param name="MyAudioClip"></param>
		/// <returns></returns>
		public IEnumerator WaitForClip(AudioClip MyAudioClip)
		{
			while (MyAudioClip.loadState != AudioDataLoadState.Loaded)
			{
				yield return null;
			}
			UpdateCurve(MyAudioClip, false);
		}

		public void SetTime(float NewTime)
		{
			MyZound.SetTime(NewTime);
			OnUpdateCurve();
		}
#endregion
	}
}
/*void MyAudioFilter(float[] data)
{
    Debug.Log("MyAudioFilter: " + data.Length + " - " + position);
    for (int i = 0; i < data.Length; i++)
    {
        //float WaveInput = Frequency * ((float)position) / (float)(samplerate);
        data[i] = MyZound.MySamples[position + i];//MyCurve.Evaluate(MyCurve.keys[i].value);    //WaveInput  //Amplitude * 
    }
    position += data.Length;
}
void OnAudioSetPosition(int newPosition)
{
    position = newPosition;
}*/
