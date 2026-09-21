using UnityEngine;

public class DragTarget : MonoBehaviour
{
	private Transform t;

	private Camera mainCam;

	private Vector3 offset;

	private void Start()
	{
		t = base.transform;
		mainCam = Camera.main;
	}

	private void OnMouseDown()
	{
		Vector2 vector = Input.mousePosition;
		float z = mainCam.WorldToScreenPoint(t.position).z;
		Vector3 vector2 = mainCam.ScreenToWorldPoint(new Vector3(vector.x, vector.y, z));
		offset = t.position - vector2;
	}

	private void OnMouseDrag()
	{
		Vector2 vector = Input.mousePosition;
		float z = mainCam.WorldToScreenPoint(t.position).z;
		t.position = mainCam.ScreenToWorldPoint(new Vector3(vector.x, vector.y, z)) + offset;
	}
}
