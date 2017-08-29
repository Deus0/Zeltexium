using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeltex.Util;
using Zeltex.Voxels;

namespace Zeltex.Guis
{
    /// <summary>
    /// Main viewer for a voxel mesh
    /// To Do:
    ///     - use previous lines for cube for drawing
    ///     - selection of voxels with control click
    /// </summary>
    public class VoxelViewer : ObjectViewer
    {
        #region Variables
        [Header("VoxelViewer")]
        public RawImage TextureViewer;
        #endregion

        /// <summary>
        /// Extends ZelGui OnBegin function
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();
            GetSpawn().GetComponent<World>().MyDataBase = VoxelManager.Get();
            GetSpawn().GetComponent<World>().MyUpdater = WorldUpdater.Get();
        }

        public IEnumerator RunScript(List<string> MyScript)
        {
            //Debug.LogError("Loading new script of size: " + MyScript.Count);
            yield return GetSpawn().GetComponent<World>().RunScriptRoutine(MyScript);
            if (GetSpawn().GetComponent<World>().MyDataBase.GetMaterial(0) && TextureViewer)
            {//GetImage("TextureViewer")
                TextureViewer.texture = GetSpawn().GetComponent<World>().MyDataBase.GetMaterial(0).mainTexture;
            }
            Int3 MySize = GetSpawn().GetComponent<World>().GetWorldSizeChunks();
            MySize.x++;
            MySize.y++;
            MySize *= 8 * 1.05f;
            float MultiplierZ = MySize.y;
            if (MySize.x > MySize.y)
            {
                MultiplierZ = MySize.x;
            }

            Vector3 NewPosition = new Vector3(
                GetSpawn().transform.position.x, 
                GetSpawn().transform.position.y, 
                GetSpawn().transform.lossyScale.z * MultiplierZ * 1.45f);
            MyCamera.transform.position = NewPosition;
            MyCamera.transform.LookAt(GetSpawn().transform.position);
        }
    }
}
