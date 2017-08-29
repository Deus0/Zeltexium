using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZeltexTools;

namespace Zeltex.Generators
{
	[System.Serializable]
	public class TileAnimationState
    {
		public float MyTime = 1f;
		public int TileIndex = 0;
	}

	public class CubeTextureMap : TileMapGenerator 
	{
		public int[] MyTileIndexes = new int[6];
		public float AnimationSpeed = 1f;
		float LastChanged = 0f;
		int AnimationIndex = 0;
		public List<TileAnimationState> MyAnimationStates;

		// Use this for initialization
		void Start () 
		{
			Debug.Log ("Start in CubeTextureMap");    
			//OuputTexture = CreateTileMap (MyTextures, OuputTexture, 4);
			gameObject.GetComponent<MeshRenderer> ().material.mainTexture = OuputTexture;
			UpdateCubeUVs ();
			LastChanged = Time.time;
		}

		void Update() 
		{
			AnimateCubeFace ();
		}
		public void AnimateCubeFace() {
			if (MyAnimationStates.Count > 0)
			if (Time.time - LastChanged > MyAnimationStates[AnimationIndex].MyTime)
                {
				LastChanged = Time.time;
				AnimationIndex++;
				if (AnimationIndex >= MyAnimationStates.Count)
					AnimationIndex = 0;

				MeshFilter MyMeshFilter = gameObject.GetComponent<MeshFilter> ();
				Vector2[] uvs = MyMeshFilter.mesh.uv;
				//Vector2 MyPosition = GetPosition (AnimationIndex);
				Vector2[] FaceUVs = GetFaceUVs(MyAnimationStates[AnimationIndex].TileIndex);
				uvs[2*4] = FaceUVs[0];
				uvs[2*4+1] = FaceUVs[1];
				uvs[2*4+2] = FaceUVs[2];
				uvs[2*4+3] = FaceUVs[3];

				/*Vector2 TileSize = GetTileSize ();
				uvs [8] = new Vector2 (MyPosition.x + TileSize.x, MyPosition.y);
				uvs [9] = new Vector2 (MyPosition.x, MyPosition.y + TileSize.y);
				uvs [10] = new Vector2 (MyPosition.x, MyPosition.y);
				uvs [11] = new Vector2 (MyPosition.x + TileSize.x, MyPosition.y + TileSize.y);*/

				MyMeshFilter.mesh.uv = uvs;
			}
		}
		public Vector2 GetTileSize () {
			float MySizeX = MaxX;
			float MySizeY = MaxY;
			//Debug.LogError ("Testing: " + (1f / MySizeX));
			return new Vector2(1f / MySizeX, 1f / MySizeY);
		}
		public Vector2 GetPosition(int Index) {
			Vector2 TileSize = GetTileSize();
			Vector2 MyPosition = new Vector2 (Index % MaxX, Index / MaxX);
			MyPosition.x *= TileSize.x;
			MyPosition.y *= TileSize.y;
			return MyPosition;
		}
		public Vector2[] GetFaceUVs(int TileIndex) 
		{
			Vector2 TileSize = GetTileSize();
			//Debug.LogError(FaceIndex + " : " + TileSize.ToString());
			Vector2[] uvs = new Vector2[4];
			Vector2 MyPosition = GetPosition(TileIndex);
			//int i = FaceIndex * 4;
			uvs[0] = new Vector2(MyPosition.x+TileSize.x, MyPosition.y);
			uvs[1]  = new Vector2(MyPosition.x,  MyPosition.y+TileSize.y);
			uvs[2] = new Vector2(MyPosition.x,  MyPosition.y);
			uvs[3]  = new Vector2(MyPosition.x+TileSize.x, MyPosition.y+TileSize.y);
			return uvs;
		}
		public void UpdateCubeUVs() 
		{
			//Debug.Log ("Creating Cube UV Map");
			MeshFilter MyMeshFilter = gameObject.GetComponent<MeshFilter> ();
			Vector2[] uvs = MyMeshFilter.mesh.uv;
			
			if (MyMeshFilter.mesh == null || MyMeshFilter.mesh.uv.Length != 24) {
				Debug.LogError("Script needs to be attached to built-in cube");
				return;
			}
			//Debug.LogError ("TileSize: " + TileSize.ToString ());
			for (int i = 0; i < 6; i ++)
			{
				Vector2[] FaceUVs = GetFaceUVs(MyTileIndexes[i]);
				uvs[i*4] = FaceUVs[0];
				uvs[i*4+1] = FaceUVs[1];
				uvs[i*4+2] = FaceUVs[2];
				uvs[i*4+3] = FaceUVs[3];
				//Debug.LogError(i + " : " + FaceUVs[0].ToString() + " : " + FaceUVs[1].ToString() + " : " + FaceUVs[2].ToString() + " : " + FaceUVs[3].ToString());
			}
			MyMeshFilter.mesh.uv = uvs;
		}

	}
}