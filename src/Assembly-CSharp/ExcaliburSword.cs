using System;
using System.Collections.Generic;

public class ExcaliburSword : WeaponItem
{
	private static readonly string[] traitsToBeGainedFromOwnership = new string[3] { "Inspiring", "Robust", "Mighty" };

	private List<string> _traitsGainedByCurrentOwner;

	private string _previousClassOfCurrentOwner;

	public List<string> traitsGainedByCurrentOwner => _traitsGainedByCurrentOwner;

	public string previousClassOfCurrentOwner => _previousClassOfCurrentOwner;

	public override Type serializedData => typeof(SaveDataExcaliburSword);

	public ExcaliburSword()
	{
		Initialize(TILE_OBJECT_TYPE.EXCALIBUR_SWORD);
		base.traitContainer.RemoveTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Treasure");
		_traitsGainedByCurrentOwner = new List<string>();
		_previousClassOfCurrentOwner = string.Empty;
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
	}

	public ExcaliburSword(SaveDataExcaliburSword data)
		: base(data)
	{
		_traitsGainedByCurrentOwner = data.traitsGainedByCurrentOwner;
		_previousClassOfCurrentOwner = data.previousClass;
	}

	public override void SetInventoryOwner(Character p_newOwner)
	{
		Character character = base.isBeingCarriedBy;
		base.SetInventoryOwner(p_newOwner);
		if (character == p_newOwner)
		{
			return;
		}
		if (character != null)
		{
			TryRevertClassOfOwner(character);
			for (int i = 0; i < _traitsGainedByCurrentOwner.Count; i++)
			{
				string traitName = _traitsGainedByCurrentOwner[i];
				character.traitContainer.RemoveTrait(character, traitName);
			}
			SetCharacterOwner(null);
		}
		_previousClassOfCurrentOwner = string.Empty;
		_traitsGainedByCurrentOwner.Clear();
		if (p_newOwner == null)
		{
			return;
		}
		SetCharacterOwner(p_newOwner);
		if (CanBecomeHero(p_newOwner))
		{
			_previousClassOfCurrentOwner = p_newOwner.characterClass.className;
			p_newOwner.classComponent.AssignClass("Hero");
		}
		for (int j = 0; j < traitsToBeGainedFromOwnership.Length; j++)
		{
			string text = traitsToBeGainedFromOwnership[j];
			if (p_newOwner.traitContainer.AddTrait(p_newOwner, text))
			{
				_traitsGainedByCurrentOwner.Add(text);
			}
		}
		p_newOwner.combatComponent.UpdateMaxHPAndReset();
	}

	private void TryRevertClassOfOwner(Character p_owner)
	{
		if (p_owner.characterClass.className == "Hero")
		{
			p_owner.classComponent.AssignClass(previousClassOfCurrentOwner);
		}
		else if (p_owner.characterClass.className == "Werewolf" && p_owner.classComponent.previousClassName == "Hero")
		{
			p_owner.classComponent.OverridePreviousClassName(previousClassOfCurrentOwner);
		}
	}

	private bool CanBecomeHero(Character p_character)
	{
		if (!p_character.traitContainer.IsBlessed())
		{
			return false;
		}
		if (p_character.traitContainer.HasTrait("Evil", "Treacherous", "Demon Cultist"))
		{
			return false;
		}
		if (p_character.characterClass.className == "Werewolf" || p_character.characterClass.className == "Necromancer" || p_character.characterClass.className == "Vampire Lord")
		{
			return false;
		}
		if (!p_character.isNormalCharacter)
		{
			return false;
		}
		return true;
	}
}
