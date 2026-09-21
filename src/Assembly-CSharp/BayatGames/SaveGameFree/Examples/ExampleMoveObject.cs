using UnityEngine;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleMoveObject : MonoBehaviour
{
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.x += Input.GetAxis("Horizontal");
		position.y += Input.GetAxis("Vertical");
		base.transform.position = position;
	}
}
