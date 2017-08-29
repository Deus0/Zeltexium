using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class TestLayermask : MonoBehaviour
{
    public bool DoTest;
    public LayerMask MyLayer;

	// Update is called once per frame
	void Update ()
    {
		if (DoTest)
        {
            DoTest = false;
            MyLayer = gameObject.layer;
            Debug.LogError("Test 1: " + LayerMask.LayerToName(MyLayer));
            // test 2
            LayerMask MyObjectLayer = Mathf.RoundToInt(Mathf.Log(1 << gameObject.layer, 2));
            Debug.LogError("Test 2: " + LayerMask.LayerToName(MyObjectLayer));
        }
	}
}
