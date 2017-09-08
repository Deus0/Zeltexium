using UnityEngine;
using System.Collections;

public class DeleteDefaultData : MonoBehaviour {

	public void DeleteData()
    {
        FileManagement.DeleteFile("data.txt");
    }
}
