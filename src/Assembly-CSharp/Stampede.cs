using System;
using Inner_Maps;
using UnityEngine;

public class Stampede : MovingTileObject
{
	private StampedeMapObjectVisual _stampedeMapVisual;

	public GameDate expiryDate { get; set; }

	public Vector3 targetDirection { get; set; }

	public float angle { get; private set; }

	public int width { get; private set; }

	public override Type serializedData => typeof(SaveDataStampede);

	public Stampede()
	{
		Initialize(TILE_OBJECT_TYPE.STAMPEDE, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public Stampede(SaveDataStampede data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		base.hasExpired = data.hasExpired;
		targetDirection = data.targetDirection;
		angle = data.angle;
		width = data.width;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_stampedeMapVisual = mapVisual as StampedeMapObjectVisual;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return "Stampede";
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
	}

	public void SetAngle(float p_angle)
	{
		angle = p_angle;
		RotateStampedeByAngle();
	}

	public void SetWidth(int p_width)
	{
		width = p_width;
		_stampedeMapVisual?.UpdateWidth();
	}

	public void RotateStampedeByAngle()
	{
		if (base.mapObjectVisual != null)
		{
			base.mapObjectVisual.Rotate(Quaternion.Euler(0f, 0f, angle - 90f));
		}
	}

	public void StartMovement()
	{
		_stampedeMapVisual?.StartMovement();
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_stampedeMapVisual != null && _stampedeMapVisual.isSpawned)
		{
			tile = _stampedeMapVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		RotateStampedeByAngle();
		SetWidth(width);
		if (_stampedeMapVisual != null)
		{
			_stampedeMapVisual.ScheduleExpiry(expiryDate);
			_stampedeMapVisual.StartMovement();
		}
	}
}
