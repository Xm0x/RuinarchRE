using System;

namespace Factions.Faction_Types;

public abstract class CultFaction : FactionType
{
	public abstract RELIGION cultReligion { get; }

	public string classNameOfLeaderBeforeBecomingCultLeader { get; private set; }

	public override Type serializedData => typeof(SaveDataCultFaction);

	public override bool usesBothWoodAndStoneResources => true;

	public CultFaction(FACTION_TYPE factionType)
		: base(factionType)
	{
	}

	public CultFaction(SaveDataFactionType saveData, FACTION_TYPE factionType)
		: base(factionType, saveData)
	{
		SaveDataCultFaction saveDataCultFaction = saveData as SaveDataCultFaction;
		classNameOfLeaderBeforeBecomingCultLeader = saveDataCultFaction.classNameOfLeaderBeforeBecomingCultLeader;
	}

	public override void ProcessNewMember(Character character)
	{
		if (ShouldChangeReligionOnJoinCult(character))
		{
			character.religionComponent.ChangeReligion(cultReligion);
		}
	}

	protected bool ShouldChangeReligionOnJoinCult(Character p_character)
	{
		if (p_character.classComponent.IsStalkerCannotBeTurned())
		{
			return false;
		}
		if ((p_character.isNormalCharacter || (p_character.raceSetting.category != CHARACTER_CATEGORY.Beast && p_character.raceSetting.category != CHARACTER_CATEGORY.Undead)) && !p_character.traitContainer.HasTrait(cultReligion.GetCultistTraitNameForReligion()))
		{
			return true;
		}
		return false;
	}

	public override void ProcessOnFactionLeaderChanged(ILeader p_previousLeader, ILeader p_newLeader)
	{
		string cultLeaderClassNameForReligion = cultReligion.GetCultLeaderClassNameForReligion();
		if (p_previousLeader is Character character && character.characterClass.className == cultLeaderClassNameForReligion)
		{
			character.classComponent.AssignClass(classNameOfLeaderBeforeBecomingCultLeader);
			classNameOfLeaderBeforeBecomingCultLeader = string.Empty;
		}
		if (p_newLeader is Character character2)
		{
			classNameOfLeaderBeforeBecomingCultLeader = character2.characterClass.className;
			character2.classComponent.AssignClass(cultLeaderClassNameForReligion);
			Messenger.Broadcast(FactionSignals.CHARACTER_BECAME_RELIGIOUS_CULT_LEADER, character2);
		}
	}
}
