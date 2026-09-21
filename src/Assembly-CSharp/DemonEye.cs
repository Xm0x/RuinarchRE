using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class DemonEye : TileObject
{
	private List<LocationGridTile> tilesInRadius;

	private EyeWardHighlight _eyeWardHighlight;

	private Watcher m_owner;

	public DemonEye()
	{
		tilesInRadius = new List<LocationGridTile>();
		Initialize(TILE_OBJECT_TYPE.DEMON_EYE, shouldAddCommonAdvertisements: false);
		AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Indestructible");
		base.hiddenComponent.SetIsHidden(this, state: true, affectAlpha: false);
		PlayerManager.Instance.player.tileObjectComponent.AddEyeWard(this);
	}

	public DemonEye(SaveDataTileObject data)
		: base(data)
	{
		tilesInRadius = new List<LocationGridTile>();
	}

	public void UpdateRange()
	{
		if (gridTileLocation != null && base.previousTile != gridTileLocation)
		{
			tilesInRadius.Clear();
			gridTileLocation.PopulateTilesInRadius(tilesInRadius, m_owner.GetEyeWardRadius(), 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < tilesInRadius.Count; i++)
			{
				tilesInRadius[i].tileObjectComponent.SetIsSeenByEyeWard(state: true);
			}
		}
		else if (base.previousTile != null && gridTileLocation == null)
		{
			for (int j = 0; j < tilesInRadius.Count; j++)
			{
				tilesInRadius[j].tileObjectComponent.SetIsSeenByEyeWard(state: false);
			}
		}
	}

	protected override void OnSetGridTileLocation()
	{
		base.OnSetGridTileLocation();
		UpdateRange();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (PlayerManager.Instance.player.IsCurrentActiveSpell(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD))
		{
			ShowEyeWardHighlight();
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		PlayerManager.Instance.player.tileObjectComponent.RemoveEyeWard(this);
		if (_eyeWardHighlight != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_eyeWardHighlight.gameObject);
			_eyeWardHighlight = null;
		}
		PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD).AdjustCharges(1);
	}

	public void ReduceHPBypassEverything(int amount)
	{
		if (base.currentHP == 0 && amount < 0)
		{
			return;
		}
		if (Mathf.Abs(amount) > base.currentHP)
		{
			amount = -base.currentHP;
		}
		base.currentHP -= amount;
		base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		if ((bool)mapVisual)
		{
			if (mapVisual.hasHPBarGO && mapVisual.hpBarGO.activeSelf)
			{
				mapVisual.UpdateHP(this);
			}
			else if (base.currentHP > 0)
			{
				mapVisual.QuickShowHPBar(this);
			}
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (base.currentHP <= 0)
		{
			locationGridTile?.structure.RemovePOI(this);
		}
	}

	public void SetBeholderOwner(Watcher p_beholder)
	{
		m_owner = p_beholder;
	}

	public Watcher GetBeholderOwner()
	{
		return m_owner;
	}

	public void ShowEyeWardHighlight()
	{
		if (_eyeWardHighlight == null)
		{
			GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Eye_Ward_Highlight, allowRotation: false);
			if ((bool)gameObject)
			{
				_eyeWardHighlight = gameObject.GetComponent<EyeWardHighlight>();
			}
		}
		if (_eyeWardHighlight != null)
		{
			_eyeWardHighlight.HideHighlight();
			_eyeWardHighlight.SetupHighlight(m_owner.GetEyeWardRadius());
			_eyeWardHighlight.ShowHighlight();
		}
	}

	public void HideEyeWardHighlight()
	{
		if (_eyeWardHighlight != null)
		{
			_eyeWardHighlight.HideHighlight();
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_EYE_WARD);
		PlayerManager.Instance.player.tileObjectComponent.AddEyeWard(this);
	}
}
