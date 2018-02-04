﻿using UnityEngine;
using System.Collections;
using Zeltex.Voxels;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using ZeltexTools;
using Zeltex;

namespace Zeltex.Guis
{
    /// <summary>
    /// Main editor for Polygonal meshes used in a voxel grid.
    /// </summary>
    public class VoxelPolygonViewer : ObjectViewer
    {
        #region Mesh

        public void LoadMeta(string MetaName)
        {
            VoxelMeta MyVoxel = VoxelManager.Get().GetMeta(MetaName);
            if (MyVoxel != null)
            {
                LoadVoxelMesh(MyVoxel);
            }
            else
            {
                Debug.LogError("Meta not found: " + MetaName);
            }
        }

        /// <summary>
        /// Purely for loading models, used ??
        /// </summary>
        public void LoadVoxelMesh(string ModelIndex)
        {
            LoadVoxelMesh(ModelIndex, 0);
        }

        /// <summary>
        /// Load in the voxel mesh, used externally to load a voxel meta
        /// </summary>
        public void LoadVoxelMesh(VoxelMeta MyVoxel)
        {
            if (MyVoxel != null)
            {
                LoadVoxelMesh(MyVoxel.ModelID, MyVoxel.TextureMapID);
            }
            else
            {
                Debug.LogError("Meta not found.");
            }
        }

        /// <summary>
        /// Load in the voxel mesh, used by polygon maker to edit exact data on the model/texture
        /// </summary>
        public void LoadVoxelMesh(string ModelName, int TextureMapIndex)
        {
            if (GetSpawn())
            {
                PolyModelHandle MyModelHandle = GetSpawn().GetComponent<PolyModelHandle>();
                if (MyModelHandle)
                {
                    GetSpawn().GetComponent<PolyModelHandle>().LoadVoxelMesh(DataManager.Get().GetElement(DataFolderNames.PolyModels, ModelName) as PolyModel, TextureMapIndex);
                }
                else
                {
                    Debug.LogError("Spawn has no PolyModelHandle: " + name);
                }
            }
            else
            {

                Debug.LogError(name + " has no spawn.");
            }
            //LoadedModel = MyVoxelManager.GetModel(ModelName);
			//LoadVoxelMesh(LoadedModel, TextureMapIndex, true);
		}

		/// <summary>
		/// Load in the voxel mesh, used by polygon maker to edit exact data on the model/texture
		/// </summary>
		public void LoadVoxelMesh(PolyModel MyModel, int TextureMapIndex)
        {
            GetSpawn().GetComponent<PolyModelHandle>().LoadVoxelMesh(MyModel, TextureMapIndex);
            /*LoadedModel = MyModel;
			if (MyModel != null)
			{
				LoadVoxelMesh(LoadedModel, TextureMapIndex, true);
			}
			else
			{
				Debug.LogError("Model is null.");
				ClearMesh();
			}*/
		}

		/// <summary>
		/// Load in the voxel mesh
		/// </summary>
		public void LoadVoxelMesh(PolyModel MyModel, int TextureMapIndex, bool IsRefreshHandlers)
		{
            GetSpawn().GetComponent<PolyModelHandle>().LoadVoxelMesh(MyModel, TextureMapIndex);
			/*LoadedModelName = MyModel.Name;
            //Debug.LogError("PolygonViewer [" + name + "] is loading [" + MyModel.Name + "] with texture map [" + TextureMapIndex + "]");
			List<string> SelectedHandlerNames = new List<string>();
            if (IsRefreshHandlers)
            {
                for (int i = 0; i < SelectedHandlers.Count; i++)
                {
                    SelectedHandlerNames.Add(SelectedHandlers[i].name);
                }
                UpdateHandlerMode(HandlerMode);
            }
            LoadedTextureMap = TextureMapIndex;
            MyVoxelManager.UpdateWithSingleVoxelMesh(GetSpawn(), MyModel.Name, TextureMapIndex);
            // after loaded - check if any handlers are out of bound, and reassign them if they arn't
            if (IsRefreshHandlers && HandlerMode == 1)
            {
                int VertCount = MyModel.GetAllVerts().Count;
                for (int i = 0; i < SelectedHandlerNames.Count; i++)
                {
                    int VertIndex = HandlerNameToIndex(SelectedHandlerNames[i]);
                    if (VertIndex >= 0 && VertIndex < VertCount)
                    {
                        string MyName = "VertHandler_" + VertIndex;
                        for (int j = 0; j < MyHandlers.Count; j++)
                        {
                            if (MyHandlers[j].name == MyName)
                            {
                                SelectHandler(MyHandlers[j]);
                                break;
                            }
                        }
                    }
                }
            }*/
        }

