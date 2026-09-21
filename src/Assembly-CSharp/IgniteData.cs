using System.Collections.Generic;
using Inner_Maps;
using Object_Pools;
using Traits;
using UtilityScripts;

public class IgniteData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IGNITE;

	public override string name => "Ignite";

	public override string description => "This Ability applies Burning to an object.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public IgniteData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		LocationGridTile gridTileLocation = targetPOI.gridTileLocation;
		if (gridTileLocation != null)
		{
			BurningSource bs = null;
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			gridTileLocation.PopulateTraitablesOnTile(list);
			for (int i = 0; i < list.Count; i++)
			{
				ITraitable traitable = list[i];
				IgniteEffect(traitable, ref bs);
			}
			RuinarchListPool<ITraitable>.Release(list);
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", name + " activated", LOG_TAG.Player);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if (tileObject.gridTileLocation == null)
		{
			return false;
		}
		if (!tileObject.traitContainer.HasTrait("Flammable"))
		{
			return false;
		}
		if (tileObject.traitContainer.HasTrait("Burnt", "Burning", "Wet", "Fire Resistant"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(tileObject);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		if (targetTileObject.gridTileLocation == null)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Ignite_Not_On_Floor") + "|";
		}
		string value = string.Empty;
		if (targetTileObject.traitContainer.HasTrait("Burning"))
		{
			value = TraitManager.Instance.GetLocalizedNameOfTrait("Burning");
		}
		else if (targetTileObject.traitContainer.HasTrait("Wet"))
		{
			value = TraitManager.Instance.GetLocalizedNameOfTrait("Wet");
		}
		else if (targetTileObject.traitContainer.HasTrait("Burnt"))
		{
			value = TraitManager.Instance.GetLocalizedNameOfTrait("Burnt");
		}
		else if (targetTileObject.traitContainer.HasTrait("Fire Resistant"))
		{
			value = TraitManager.Instance.GetLocalizedNameOfTrait("Fire Resistant");
		}
		if (!string.IsNullOrEmpty(value))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowerReasons_Table", "Ignite_Immune");
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
			text = text + log.logText + "|";
			LogPool.Release(log);
		}
		return text;
	}

	private void IgniteEffect(ITraitable traitable, ref BurningSource bs)
	{
		if (traitable.gridTileLocation == null)
		{
			return;
		}
		Trait trait = null;
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.IGNITE);
		if (traitable.traitContainer.AddTrait(traitable, "Burning", out trait, null, bypassElementalChance: true, durationBonusPerLevel, 0f, ELEMENTAL_TYPE.Fire))
		{
			Burning traitOrStatus = traitable.traitContainer.GetTraitOrStatus<Burning>("Burning");
			if (traitOrStatus != null)
			{
				traitOrStatus.SetIsPlayerSource(p_state: true);
				TraitManager.Instance.ProcessBurningTrait(traitable, trait, ref bs);
			}
		}
	}
}
