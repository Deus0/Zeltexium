using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Guis;


namespace Zeltex.Guis
{
    /// <summary>
    /// A quick handle to spawn guis on prefabs
    /// </summary>
    public class GuiSpawnerHandle : MonoBehaviour
    {
        [Tooltip("Destroys the gui if referenced in the scene")]
        public GameObject PreviousGui;
        private GameObject SpawnedGui;

        public void SpawnGui(string GuiName)
        {
            if (GuiSpawner.Get())
            {
                if (SpawnedGui == null)
                {
                    SpawnedGui = GuiSpawner.Get().SpawnGui(GuiName);
                    if (PreviousGui)
                    {
                        Destroy(PreviousGui);
                    }
                }
                else
                {
                    Destroy(SpawnedGui);
                }
            }
        }

        public void DespawnGui(GameObject MyGui)
        {
            if (GuiSpawner.Get())
            {
                GuiSpawner.Get().DestroySpawn(MyGui);
            }
        }

        public void EnableGui(string GuiName)
        {
            if (GuiSpawner.Get())
            {
                GuiSpawner.Get().EnableGui(GuiName);
            }
        }

        public void DisableGui(string GuiName)
        {
            if (GuiSpawner.Get())
            {
                GuiSpawner.Get().DisableGui(GuiName);
            }
        }

        public void DisableCurrentGui()
        {
            if (PreviousGui)
            {
                ZelGui MyGui = PreviousGui.GetComponent<ZelGui>();
                if (MyGui)
                {
                    MyGui.Disable();
                }
            }
        }
    }

}