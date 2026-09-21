using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubGoalNotificationUI : MonoBehaviour
{
	[SerializeField]
	private Transform notificationParent;

	[SerializeField]
	private GameObject prefabNotification;

	private uint _activeNotifs;

	private List<SubGoal> _pendingNotifs;

	private void Awake()
	{
		_pendingNotifs = new List<SubGoal>(30);
		_activeNotifs = 0u;
	}

	public void ShowNotification(SubGoal p_subGoal)
	{
		if (_activeNotifs >= 3)
		{
			_pendingNotifs.Add(p_subGoal);
			return;
		}
		_pendingNotifs.Remove(p_subGoal);
		ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabNotification.name, Vector3.zero, Quaternion.identity, notificationParent).GetComponent<SubGoalNotificationItem>().Initialize(p_subGoal, OnDestroyNotification, 7f);
		_activeNotifs++;
	}

	private void TryShowPendingNotif()
	{
		if (_pendingNotifs.Count > 0 && _activeNotifs < 3)
		{
			ShowNotification(_pendingNotifs.First());
		}
	}

	private void OnDestroyNotification()
	{
		_activeNotifs--;
		TryShowPendingNotif();
	}
}
