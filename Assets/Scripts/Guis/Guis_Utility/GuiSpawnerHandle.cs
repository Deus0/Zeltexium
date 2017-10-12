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
                if (SpawnedGui != null)
                {
                   // Destroy(SpawnedGui);
                    GuiSpawner.Get().DestroySpawn(SpawnedGui);
                }

                SpawnedGui = GuiSpawner.Get().GetGui(GuiName);
                if (SpawnedGui == null)
                {
                    SpawnedGui = GuiSpawner.Get().SpawnGui(GuiName);
                }
                if (SpawnedGui)
                {
                    ZelGui MyGui = SpawnedGui.GetComponent<ZelGui>();
                    if (MyGui)
                    {
                        MyGui.TurnOn();
                        MyGui.Enable();
                    }
                    else
                    {
                        Debug.LogError("Unable to GetComponent ZelGui off gui: " + GuiName);
                    }
                }
                else
                {
                    Debug.LogError("Unable to spawn gui: " + GuiName);
                }
                /*if (PreviousGui)
                {
                    Destroy(PreviousGui);
                }*/
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