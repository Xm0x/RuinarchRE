using Traits;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class CultistsListUI : PopupMenuBase
{
	[SerializeField]
	private ScrollRect listScrollRect;

	[SerializeField]
	private GameObject listItemPrefab;

	[SerializeField]
	private Toggle cultistsToggle;

	[SerializeField]
	private UIHoverPosition tooltipPos;

	private void Awake()
	{
		Close();
	}

	public void Initialize()
	{
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterRemovedTrait);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
	}

	public void UpdateList()
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.traitContainer.HasTrait("Demon Cultist") && character.isNormalCharacter && character.race != RACE.RATMAN)
			{
				CreateNewItemFor(character);
			}
		}
	}

	public void ToggleList(bool isOn)
	{
		if (isOn)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	public override void Close()
	{
		base.Close();
		cultistsToggle.SetIsOnWithoutNotify(value: false);
	}

	public override void Open()
	{
		base.Open();
		cultistsToggle.SetIsOnWithoutNotify(value: true);
	}

	protected override void OnGameObjectEnabled()
	{
		base.OnGameObjectEnabled();
		Messenger.Broadcast(UISignals.TOP_UI_ENABLED);
	}

	protected override void OnGameObjectDisabled()
	{
		base.OnGameObjectDisabled();
		Messenger.Broadcast(UISignals.TOP_UI_DISABLED);
	}

	private void OnCharacterGainedTrait(Character character, Trait trait)
	{
		if (trait is DemonCultist && character.isNormalCharacter && character.race != RACE.RATMAN)
		{
			CreateNewItemFor(character);
		}
	}

	private void OnCharacterRemovedTrait(Character character, Trait trait)
	{
		if (trait is DemonCultist)
		{
			RemoveItemOf(character);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Demon Cultist"))
		{
			CharacterPortrait characterItem = GetCharacterItem(p_character);
			if (characterItem != null)
			{
				characterItem.transform.SetAsLastSibling();
			}
		}
	}

	private void OnCharacterMarkerDestroyed(Character p_character)
	{
		RemoveItemOf(p_character);
	}

	private void CreateNewItemFor(Character character)
	{
		CharacterPortrait component = ObjectPoolManager.Instance.InstantiateObjectFromPool(listItemPrefab.name, Vector3.zero, Quaternion.identity, listScrollRect.content).GetComponent<CharacterPortrait>();
		component.GeneratePortrait(character);
		component.SetHoverActions(OnHoverOverPortrait, OnHoverOutPortrait);
		component.transform.SetAsFirstSibling();
	}

	private void RemoveItemOf(Character character)
	{
		CharacterPortrait characterItem = GetCharacterItem(character);
		if (characterItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(characterItem);
		}
	}

	private CharacterPortrait GetCharacterItem(Character p_character)
	{
		CharacterPortrait[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<CharacterPortrait>(listScrollRect.content.gameObject);
		foreach (CharacterPortrait characterPortrait in componentsInDirectChildren)
		{
			if (characterPortrait.character == p_character)
			{
				return characterPortrait;
			}
		}
		return null;
	}

	private void OnHoverOverPortrait(CharacterPortrait p_portrait)
	{
		if (p_portrait.character != null)
		{
			UIManager.Instance.ShowCharacterNameplateTooltip(p_portrait.character, tooltipPos);
		}
	}

	private void OnHoverOutPortrait(CharacterPortrait p_portrait)
	{
		if (p_portrait.character != null)
		{
			UIManager.Instance.HideCharacterNameplateTooltip();
		}
	}
}
