using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

public class InstigateWarData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INSTIGATE_WAR;

	public override string name => "Instigate War";

	public override string description => "Force a Faction Leader to declare war on another faction.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public InstigateWarData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character sourceCharacter = targetPOI as Character;
		if (sourceCharacter == null)
		{
			return;
		}
		Faction sourceFaction = sourceCharacter.faction;
		List<Faction> list = RuinarchListPool<Faction>.Claim(10);
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction != sourceFaction && faction.factionType.type != FACTION_TYPE.Vagrants && faction.factionType.type != FACTION_TYPE.Demons && faction.factionType.type != FACTION_TYPE.Wild_Monsters && faction.factionType.type != FACTION_TYPE.Retaliator && faction.HasMemberThatIsNotDead())
			{
				list.Add(faction);
			}
		}
		UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
		{
			OnChooseFaction(o, sourceCharacter);
		}, null, (Faction t) => CanInstigateWar(sourceFaction, t), "", delegate(Faction t)
		{
			OnHoverEnter(sourceFaction, t);
		}, OnHoverExit, "", showCover: true, 25);
		RuinarchListPool<Faction>.Release(list);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (!targetCharacter.isFactionLeader)
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
		if (!targetCharacter.isFactionLeader)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_Faction_Leader") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		if (target is Faction faction)
		{
			Character targetPOI = faction.characters[0];
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_War, targetPOI);
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (target is Faction faction)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Instigate_War_Message");
			log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			string logText = log.logText;
			LogPool.Release(log);
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
			conversationList.Add(item);
		}
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful))
		{
			p_newSuccessRate *= 0.25f;
		}
		else if (p_targetCharacter.traitContainer.HasTrait("Diplomatic"))
		{
			p_newSuccessRate *= 0.5f;
		}
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
		{
			p_newSuccessRate *= 3f;
		}
		base.ProcessSuccessRateWithMultipliers(p_targetCharacter, p_otherTarget, ref p_newSuccessRate);
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		string text = string.Empty;
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.faction.nameWithColor + " - " + LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", "Peaceful") + ": <color=white>x0.25</color>";
		}
		else if (p_targetCharacter.traitContainer.HasTrait("Diplomatic"))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + TraitManager.Instance.GetLocalizedNameOfTrait("Diplomatic") + ": <color=white>x0.5</color>";
		}
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.faction.nameWithColor + " - " + LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", "Warmonger") + ": <color=white>x3</color>";
		}
		if (text != string.Empty)
		{
			return text;
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}

	private bool CanInstigateWar(Faction source, Faction target)
	{
		if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target))
		{
			return false;
		}
		return true;
	}

	private void OnHoverEnter(Faction source, Faction target)
	{
		if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target))
		{
			UIManager.Instance.ShowSmallInfo(Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Already_War")));
		}
	}

	private void OnHoverExit(Faction source)
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnChooseFaction(object obj, Character source)
	{
		if (obj is Faction p_otherTarget)
		{
			UIManager.Instance.HideObjectPicker();
			UIManager.Instance.ShowSchemeUI(source, p_otherTarget, this);
		}
	}
}
