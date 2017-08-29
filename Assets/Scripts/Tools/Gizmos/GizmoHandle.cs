using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Tools
{
    /// <summary>
    /// Handles the drag events per object
    /// </summary>
    public class GizmoHandle : MonoBehaviour
    {
        [HideInInspector]
        public Gizmo MyGizmo;

        public void OnRayhit(Ray MyRay, RaycastHit MyHit)
        {
            MyGizmo.OnRayhit(this, MyRay, MyHit);
        }
    }
}