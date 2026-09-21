using Ruinarch;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacterInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Equips")]
	[SerializeField]
	private Image weaponImg;

	[SerializeField]
	private Image armorImg;

	[SerializeField]
	private Image accessoryImg;

	[SerializeField]
	private HoverHandler weaponHoverText;

	[SerializeField]
	private HoverHandler armorHoverText;

	[SerializeField]
	private HoverHandler accessoryHoverText;

	[SerializeField]
	private EventEquipButton weaponEventButton;

	[SerializeField]
	private EventEquipButton armorEventButton;

	[SerializeField]
	private EventEquipButton accessoryEventButton;

	[SerializeField]
	private EquipmentToolTip equipmentToolTip;

	protected Character _activeCharacter;

	protected Character _previousCharacter;

	public Character activeCharacter => _activeCharacter;

	internal override void Initialize()
	{
		base.Initialize();
		ListenEquipmentHoverListener();
		weaponEventButton.AddPointerRightClickAction(OnRightClickEquipment);
		armorEventButton.AddPointerRightClickAction(OnRightClickEquipment);
		accessoryEventButton.AddPointerRightClickAction(OnRightClickEquipment);
		Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.WEAPON_UNEQUIPPED, OnEquipmentUnequipped);
		Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.ARMOR_UNEQUIPPED, OnEquipmentUnequipped);
		Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.ACCESSORY_UNEQUIPPED, OnEquipmentUnequipped);
		Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.CHARACTER_EQUIPPED_ITEM, OnCharacterEquippedItem);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (_activeCharacter == p_character)
		{
			CloseMenu();
		}
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		Selector.Instance.Deselect();
		Character character = _activeCharacter;
		_activeCharacter = null;
		if (character != null && (object)character.marker != null)
		{
			character.marker.ForceUpdateSortingOrder();
			if (InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == character.marker.gameObject.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
			character.marker.UpdateNameplateElementsState();
		}
	}

	public override void OpenMenu()
	{
		_previousCharacter = _activeCharacter;
		_activeCharacter = _data as Character;
		base.OpenMenu();
		if (_previousCharacter != null && _previousCharacter.hasMarker)
		{
			_previousCharacter.marker.ForceUpdateSortingOrder();
			if (_previousCharacter.grave == null || _previousCharacter.grave.isBeingCarriedBy == null)
			{
				_previousCharacter.marker.UpdateNameplateElementsState();
			}
		}
		if (UIManager.Instance.IsConversationMenuOpen())
		{
			backButton.interactable = false;
		}
		if (UIManager.Instance.IsObjectPickerOpen())
		{
			UIManager.Instance.HideObjectPicker();
		}
		if (!_activeCharacter.marker || !(_activeCharacter.marker.transform != null))
		{
			return;
		}
		_activeCharacter.marker.ForceUpdateSortingOrder();
		if (_activeCharacter.tileObjectComponent.isUsingBed)
		{
			if ((bool)_activeCharacter.tileObjectComponent.bedBeingUsed.mapObjectVisual)
			{
				Selector.Instance.Select(_activeCharacter.tileObjectComponent.bedBeingUsed, _activeCharacter.tileObjectComponent.bedBeingUsed.mapObjectVisual.transform);
			}
		}
		else
		{
			Selector.Instance.Select(_activeCharacter, _activeCharacter.marker.transform);
		}
		if (_activeCharacter.grave == null || _activeCharacter.grave.isBeingCarriedBy == null)
		{
			_activeCharacter.marker.UpdateNameplateElementsState();
		}
	}

	public void TryUpdateCharacterInfo()
	{
		if (_activeCharacter != null)
		{
			UpdateCharacterInfo();
		}
	}

	protected virtual void UpdateCharacterInfo()
	{
		if (_activeCharacter != null)
		{
			UpdateEquipmentDisplay();
		}
	}

	private void UpdateEquipmentDisplay()
	{
		if (_activeCharacter.equipmentComponent.currentWeapon != null)
		{
			weaponImg.enabled = true;
			weaponImg.sprite = _activeCharacter.equipmentComponent.currentWeapon.equipmentData.imgIcon;
			weaponEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentWeapon);
		}
		else
		{
			weaponImg.enabled = false;
			weaponEventButton.ClearData();
		}
		if (_activeCharacter.equipmentComponent.currentArmor != null)
		{
			armorImg.enabled = true;
			armorImg.sprite = _activeCharacter.equipmentComponent.currentArmor.equipmentData.imgIcon;
			armorEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentArmor);
		}
		else
		{
			armorImg.enabled = false;
			armorEventButton.ClearData();
		}
		if (_activeCharacter.equipmentComponent.currentAccessory != null)
		{
			accessoryImg.enabled = true;
			accessoryImg.sprite = _activeCharacter.equipmentComponent.currentAccessory.equipmentData.imgIcon;
			accessoryEventButton.SetData(_activeCharacter, _activeCharacter.equipmentComponent.currentAccessory);
		}
		else
		{
			accessoryImg.enabled = false;
			accessoryEventButton.ClearData();
		}
	}

	private void OnHoverExitEquipment()
	{
		equipmentToolTip.gameObject.SetActive(value: false);
	}

	private void OnHoverEquipment(EquipmentItem p_equipmentItem)
	{
		if (p_equipmentItem != null)
		{
			equipmentToolTip.gameObject.SetActive(value: true);
			equipmentToolTip.ShowEquipmentItem(p_equipmentItem);
		}
	}

	private void ListenEquipmentHoverListener()
	{
		weaponHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverEquipment(activeCharacter.equipmentComponent.currentWeapon);
		});
		armorHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverEquipment(activeCharacter.equipmentComponent.currentArmor);
		});
		accessoryHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverEquipment(activeCharacter.equipmentComponent.currentAccessory);
		});
		weaponHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
		armorHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
		accessoryHoverText.AddOnHoverOutAction(OnHoverExitEquipment);
	}

	private void OnEquipmentUnequipped(Character p_character, EquipmentItem p_unequipped)
	{
		if (isShowing && activeCharacter == p_character)
		{
			UpdateEquipmentDisplay();
		}
	}

	private void OnRightClickEquipment(Character p_owner, TileObject targetWeapon)
	{
		if (targetWeapon != null)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(targetWeapon, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	private void OnCharacterEquippedItem(Character p_character, EquipmentItem p_unequipped)
	{
		if (isShowing && activeCharacter == p_character)
		{
			UpdateEquipmentDisplay();
		}
	}
}