        /// <summary>
        /// Clear the mesh!
        /// </summary>
        public void ClearMesh()
        {
            GetSpawn().GetComponent<PolyModelHandle>().ClearHandlers();
            if (GetSpawn().GetComponent<MeshFilter>().sharedMesh == null)
            {
                GetSpawn().GetComponent<MeshFilter>().sharedMesh = new Mesh(); // clear mesh
            }
            else
            {
                GetSpawn().GetComponent<MeshFilter>().sharedMesh.Clear();
            }
        }

        /// <summary>
        /// Update the mesh components with the mesh data
        /// </summary>
        private void UpdateMesh(MeshFilter MyMeshFilter, MeshRenderer MyMeshRenderer, MeshCollider MyMeshCollider, MeshData MyMeshData)
        {
            if (MyMeshFilter.sharedMesh == null)
            {
                MyMeshFilter.sharedMesh = new Mesh(); // clear mesh
            }
            else
            {
                MyMeshFilter.sharedMesh.Clear();
            }
            MyMeshFilter.sharedMesh.vertices = MyMeshData.GetVerticies();
            MyMeshFilter.sharedMesh.triangles = MyMeshData.GetTriangles();
            MyMeshFilter.sharedMesh.uv = MyMeshData.GetTextureCoordinates();
            MyMeshFilter.sharedMesh.SetColors(MyMeshData.GetColors());
            MyMeshFilter.sharedMesh.RecalculateNormals();
            // colliders
            if (MyMeshCollider)
            { 
                MyMeshCollider.sharedMesh = MyMeshFilter.sharedMesh;   // to make sure it resets
            }
        }
        #endregion
    }
}

/*[Header("VoxelPolygonViewer")]
public UVViewer MyUVViewer;
public VoxelManager MyVoxelManager;
//public PolygonMaker MyMaker;
public Material VertMaterial;
public Material FaceMaterial;
private int HandlerMode = 0;
private Color32 NormalFaceColor = new Color32(0, 255, 76, 11);
private Color32 SelectedFaceColor = new Color32(255, 76, 76, 68);
private float VertSize = 0.03f;
private string LoadedModelName;
private PolyModel LoadedModel;
private int LoadedTextureMap;
private List<GameObject> MyHandlers = new List<GameObject>();
private List<GameObject> SelectedHandlers = new List<GameObject>();
bool IsAreaSelect = true;
float AreaSelectionRadius = 0.1f;*/

/*public override void UpdatePositionX()
{
    if (SelectedHandlers.Count > 0)
    {
        //float OldX = SelectedHandlers[SelectedHandlers.Count - 1].transform.localPosition.x;
        float NewX = float.Parse(InputPositionX.text);
        Vector3 NewPosition = SelectedHandlers[SelectedHandlers.Count - 1].transform.localPosition;
        NewPosition.x = NewX;
        Vector3 DifferencePosition = SelectedHandlers[SelectedHandlers.Count - 1].transform.localPosition - NewPosition;
        for (int i = 0; i < SelectedHandlers.Count; i++)
        {
            MoveHandler(SelectedHandlers[i], DifferencePosition);
        }
        RefreshModel();
    }
}*/
