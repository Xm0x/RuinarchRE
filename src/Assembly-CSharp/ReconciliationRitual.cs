using Locations.Settlements;

public class ReconciliationRitual : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public ReconciliationRitual()
		: base(INTERACTION_TYPE.RECONCILIATION_RITUAL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Ritual Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			BaseSettlement homeSettlement = node.actor.homeSettlement;
			Character p_character = null;
			Character p_character2 = null;
			if (homeSettlement != null)
			{
				GetTwoCharactersThatAreEnemiesOrRivalInHomeSettlementExcept(homeSettlement, node.actor, ref p_character, ref p_character2);
				if (p_character != null && p_character2 != null)
				{
					node.SetOtherData(new OtherData[2]
					{
						new CharacterOtherData(p_character),
						new CharacterOtherData(p_character2)
					});
				}
				else
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "no_reconciliation_target";
				}
			}
			else
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_reconciliation_target";
			}
		}
		return goapActionInvalidity;
	}

	public void PreRitualSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.otherData != null && goapNode.otherData.Length == 2)
		{
			Character character = goapNode.otherData[0].obj as Character;
			Character character2 = goapNode.otherData[1].obj as Character;
			if (character != null && character2 != null)
			{
				goapNode.descriptionLog.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				goapNode.descriptionLog.AddToFillers(character2, character2.name, LOG_IDENTIFIER.CHARACTER_3);
				goapNode.SetOtherData(new OtherData[2]
				{
					new CharacterOtherData(character),
					new CharacterOtherData(character2)
				});
			}
		}
	}

	public void AfterRitualSuccess(ActualGoapNode goapNode)
	{
		Character character = null;
		Character character2 = null;
		if (goapNode.otherData != null && goapNode.otherData.Length == 2)
		{
			OtherData obj = goapNode.otherData[0];
			OtherData otherData = goapNode.otherData[1];
			if (obj is CharacterOtherData characterOtherData)
			{
				character = characterOtherData.character;
			}
			if (otherData is CharacterOtherData characterOtherData2)
			{
				character2 = characterOtherData2.character;
			}
		}
		if (character != null && character2 != null)
		{
			character.relationshipContainer.AdjustOpinion(character, character2, "Base", 50);
			character2.relationshipContainer.AdjustOpinion(character2, character, "Base", 50);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.name + " success", LOG_TAG.Life_Changes);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.CHARACTER_3);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		else
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.name + " fail", LOG_TAG.Life_Changes);
			log2.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	private void GetTwoCharactersThatAreEnemiesOrRivalInHomeSettlementExcept(BaseSettlement p_homeSettlement, Character p_actor, ref Character p_character1, ref Character p_character2)
	{
		if (p_actor.traitContainer.IsReligiousCultist())
		{
			for (int i = 0; i < p_homeSettlement.residents.Count; i++)
			{
				Character character = p_homeSettlement.residents[i];
				if (character != p_actor && character.isNormalCharacter)
				{
					for (int j = 0; j < character.relationshipContainer.charactersWithOpinion.Count; j++)
					{
						Character character2 = character.relationshipContainer.charactersWithOpinion[j];
						if (character != character2 && character2 != p_actor && character2.isNormalCharacter && character.relationshipContainer.IsEnemiesWith(character2) && character2.relationshipContainer.IsEnemiesWith(character) && !character.isDead && !character2.isDead && (CharacterManager.Instance.IsCultistOfSameReligion(p_actor, character) || CharacterManager.Instance.IsCultistOfSameReligion(p_actor, character2)))
						{
							p_character1 = character;
							p_character2 = character2;
							break;
						}
					}
				}
				if (p_character1 != null && p_character2 != null)
				{
					break;
				}
			}
			return;
		}
		for (int k = 0; k < p_homeSettlement.residents.Count; k++)
		{
			Character character3 = p_homeSettlement.residents[k];
			if (character3 != p_actor && character3.isNormalCharacter)
			{
				for (int l = 0; l < character3.relationshipContainer.charactersWithOpinion.Count; l++)
				{
					Character character4 = character3.relationshipContainer.charactersWithOpinion[l];
					if (character3 != character4 && character4 != p_actor && character4.isNormalCharacter && character3.relationshipContainer.IsEnemiesWith(character4) && character4.relationshipContainer.IsEnemiesWith(character3) && !character3.isDead && !character4.isDead)
					{
						p_character1 = character3;
						p_character2 = character4;
						break;
					}
				}
			}
			if (p_character1 != null && p_character2 != null)
			{
				break;
			}
		}
	}

	public static bool HasTwoCharactersThatAreEnemiesOrRivalInHomeSettlementForReconciliation(BaseSettlement p_homeSettlement, Character p_actor)
	{
		if (p_actor.traitContainer.IsReligiousCultist())
		{
			for (int i = 0; i < p_homeSettlement.residents.Count; i++)
			{
				Character character = p_homeSettlement.residents[i];
				if (character == p_actor || !character.isNormalCharacter)
				{
					continue;
				}
				for (int j = 0; j < character.relationshipContainer.charactersWithOpinion.Count; j++)
				{
					Character character2 = character.relationshipContainer.charactersWithOpinion[j];
					if (character != character2 && character2 != p_actor && character2.isNormalCharacter && character.relationshipContainer.IsEnemiesWith(character2) && character2.relationshipContainer.IsEnemiesWith(character) && !character.isDead && !character2.isDead && (CharacterManager.Instance.IsCultistOfSameReligion(p_actor, character) || CharacterManager.Instance.IsCultistOfSameReligion(p_actor, character2)))
					{
						return true;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < p_homeSettlement.residents.Count; k++)
			{
				Character character3 = p_homeSettlement.residents[k];
				if (character3 == p_actor || !character3.isNormalCharacter)
				{
					continue;
				}
				for (int l = 0; l < character3.relationshipContainer.charactersWithOpinion.Count; l++)
				{
					Character character4 = character3.relationshipContainer.charactersWithOpinion[l];
					if (character3 != character4 && character4 != p_actor && character4.isNormalCharacter && character3.relationshipContainer.IsEnemiesWith(character4) && character4.relationshipContainer.IsEnemiesWith(character3) && !character3.isDead && !character4.isDead)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
