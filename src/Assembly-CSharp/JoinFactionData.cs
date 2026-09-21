using System.Collections.Generic;
using Maccima_Games.Util;
using Object_Pools;
using UtilityScripts;

public class JoinFactionData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.JOIN_FACTION;

	public override string name => "Join Faction";

	public override string description => "This Ability instructs the character to join a target Faction if possible.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public JoinFactionData()
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
		Faction faction = sourceCharacter.faction;
		List<Faction> list = RuinarchListPool<Faction>.Claim();
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction2 = FactionManager.Instance.allFactions[i];
			if (faction2 != faction && faction2.isMajorNonPlayer && faction2.HasMemberThatIsNotDead() && !faction2.isDisbanded)
			{
				list.Add(faction2);
			}
		}
		UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
		{
			OnChooseFaction(o, sourceCharacter);
		}, null, (Faction t) => CanJoinFaction(sourceCharacter, t), "", delegate(Faction t)
		{
			OnHoverEnter(sourceCharacter, t);
		}, OnHoverExit, "", showCover: true, 25);
		RuinarchListPool<Faction>.Release(list);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (!targetCharacter.isVagrant)
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
		if (!targetCharacter.isVagrant)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_Vagrant") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		if (target is Faction faction)
		{
			Character targetPOI = faction.characters[0];
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, targetPOI, "join_faction_normal");
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (target is Faction faction)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Join_Faction_Message");
			log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			string logText = log.logText;
			LogPool.Release(log);
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
			conversationList.Add(item);
		}
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	private bool CanJoinFaction(Character source, Faction target)
	{
		if (!target.ideologyComponent.DoesCharacterFitCurrentIdeologies(source))
		{
			return false;
		}
		if (target.IsCharacterBannedFromJoining(source))
		{
			return false;
		}
		return true;
	}

	private void OnHoverEnter(Character source, Faction target)
	{
		string text = string.Empty;
		if (!target.ideologyComponent.DoesCharacterFitCurrentIdeologies(source))
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("actorName", source.name);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Cultist_Join_Faction_Ideology_Incompatible", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			text += Utilities.ColorizeInvalidText(localizedValue);
		}
		if (target.IsCharacterBannedFromJoining(source))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			Dictionary<string, string> dictionary2 = MaccimaDictionaryPool<string, string>.Claim();
			dictionary2.Add("actorName", source.name);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Cultist_Join_Faction_Banned", dictionary2);
			MaccimaDictionaryPool<string, string>.Release(dictionary2);
			text += Utilities.ColorizeInvalidText(localizedValue2);
		}
		if (text != string.Empty)
		{
			UIManager.Instance.ShowSmallInfo(text);
		}
	}

	private void OnHoverExit(Faction source)
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnChooseFaction(object obj, Character source)
	{
		UIManager.Instance.HideObjectPicker();
		if (obj is Faction p_otherTarget)
		{
			UIManager.Instance.ShowSchemeUI(source, p_otherTarget, this);
		}
	}
}
