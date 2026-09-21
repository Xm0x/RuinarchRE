using System.Collections.Generic;
using Maccima_Games.Util;
using UtilityScripts;

public class CultistJoinFactionData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION;

	public override string name => "Join Faction";

	public override string description => "This Action instructs the character to join a target Faction. This is a special action available only on Cultists.";

	public override bool canBeCastOnBlessed => true;

	public CultistJoinFactionData()
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
		_ = sourceCharacter.faction;
		List<Faction> list = RuinarchListPool<Faction>.Claim();
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.HasMemberThatIsNotDead() && !faction.isDisbanded && faction.isMajorNonPlayer)
			{
				list.Add(faction);
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

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character && !character.traitContainer.HasTrait("Demon Cultist"))
		{
			return false;
		}
		return base.IsValid(target);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.traitContainer.HasTrait("Enslaved"))
			{
				return false;
			}
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Enslaved"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Slave_Cannot_Target") + "|";
		}
		return text;
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
		if (target == source.faction)
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
		if (target == source.faction)
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			Dictionary<string, string> dictionary3 = MaccimaDictionaryPool<string, string>.Claim();
			dictionary3.Add("actorName", source.name);
			string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Cultist_Join_Faction_Current_Faction", dictionary3);
			MaccimaDictionaryPool<string, string>.Release(dictionary3);
			text += Utilities.ColorizeInvalidText(localizedValue3);
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
		base.ActivateAbility((IPointOfInterest)source);
		if (obj is Faction faction)
		{
			Character targetPOI = faction.characters[0];
			source.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, targetPOI, "join_faction_normal");
		}
	}
}
