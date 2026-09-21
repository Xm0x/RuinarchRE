using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class Watcher : DemonicStructure
{
	private int m_eyesLevel;

	private int m_radiusLevel;

	private int m_eyeWardMaxCount;

	private int m_eyeWardRadius;

	public List<DemonEye> eyeWards { get; private set; }

	public override Type serializedData => typeof(SaveDataWatcher);

	public Watcher(Region location)
		: base(STRUCTURE_TYPE.WATCHER, location)
	{
		SetMaxHPAndReset(2500);
		eyeWards = new List<DemonEye>();
		m_eyeWardMaxCount = 3;
		m_eyeWardRadius = 9;
	}

	public Watcher(Region location, SaveDataWatcher data)
		: base(location, data)
	{
		eyeWards = new List<DemonEye>();
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		Messenger.RemoveListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
		for (int i = 0; i < eyeWards.Count; i++)
		{
			DemonEye demonEye = eyeWards[i];
			if (demonEye.gridTileLocation != null)
			{
				demonEye.gridTileLocation.structure.RemovePOI(demonEye);
			}
			if (RemoveEyeWard(demonEye))
			{
				i--;
			}
		}
		if (PlayerManager.Instance.player.currentActivePlayerSpell is SpawnEyeWardData spawnEyeWardData && spawnEyeWardData.watcherParentOfEye == this)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		position.y -= 0.5f;
		worldPosition = position;
	}

	protected override string GetExtraInfo1Header()
	{
		return LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", "Watcher_Header_1") + (m_eyesLevel + 1);
	}

	protected override string GetExtraInfo1Description()
	{
		return eyeWards.Count + "/" + m_eyeWardMaxCount;
	}

	protected override string GetExtraInfo2Header()
	{
		return LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", "Watcher_Header_2") + (m_radiusLevel + 1);
	}

	protected override string GetExtraInfo2Description()
	{
		return m_eyeWardRadius * 2 + "x" + m_eyeWardRadius * 2;
	}

	private void OnDestroyTileObject(TileObject p_tileObject)
	{
		if (p_tileObject is DemonEye p_eyeWard)
		{
			RemoveEyeWard(p_eyeWard);
		}
	}

	public void AddEyeWard(DemonEye p_eyeWard)
	{
		if (!eyeWards.Contains(p_eyeWard))
		{
			eyeWards.Add(p_eyeWard);
			p_eyeWard.SetBeholderOwner(this);
			Messenger.Broadcast(StructureSignals.UPDATE_EYE_WARDS, this);
		}
	}

	public bool RemoveEyeWard(DemonEye p_eyeWard)
	{
		if (eyeWards.Remove(p_eyeWard))
		{
			p_eyeWard.SetBeholderOwner(null);
			Messenger.Broadcast(StructureSignals.UPDATE_EYE_WARDS, this);
			return true;
		}
		return false;
	}

	public void LevelUpEyes()
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-EditableValuesManager.Instance.GetBeholderEyeUpgradeCostPerLevel(m_eyesLevel).processedAmount);
		m_eyesLevel = Mathf.Clamp(++m_eyesLevel, 1, 4);
		m_eyeWardMaxCount = Mathf.Clamp(++m_eyeWardMaxCount, 1, 8);
	}

	public void LevelUpRadius()
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_radiusLevel).processedAmount);
		m_radiusLevel = Mathf.Clamp(++m_radiusLevel, 1, 4);
		m_eyeWardRadius += 3;
		m_eyeWardRadius = Mathf.Clamp(m_eyeWardRadius, 9, 18);
		eyeWards.ForEach(delegate(DemonEye eachEye)
		{
			eachEye.UpdateRange();
		});
	}

	public void OverrideEyeRadius(int p_value)
	{
		m_eyeWardRadius = p_value;
		eyeWards.ForEach(delegate(DemonEye eachEye)
		{
			eachEye.UpdateRange();
		});
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataWatcher saveDataWatcher = saveDataLocationStructure as SaveDataWatcher;
		if (saveDataWatcher.eyeWards != null)
		{
			for (int i = 0; i < saveDataWatcher.eyeWards.Count; i++)
			{
				if (!string.IsNullOrEmpty(saveDataWatcher.eyeWards[i]))
				{
					eyeWards.Add(DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataWatcher.eyeWards[i]) as DemonEye);
					eyeWards[i].SetBeholderOwner(this);
					eyeWards[i].UpdateRange();
				}
			}
		}
		m_eyesLevel = saveDataWatcher.eyesLevel;
		m_radiusLevel = saveDataWatcher.radiusLevel;
		m_eyeWardMaxCount = saveDataWatcher.eyeWardMaxCount;
		m_eyeWardRadius = saveDataWatcher.eyeWardRadius;
		Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
	}

	public int GetEyeLevel()
	{
		return m_eyesLevel;
	}

	public int GetRadiusLevel()
	{
		return m_radiusLevel;
	}

	public int GetEyeWardRadius()
	{
		return m_eyeWardRadius;
	}

	public int GetCurrentMaxEyeCount()
	{
		return m_eyeWardMaxCount;
	}
}
