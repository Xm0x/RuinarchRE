using System;
using UnityEngine;

public class FollowerComponent : MonoBehaviour
{
	private Transform targetTransform;

	private Action onReachTarget;

	private float speed = 5f;

	public void SetTarget(Transform target, Action onReachTarget)
	{
		Vector3 vector = target.position - base.transform.position;
		vector.Normalize();
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		base.transform.rotation = Quaternion.Euler(0f, 0f, num - 90f);
		targetTransform = target;
		this.onReachTarget = onReachTarget;
		Messenger.AddListener<GameObject>(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
	}

	private void FixedUpdate()
	{
		if (!GameManager.Instance.isPaused && !(targetTransform == null))
		{
			float maxDistanceDelta = speed * Time.deltaTime;
			base.transform.position = Vector2.MoveTowards(base.transform.position, targetTransform.position, maxDistanceDelta);
			if ((base.transform.position - targetTransform.position).magnitude < 1f)
			{
				OnReachTarget();
			}
		}
	}

	private void OnReachTarget()
	{
		Debug.Log(base.name + " has reached target");
		onReachTarget?.Invoke();
		targetTransform = null;
		UnityEngine.Object.Destroy(this);
	}

	private void OnPooledObjectDestroyed(GameObject go)
	{
		if (go.transform == targetTransform)
		{
			targetTransform = null;
			UnityEngine.Object.Destroy(this);
		}
	}
}
