using Inner_Maps.Location_Structures;
using Object_Pools;

public class UpgradeBeholderRadiusLevelData : PlayerAction
{
	private Watcher m_targetBeholder;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL;

	public override string name => "Increase Radius";

	public override string description => GetDescription();

	public override string localizedDescription => GetDescription();

	public string GetDescription()
	{
		if (m_targetBeholder != null)
		{
			if (m_targetBeholder.GetRadiusLevel() >= 3)
			{
				return LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Upgrade_Beholder_Radius_Level_Max");
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Powers", "PlayerActions_Table", "Upgrade_Beholder_Radius_Level");
			log.AddToFillers(null, EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).GetCostStringWithIcon(), LOG_IDENTIFIER.STRING_1);
			string logText = log.logText;
			LogPool.Release(log);
			return logText;
		}
		return LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Upgrade_Beholder_Radius_Level_Max");
	}

	public override bool CanPerformAbilityTowards(LocationStructure target)
	{
		bool flag = false;
		m_targetBeholder = target as Watcher;
		flag = m_targetBeholder.GetRadiusLevel() < 3;
		flag = ((m_targetBeholder.GetRadiusLevel() < 3 && PlayerManager.Instance.player.chaoticEnergy >= EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).processedAmount && flag) ? true : false);
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Watcher)
		{
			return true;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure)
	{
		string reasonsWhyCannotPerformAbilityTowards = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
		m_targetBeholder = structure as Watcher;
		if (m_targetBeholder.GetRadiusLevel() >= 3)
		{
			return reasonsWhyCannotPerformAbilityTowards + GetLocalizedReasonWhyCannotPerformAbilityTowards("Watcher_Radius_Max_Level") + "|";
		}
		if (PlayerManager.Instance.player.chaoticEnergy < EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_targetBeholder.GetRadiusLevel()).processedAmount)
		{
			return reasonsWhyCannotPerformAbilityTowards + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_Enough_Chaotic_Energy") + "|";
		}
		return reasonsWhyCannotPerformAbilityTowards;
	}

	public UpgradeBeholderRadiusLevelData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		(structure as Watcher).LevelUpRadius();
		base.ActivateAbility(structure);
	}
}
