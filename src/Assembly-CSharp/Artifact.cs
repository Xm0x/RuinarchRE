using System;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : TileObject
{
	private GameObject _artifactEffectGO;

	public ArtifactData data { get; }

	public ARTIFACT_TYPE type => data.type;

	public override Type serializedData => typeof(SaveDataArtifact);

	public Artifact(ARTIFACT_TYPE type)
	{
		data = ScriptableObjectsManager.Instance.GetArtifactData(type);
		Initialize(TILE_OBJECT_TYPE.ARTIFACT, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public Artifact(SaveDataArtifact data)
		: base(data)
	{
		this.data = ScriptableObjectsManager.Instance.GetArtifactData(data.artifactType);
	}

	public override string ToString()
	{
		return base.name;
	}

	protected override string GenerateInternalName()
	{
		return data.internalName;
	}

	protected override string GenerateDisplayName()
	{
		string text = data.internalName;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", text);
		if (!string.IsNullOrEmpty(localizedValue))
		{
			return localizedValue;
		}
		return text;
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.actions = new List<PLAYER_SKILL_TYPE>();
		AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if ((bool)_artifactEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
			_artifactEffectGO = null;
		}
		_artifactEffectGO = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Artifact);
		if (base.characterOwner != null)
		{
			SetCharacterOwner(null);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		if ((bool)_artifactEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
			_artifactEffectGO = null;
		}
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		if ((bool)_artifactEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
			_artifactEffectGO = null;
		}
	}
}
