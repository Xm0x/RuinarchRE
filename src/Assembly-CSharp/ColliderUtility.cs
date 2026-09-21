using UnityEngine;

public class ColliderUtility : MonoBehaviour
{
	private Collider2D _collider;

	private void Awake()
	{
		_collider = GetComponent<Collider2D>();
	}

	[ContextMenu("Get Contact Points")]
	public void GetContactPoints()
	{
		Collider2D[] array = new Collider2D[100];
		int contacts = _collider.GetContacts(array);
		for (int i = 0; i < contacts; i++)
		{
			Debug.Log(array[i].name);
		}
	}

	[ContextMenu("Get Collisions")]
	public void GetCollisions()
	{
		Collider2D[] array = new Collider2D[100];
		int num = _collider.OverlapCollider(default(ContactFilter2D).NoFilter(), array);
		for (int i = 0; i < num; i++)
		{
			Debug.Log(array[i].name);
		}
	}
}
