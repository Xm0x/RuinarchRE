using UnityEngine;

public class FollowMouseUtility : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		base.transform.position = mousePosition;
	}
}
