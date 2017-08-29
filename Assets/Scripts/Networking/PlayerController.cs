using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour
{
	private void Start()
	{
		if (GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			Camera.main.transform.SetParent(transform);
			Camera.main.transform.localPosition = new Vector3(0 , Camera.main.transform.localPosition.y, 0);
			GetComponent<MeshRenderer>().material.color = Color.cyan;
		}
		else
		{
			GetComponent<MeshRenderer>().material.color = Color.red;
		}
	}

	void Update()
	{
		if (GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
			var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

			transform.Rotate(0, x, 0);
			transform.Translate(0, 0, z);
		}
		else
		{
			var x = 1 * Time.deltaTime * 150.0f;
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				x *= -1;
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{

			}
			else
			{
				x = 0;
			}
			var z = 1 * Time.deltaTime * 3.0f;
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				z *= -1;
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{

			}
			else
			{
				z = 0;
			}
			transform.Rotate(0, x, 0);
			transform.Translate(0, 0, z);
		}
	}
}