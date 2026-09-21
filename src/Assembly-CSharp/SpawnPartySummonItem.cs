using System;
using System.Collections.Generic;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnPartySummonItem : PooledObject
{
	[SerializeField]
	private RuinarchButton btn;

	[SerializeField]
	private Image imgPortrait;

	[SerializeField]
	private GameObject goCover;

	[SerializeField]
	private GameObject goCheckmark;

	[SerializeField]
	private GameObject goUsedCharges;

	[SerializeField]
	private TextMeshProUGUI lblUsedCharges;

	[SerializeField]
	private HoverHandler hoverHandler;

	private MonsterAndDemonUnderlingCharges _underlingCharges;

	private Action<SpawnPartySummonItem> _onLeftClickAction;

	private Action<SpawnPartySummonItem> _onHoverOverAction;

	private Action<SpawnPartySummonItem> _onHoverOutAction;

	public MonsterAndDemonUnderlingCharges data => _underlingCharges;

	private void OnEnable()
	{
		btn.onClick.AddListener(OnLeftClick);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDisable()
	{
		btn.onClick.RemoveListener(OnLeftClick);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	public void Initialize(MonsterAndDemonUnderlingCharges p_underlingCharges, Action<SpawnPartySummonItem> p_onLeftClick, Action<SpawnPartySummonItem> p_onHoverOver, Action<SpawnPartySummonItem> p_onHoverOut)
	{
		_underlingCharges = p_underlingCharges;
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(_underlingCharges.characterClassName);
		imgPortrait.sprite = characterClass.smallPortraitSprite;
		goUsedCharges.SetActive(!p_underlingCharges.isDemon);
		goCheckmark.SetActive(value: false);
		lblUsedCharges.text = "0/" + p_underlingCharges.currentCharges;
		_onLeftClickAction = p_onLeftClick;
		_onHoverOverAction = p_onHoverOver;
		_onHoverOutAction = p_onHoverOut;
		bool flag = p_underlingCharges.currentCharges > 0;
		goCover.SetActive(!flag);
		btn.interactable = flag;
	}

	public void SetCheckmarkState(bool p_state)
	{
		goCheckmark.SetActive(p_state);
	}

	public void UpdateUsedCharges(List<MonsterAndDemonUnderlingCharges> p_allUsedCharges)
	{
		int usedCharges = GetUsedCharges(p_allUsedCharges);
		lblUsedCharges.text = usedCharges + "/" + _underlingCharges.currentCharges;
		SetCoverState(usedCharges >= _underlingCharges.currentCharges);
	}

	private int GetUsedCharges(List<MonsterAndDemonUnderlingCharges> p_allUsedCharges)
	{
		int num = 0;
		for (int i = 0; i < p_allUsedCharges.Count; i++)
		{
			if (p_allUsedCharges[i] == _underlingCharges)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasRemainingCharges(List<MonsterAndDemonUnderlingCharges> p_allUsedCharges)
	{
		int usedCharges = GetUsedCharges(p_allUsedCharges);
		return _underlingCharges.currentCharges - usedCharges > 0;
	}

	private void SetCoverState(bool p_state)
	{
		goCover.SetActive(p_state);
		btn.interactable = !p_state;
	}

	private void OnLeftClick()
	{
		_onLeftClickAction?.Invoke(this);
	}

	private void OnHoverOver()
	{
		_onHoverOverAction?.Invoke(this);
	}

	private void OnHoverOut()
	{
		_onHoverOutAction?.Invoke(this);
	}

	public override void Reset()
	{
		_underlingCharges = null;
		_onLeftClickAction = null;
		goCheckmark.SetActive(value: false);
		goUsedCharges.SetActive(value: false);
		goCover.SetActive(value: false);
	}
}
