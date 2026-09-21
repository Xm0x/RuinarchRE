using System;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PlayerNotificationItem
{
	[SerializeField]
	private Button getIntelBtn;

	[SerializeField]
	private GameObject convertTooltip;

	[SerializeField]
	private GameObject effectPrefab;

	private string _intelHoverText;

	public IIntel intel { get; private set; }

	public void Initialize(IIntel intel, Action<PlayerNotificationItem> onDestroyAction = null)
	{
		this.intel = intel;
		Initialize(intel.log, onDestroyAction);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
	}

	public void GetIntel()
	{
		Vector3 position = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(getIntelBtn.transform.position);
		position.z = 0f;
		GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(effectPrefab.name, position, Quaternion.identity, InnerMapManager.Instance.transform, isWorldPosition: true);
		effectGO.transform.position = position;
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.intelToggle.transform.position);
		Vector3 position2 = effectGO.transform.position;
		position2.x -= 5f;
		position2.z = 0f;
		Vector3 vector2 = vector;
		vector2.y -= 5f;
		vector2.z = 0f;
		IIntel storedIntel = intel;
		effectGO.transform.DOPath(new Vector3[3] { vector, position2, vector2 }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(delegate
		{
			OnReachIntelTab(effectGO, storedIntel);
		});
		DeleteNotification();
	}

	private void OnReachIntelTab(GameObject effectGO, IIntel intel)
	{
		PlayerUI.Instance.DoIntelTabPunchEffect();
		ObjectPoolManager.Instance.DestroyObject(effectGO);
		PlayerManager.Instance.player.AddIntel(intel);
		AudioManager.Instance.TryPlayUISFX("Play_Store_Intel");
	}

	protected override void OnCharacterChangedName(Character character)
	{
		if (intel is InterruptIntel interruptIntel)
		{
			interruptIntel.interruptHolder.effectLog.TryUpdateLogAfterRename(character);
		}
		else if (intel is ActionIntel actionIntel)
		{
			actionIntel.node.descriptionLog.TryUpdateLogAfterRename(character);
		}
		logLbl.text = intel.log.logText;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(InstantHeight());
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (intel is ActionIntel actionIntel)
		{
			bool flag = false;
			actionIntel.node.DisconnectFromCharacter(p_character);
			if (actionIntel.node.IsNodeObjectInvalid() || actionIntel.node.IsCharacterReferenced(p_character))
			{
				flag = true;
			}
			if (flag)
			{
				intel.OnIntelRemoved();
				DeleteNotification();
			}
		}
		else if (intel is InterruptIntel interruptIntel)
		{
			interruptIntel.interruptHolder.DisconnectFromCharacter(p_character);
			if (interruptIntel.interruptHolder.IsImportantDataNull() || interruptIntel.interruptHolder.IsCharacterReferenced(p_character))
			{
				intel.OnIntelRemoved();
				DeleteNotification();
			}
		}
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (intel is ActionIntel actionIntel)
		{
			if (actionIntel.node.IsNodeObjectInvalid() || actionIntel.node.IsStructureReferenced(p_structure))
			{
				intel.OnIntelRemoved();
				DeleteNotification();
			}
		}
		else if (intel is InterruptIntel interruptIntel && (interruptIntel.interruptHolder.IsImportantDataNull() || interruptIntel.interruptHolder.IsStructureReferenced(p_structure)))
		{
			intel.OnIntelRemoved();
			DeleteNotification();
		}
	}

	public void OnHoverEnter()
	{
		if (intel != null && string.IsNullOrEmpty(_intelHoverText))
		{
			_intelHoverText = intel.GetFullIntelTooltip();
		}
		UIManager.Instance.ShowSmallInfo(_intelHoverText, _hoverPosition, "", autoReplaceText: false, relayout: true);
	}

	public void OnHoverExit()
	{
		_intelHoverText = string.Empty;
		UIManager.Instance.HideSmallInfo();
	}

	public override void DeleteOldestNotification()
	{
		intel.OnIntelRemoved();
		base.DeleteOldestNotification();
	}

	public override void Reset()
	{
		base.Reset();
		_intelHoverText = string.Empty;
		convertTooltip.SetActive(value: false);
		intel = null;
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
	}
}
