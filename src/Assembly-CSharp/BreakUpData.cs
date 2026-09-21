using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

public class BreakUpData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BREAK_UP;

	public override string name => "Break Up";

	public override string description => "Force a Villager to break up with his/her lover.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public BreakUpData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character targetCharacter = targetPOI as Character;
		if (targetCharacter == null)
		{
			return;
		}
		if (targetCharacter.relationshipContainer.GetAliveOrUnspawnedRelatablesWithRelationshipCount(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR) > 1)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			foreach (KeyValuePair<int, IRelationshipData> relationship in targetCharacter.relationshipContainer.relationships)
			{
				if (relationship.Value.IsLoverOrAffair())
				{
					Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
					if (characterByID != null)
					{
						list.Add(characterByID);
					}
				}
			}
			UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
			{
				OnChooseCharacter(o, targetCharacter);
			}, null, (Character t) => CanBeBrokenUp(targetCharacter, t), "", null, null, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list);
		}
		else
		{
			Character firstCharacterWithRelationship = targetCharacter.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
			UIManager.Instance.ShowSchemeUI(targetCharacter, firstCharacterWithRelationship, this);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (!targetCharacter.isNormalCharacter || !targetCharacter.relationshipContainer.HasRelationshipWithSpawnedCharacter(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
			{
				return false;
			}
			return true;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.isNormalCharacter)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Not_Villager") + "|";
		}
		if (!targetCharacter.relationshipContainer.HasRelationshipWithSpawnedCharacter(RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_No_Lover_Or_Affair") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		if (target is Character targetPOI)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetPOI);
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (target is Character character2)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Break_Up_Message");
			log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
			conversationList.Add(item);
		}
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		if (p_targetCharacter.traitContainer.HasTrait("Unfaithful"))
		{
			p_newSuccessRate *= 2f;
		}
		base.ProcessSuccessRateWithMultipliers(p_targetCharacter, p_otherTarget, ref p_newSuccessRate);
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		if (p_targetCharacter.traitContainer.HasTrait("Unfaithful"))
		{
			return p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + TraitManager.Instance.GetLocalizedNameOfTrait("Unfaithful") + ": <color=white>x2</color>";
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}

	private bool CanBeBrokenUp(Character source, Character target)
	{
		return true;
	}

	private void OnChooseCharacter(object obj, Character source)
	{
		if (obj is Character p_otherTarget)
		{
			UIManager.Instance.HideObjectPicker();
			UIManager.Instance.ShowSchemeUI(source, p_otherTarget, this);
		}
	}
}
