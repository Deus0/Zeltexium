﻿using UnityEngine;
using System.Collections;
using Zeltex.Items;

namespace Zeltex.Guis
{
    /// <summary>
    /// To Simply view a polygonal mesh
    /// </summary>
    public class MeshViewer : ObjectViewer
    {
        #region Variables
        [Header("MeshViewer")]
        public ItemManager MyItemManager;
        public Material MyMaterial;
        public Vector3 MyScale = new Vector3(1, 1, 1);
        #endregion

        public override void OnBegin()
        {
            base.OnBegin();
            GetSpawn().GetComponent<MeshFilter>().sharedMesh = MyItemManager.GetMesh("Texture_0");
            GetSpawn().transform.localScale = MyScale;
        }

        public void UpdateMeshWithName(string MeshName)
        {
            GetSpawn().GetComponent<MeshFilter>().sharedMesh = MyItemManager.GetMesh(MeshName);
            GetSpawn().GetComponent<MeshRenderer>().material = MyMaterial;
        }
        public void SetMesh(Mesh MyMesh)
        {
            if (MyMesh == null)
            {
                MyMesh = new Mesh();
            }
            // Destroy previous mesh!
            if (GetSpawn().GetComponent<MeshFilter>().sharedMesh != null)
            {
                GetSpawn().GetComponent<MeshFilter>().sharedMesh.Die();
            }
            GetSpawn().GetComponent<MeshFilter>().sharedMesh = MyMesh;
        }
    }
}
