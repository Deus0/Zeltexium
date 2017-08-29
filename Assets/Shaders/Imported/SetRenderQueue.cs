/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using UnityEngine;

//[AddComponentMenu("Rendering/SetRenderQueue")]
public class SetRenderQueue : MonoBehaviour
{

    [SerializeField]
    protected int[] m_queues = new int[] { 3000 };

    protected void Awake()
    {
        SetRenderQue(transform);
    }
    void SetRenderQue(Transform MyChild)
    {
        MeshRenderer MyMesh = MyChild.GetComponent<MeshRenderer>();
        if (MyMesh)
        {
            Material[] MyMaterials = MyMesh.materials;
            for (int i = 0; i < MyMaterials.Length && i < m_queues.Length; ++i)
            {
                MyMaterials[i].renderQueue = m_queues[i];
            }
        }
        for (int i = 0; i < MyChild.childCount; i++)
        {
            SetRenderQue(MyChild.GetChild(i));
        }
    }
}