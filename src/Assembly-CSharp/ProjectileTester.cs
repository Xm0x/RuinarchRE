using UnityEngine;
using UtilityScripts;

public class ProjectileTester : MonoBehaviour
{
	[SerializeField]
	private GameObject[] projectilePrefabs;

	[SerializeField]
	private Transform target;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			CreateNewProjectile();
		}
	}

	private void CreateNewProjectile()
	{
		Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		position.z = 0f;
		Object.Instantiate(CollectionUtilities.GetRandomElement(projectilePrefabs), position, Quaternion.identity).GetComponent<Projectile>().SetTarget(target, null, null, null);
	}
}
