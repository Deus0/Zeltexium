using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Zeltex.AnimationUtilities
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RenderPoints : MonoBehaviour
    {
        public bool IsSingleColor;
        public Color32 SingleColor;
        public List<Vector3> MyPoints = new List<Vector3>();
        public List<Color32> MyColors = new List<Color32>();

        protected void CreateMesh()
        {
            List<int> Indicies = new List<int>();
            for (int i = 0; i < MyPoints.Count; i++)
            {
                Indicies.Add(i);
            }
            if (IsSingleColor)
            {
                MyColors.Clear();
                for (int i = 0; i < MyPoints.Count; i++)
                {
                    MyColors.Add(SingleColor);
                }
            }

            Mesh MyMesh = new Mesh();
            MyMesh.vertices = MyPoints.ToArray();
            MyMesh.colors32 = MyColors.ToArray();
            MyMesh.SetIndices(Indicies.ToArray(), MeshTopology.Points, 0);
            GetComponent<MeshFilter>().mesh = MyMesh;
        }
        public void UpdatePoints(Vector3[] NewPoints)
        {
            MyPoints.Clear();
            for (int i = 0; i < NewPoints.Length; i++)
            {
                AddPoint(NewPoints[i], new Vector3(0.02f, 0.02f, 0.02f));
                //MyPoints.Add (NewPoints[i]);
            }
            CreateMesh();
        }
        public void AddPoint(Vector3 NewPoint, Vector3 Size)
        {
            float MyScale = 0.01f;
            for (float i = -Size.x; i <= Size.x; i += MyScale)
                for (float j = -Size.y; j <= Size.y; j += MyScale)
                    for (float k = -Size.z; k <= Size.z; k += MyScale)
                    {
                        MyPoints.Add(NewPoint + new Vector3(i, j, k));
                    }
        }
        public void Clear()
        {
            MyPoints.Clear();
            MyColors.Clear();
        }
    }
}