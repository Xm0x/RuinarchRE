using Inner_Maps.Location_Structures;
using UnityEngine;

public class ManaRegenComponent
{
	private Player m_player;

	private int m_manaPitCount;

	private int m_maxMana;

	public GameDate nextManaRegenDate { get; private set; }

	public ManaRegenComponent(Player p_player)
	{
		m_player = p_player;
		m_maxMana = EditableValuesManager.Instance.maximumMana;
		SubscribeListeners();
		ScheduleNextManaRegen();
	}

	public ManaRegenComponent(Player p_player, SaveDataManaRegenComponent data)
	{
		m_player = p_player;
		m_manaPitCount = data.manaPitCount;
		m_maxMana = data.maxMana;
		EditableValuesManager.Instance.maximumMana = m_maxMana;
	}

	public void LoadReferencesMainThread(SaveDataManaRegenComponent data)
	{
		SubscribeListeners();
		if (data.nextManaRegen.hasValue)
		{
			LoadNextManaRegen(data.nextManaRegen);
		}
		else
		{
			ScheduleNextManaRegen();
		}
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
		Messenger.AddListener<LocationStructure, Area>(StructureSignals.STRUCTURE_OBJECT_REMOVED, OnStructureDestroyed);
	}

	private void OnStructurePlaced(LocationStructure p_structure)
	{
		if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT)
		{
			EditableValuesManager.Instance.maximumMana += EditableValuesManager.Instance.GetAdditionalMaxManaPerManaPit();
			m_maxMana = EditableValuesManager.Instance.maximumMana;
			m_manaPitCount++;
		}
	}

	private void OnStructureDestroyed(LocationStructure p_structure, Area p_area)
	{
		if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT)
		{
			EditableValuesManager.Instance.maximumMana -= EditableValuesManager.Instance.GetAdditionalMaxManaPerManaPit();
			m_maxMana = EditableValuesManager.Instance.maximumMana;
			if (m_player.currenciesComponent.mana > EditableValuesManager.Instance.maximumMana)
			{
				m_player.currenciesComponent.AdjustMana(EditableValuesManager.Instance.maximumMana - m_player.currenciesComponent.mana);
			}
			m_manaPitCount--;
		}
	}

	public int GetManaPitCount()
	{
		return m_manaPitCount;
	}

	public int GetMaxMana()
	{
		return m_maxMana;
	}

	public int GetManaRegenPerHour()
	{
		return EditableValuesManager.Instance.GetManaRegenPerHour() + EditableValuesManager.Instance.GetManaRegenPerManaPit() * m_manaPitCount;
	}

	private void LoadNextManaRegen(GameDate p_date)
	{
		nextManaRegenDate = p_date;
		SchedulingManager.Instance.AddEntry(nextManaRegenDate, RegenerateMana, this);
	}

	private void ScheduleNextManaRegen()
	{
		nextManaRegenDate = GameManager.Instance.Today();
		nextManaRegenDate = nextManaRegenDate.AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30));
		SchedulingManager.Instance.AddEntry(nextManaRegenDate, RegenerateMana, this);
	}

	private void RegenerateMana()
	{
		if (m_player.currenciesComponent.mana < EditableValuesManager.Instance.maximumMana)
		{
			int amount = Mathf.FloorToInt((float)GetManaRegenPerHour() * 0.5f);
			m_player.currenciesComponent.AdjustMana(amount);
		}
		ScheduleNextManaRegen();
	}
}
