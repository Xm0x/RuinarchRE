using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AreaPickerItem : ObjectPickerItem<NPCSettlement>, IPointerClickHandler, IEventSystemHandler
{
	public Action<NPCSettlement> onClickAction;

	private NPCSettlement _npcSettlement;

	[SerializeField]
	private LocationPortrait portrait;

	public GameObject portraitCover;

	public override NPCSettlement obj => _npcSettlement;

	public void SetArea(NPCSettlement npcSettlement)
	{
		_npcSettlement = npcSettlement;
		UpdateVisuals();
	}

	public override void SetButtonState(bool state)
	{
		base.SetButtonState(state);
		portraitCover.SetActive(!state);
	}

	private void UpdateVisuals()
	{
		portrait.SetLocation(_npcSettlement.region);
		mainLbl.text = _npcSettlement.name;
	}

	private void OnClick()
	{
		if (onClickAction != null)
		{
			onClickAction(_npcSettlement);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick();
	}
}
