namespace Traits;

public class BabyInfestor : Trait
{
	public override bool isSingleton => true;

	public BabyInfestor()
	{
		name = "Baby Infestor";
		description = "Grows and transforms to older Infestor types";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 480;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character character)
		{
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		base.OnRemoveTrait(sourcePOI, removedBy);
		if (!(sourcePOI is Summon summon))
		{
			return;
		}
		if (summon.adultSummonType != SUMMON_TYPE.None)
		{
			Summon summon2 = CharacterManager.Instance.SpawnNewMonsterInstanceFrom(summon.adultSummonType, summon, summon.homeSettlement, summon.homeStructure, summon.gridTileLocation, summon.faction, !summon.isUsingDefaultName);
			summon2.ClearTerritory();
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "become_giant_spider", LOG_TAG.Life_Changes);
			log.AddToFillers(summon2, summon2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			if (summon.HasTerritory())
			{
				summon2.SetTerritory(summon.territory);
			}
			if (UIManager.Instance.IsContextMenuShowingForTarget(summon))
			{
				UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(summon2);
			}
		}
		summon.SetDestroyMarkerOnDeath(state: true);
		summon.SetShowNotificationOnDeath(showNotificationOnDeath: false);
		summon.Death();
		if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == summon)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
	}
}
