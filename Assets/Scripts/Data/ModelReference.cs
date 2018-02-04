using UnityEngine;
using Zeltex.Util;
using Newtonsoft.Json;
using Zeltex.Voxels;

namespace Zeltex
{
    /// <summary>
    /// A reference to a model or a unique one!
    /// </summary>
    public class ModelReference : Element
    {
        [JsonProperty]
        public string ModelLocation = "";
        [JsonProperty]
        public VoxelModel MyVoxelModel;
        [JsonProperty]
        public PolyModel MyPolyModel;

        public ModelReference()
        {
            MyVoxelModel = null;
            MyPolyModel = null;
        }

        public bool IsUnique()
        {
            return (MyVoxelModel != null || MyPolyModel != null);
        }
    }
}