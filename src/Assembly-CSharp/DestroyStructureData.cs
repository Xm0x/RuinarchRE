using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;

public class DestroyStructureData : PlayerAction
{
	private LocationStructure m_targetStructure;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_STRUCTURE;

	public override string name => "Destroy";

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Destroy Structure");

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Destroy Structure_Description") ?? "";

	public override string description => "This Ability can be used to destroy Demonic Structure.";

	public DestroyStructureData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		m_targetStructure = structure;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Destroy_Structure");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("structureName", structure.name);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Destroy_Structure_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnActualDestroyStructure, null, showCover: true, 150);
	}

	private void OnActualDestroyStructure()
	{
		m_targetStructure.AdjustHP(-m_targetStructure.currentHP, null, isPlayerSource: true);
		base.ActivateAbility(m_targetStructure);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Destroy Structure activated", LOG_TAG.Player);
		log.AddToFillers(m_targetStructure, m_targetStructure.GetNameRelativeTo(null), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public override bool CanPerformAbilityTowards(LocationStructure structure)
	{
		bool flag = base.CanPerformAbilityTowards(structure);
		if (flag)
		{
			if (structure.structureType.IsSpecialStructure() && structure.residents.Count > 0)
			{
				return false;
			}
			if (!structure.hasBeenDestroyed && structure.tiles.Count > 0)
			{
				return structure.currentHP > 0;
			}
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
		if (p_targetStructure.structureType.IsSpecialStructure() && p_targetStructure.residents.Count > 0)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Destroy_Occupied") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		LocationStructure locationStructure = target as LocationStructure;
		if (locationStructure is DemonicStructure { structureType: STRUCTURE_TYPE.THE_PORTAL })
		{
			return false;
		}
		if (locationStructure.hasBeenDestroyed || locationStructure.tiles.Count <= 0)
		{
			return false;
		}
		return base.IsValid(target);
	}
}
