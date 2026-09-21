using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;
using UnityEngine.UI;

public class SummonPartyItem : PooledObject
{
	[SerializeField]
	private Transform transformMemberItemsParent;

	[SerializeField]
	private GameObject prefabSummonActiveItem;

	[SerializeField]
	private GameObject goPartyDivider;

	private Party _party;

	private List<SummonActiveItem> _items;

	private UIHoverPosition _tooltipPos;

	public Party party => _party;

	private void Awake()
	{
		_items = new List<SummonActiveItem>();
	}

	public void Initialize(Party p_party, UIHoverPosition p_tooltipPos)
	{
		_party = p_party;
		_tooltipPos = p_tooltipPos;
		for (int i = 0; i < p_party.members.Count; i++)
		{
			Character character = p_party.members[i];
			if (!character.isDead)
			{
				CreateMemberItem(character);
			}
		}
		SubscribeListeners();
		LayoutRebuilder.ForceRebuildLayoutImmediate(transformMemberItemsParent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}

	public void UpdatePartyDividerState()
	{
		goPartyDivider.SetActive(base.transform.GetSiblingIndex() < base.transform.parent.childCount - 1);
	}

	private void CreateMemberItem(Character p_character)
	{
		if (!HasMemberItem(p_character))
		{
			SummonActiveItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabSummonActiveItem.name, Vector3.zero, Quaternion.identity, transformMemberItemsParent).GetComponent<SummonActiveItem>();
			component.Initialize(p_character, OnHoverOverMember, OnHoverOutMember);
			_items.Add(component);
			goPartyDivider.SetActive(value: true);
			goPartyDivider.transform.SetAsLastSibling();
			UpdatePartyDividerState();
			LayoutRebuilder.ForceRebuildLayoutImmediate(transformMemberItemsParent as RectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
		}
	}

	private void DeleteMemberItem(Character p_character)
	{
		SummonActiveItem memberItem = GetMemberItem(p_character);
		if (memberItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(memberItem);
			_items.Remove(memberItem);
			LayoutRebuilder.ForceRebuildLayoutImmediate(transformMemberItemsParent as RectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
		}
	}

	private SummonActiveItem GetMemberItem(Character p_character)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			SummonActiveItem summonActiveItem = _items[i];
			if (summonActiveItem.character == p_character)
			{
				return summonActiveItem;
			}
		}
		return null;
	}

	private bool HasMemberItem(Character p_character)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			if (_items[i].character == p_character)
			{
				return true;
			}
		}
		return false;
	}

	private void OnHoverOverMember(SummonActiveItem p_item)
	{
		UIManager.Instance.ShowCharacterNameplateTooltip(p_item.character, _tooltipPos);
	}

	private void OnHoverOutMember(SummonActiveItem p_item)
	{
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Party, Character>(PartySignals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
		Messenger.RemoveListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY, OnCharacterLeftParty);
	}

	private void OnCharacterJoinedParty(Party p_party, Character p_character)
	{
		if (p_party == _party)
		{
			CreateMemberItem(p_character);
		}
	}

	private void OnCharacterLeftParty(Party p_party, Character p_character)
	{
		if (p_party == _party)
		{
			DeleteMemberItem(p_character);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_party = null;
		for (int i = 0; i < _items.Count; i++)
		{
			ObjectPoolManager.Instance.DestroyObject(_items[i]);
		}
		_items.Clear();
		UnsubscribeListeners();
	}
}
