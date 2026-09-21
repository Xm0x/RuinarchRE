using System;
using Ruinarch;
using UnityEngine;

public class PointerDirection : MonoBehaviour
{
	public Action<PointerDirection, Vector3> OnClick;

	public Action<PointerDirection> OnCancel;

	private float offset = 270f;

	private bool _wasPlacedThisFrame;

	public static int ActiveCount { get; set; }

	private void OnEnable()
	{
		_wasPlacedThisFrame = true;
		ActiveCount++;
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.AddListener(ControlsSignals.ON_CLOSE_OPEN_POP_UPS, OnMenuClosedByCancel);
	}

	private void OnDisable()
	{
		ActiveCount--;
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.RemoveListener(ControlsSignals.ON_CLOSE_OPEN_POP_UPS, OnMenuClosedByCancel);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Center_Portal || p_action == SHORTCUT_ACTION.Snatch_Villager || p_action == SHORTCUT_ACTION.Snatch_Monster || p_action == SHORTCUT_ACTION.Cycle_Eyes)
		{
			OnCancel?.Invoke(this);
		}
	}

	private void OnMenuClosedByCancel()
	{
		OnCancel?.Invoke(this);
	}

	private void Update()
	{
		if (_wasPlacedThisFrame)
		{
			_wasPlacedThisFrame = false;
			return;
		}
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
		Vector3 vector2 = vector - base.transform.position;
		vector2.Normalize();
		float num = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, num + offset);
		base.transform.rotation = quaternion;
		Vector3 vector3 = quaternion * Vector3.up;
		Vector3 arg = ((!(Vector2.Distance(base.transform.position, vector) < 1f)) ? vector : (base.transform.position + vector3 * 100f));
		if (InputManager.Instance.GetMouseButtonDown(0))
		{
			if (UIManager.Instance.IsMouseOnUI())
			{
				OnCancel?.Invoke(this);
			}
			else
			{
				OnClick?.Invoke(this, arg);
			}
		}
	}
}
