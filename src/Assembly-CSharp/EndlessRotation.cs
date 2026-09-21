using UnityEngine;

public class EndlessRotation : MonoBehaviour
{
	public int speed;

	private void Update()
	{
		base.transform.Rotate(0f, 0f, (float)speed * Time.deltaTime);
	}
}
