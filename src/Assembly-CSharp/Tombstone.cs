using System;
using UnityEngine;

public class Tombstone : TileObject
{
	private bool _respawnCorpseOnDestroy;

	private Character[] _users;

	public override Vector2 selectableSize => new Vector2(1f, 0.8f);

	public override Vector3 worldPosition
	{
		get
		{
			Vector3 position = mapVisual.transform.position;
			position.y += 0.15f;
			return position;
		}
	}

	public override Character[] users
	{
		get
		{
			_users[0] = character;
			return _users;
		}
	}

	public Character character { get; private set; }

	public override Type serializedData => typeof(SaveDataTombstone);

	public Tombstone()
	{
		AddAdvertisedAction(INTERACTION_TYPE.REMEMBER_FALLEN);
		AddAdvertisedAction(INTERACTION_TYPE.SPIT);
		AddAdvertisedAction(INTERACTION_TYPE.RAISE_CORPSE);
		AddAdvertisedAction(INTERACTION_TYPE.CARRY_CORPSE);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_CORPSE);
		AddAdvertisedAction(INTERACTION_TYPE.GO_TO);
		_users = new Character[1];
		_respawnCorpseOnDestroy = true;
	}

	public Tombstone(SaveDataTombstone data)
		: base(data)
	{
		_users = new Character[1];
		_respawnCorpseOnDestroy = true;
		if (!Advertises(INTERACTION_TYPE.GO_TO))
		{
			AddAdvertisedAction(INTERACTION_TYPE.GO_TO);
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataTombstone saveDataTombstone = data as SaveDataTombstone;
		if (string.IsNullOrEmpty(saveDataTombstone.characterID))
		{
			return;
		}
		character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTombstone.characterID);
		if (character != null && character.race.IsSapient())
		{
			AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
		}
		if (base.isBeingCarriedBy != null && character != null && character.hasMarker)
		{
			character.DisableMarker();
			if ((bool)character.marker.nameplate)
			{
				character.marker.nameplate.SetNameActiveState(state: false);
			}
		}
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		if (base.isBeingCarriedBy != null && character != null && character.hasMarker)
		{
			character.DisableMarker();
			if ((bool)character.marker.nameplate)
			{
				character.marker.nameplate.SetNameActiveState(state: false);
			}
		}
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
		character.marker.PlaceMarkerAt(gridTileLocation);
		character.DisableMarker();
		if ((bool)character.marker.nameplate)
		{
			character.marker.nameplate.UpdateNameActiveState();
		}
		character.marker.TryCancelExpiry();
		character.SetGrave(this);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		character.marker.PlaceMarkerAt(gridTileLocation);
		character.DisableMarker();
		if ((bool)character.marker.nameplate)
		{
			character.marker.nameplate.UpdateNameActiveState();
		}
		character.marker.TryCancelExpiry();
		character.SetGrave(this);
		if (character.traitContainer.HasTrait("Plagued"))
		{
			PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(this);
		}
		if (character.race.IsSapient())
		{
			AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		if (_respawnCorpseOnDestroy)
		{
			if (base.previousTile != null)
			{
				character.EnableMarker();
				character.marker.ScheduleExpiry();
				character.marker.PlaceMarkerAt(base.previousTile);
				character.SetGrave(null);
				character.jobComponent.TriggerBuryMe();
			}
			else
			{
				if ((bool)character.marker)
				{
					character.DestroyMarker();
				}
				character.SetGrave(null);
			}
		}
		else
		{
			character.SetGrave(null);
			character.DestroyMarker();
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
	}

	public void SetRespawnCorpseOnDestroy(bool state)
	{
		_respawnCorpseOnDestroy = state;
	}

	public override string ToString()
	{
		return base.name + " - " + character?.name;
	}

	public override void SetInventoryOwner(Character p_newOwner)
	{
		base.SetInventoryOwner(p_newOwner);
		if (p_newOwner != null && character.hasMarker)
		{
			character.DisableMarker();
			if ((bool)character.marker.nameplate)
			{
				character.marker.nameplate.SetNameActiveState(state: false);
			}
		}
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		character = null;
		_users = null;
	}

	public void SetCharacter(Character character)
	{
		this.character = character;
		Initialize(TILE_OBJECT_TYPE.TOMBSTONE, shouldAddCommonAdvertisements: false);
	}

	public void SetCharacter(Character character, SaveDataTileObject data)
	{
		this.character = character;
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		Character character = this.character;
		if (character == null || character.reactionComponent.charactersThatSawThisDead.Contains(actor))
		{
			return;
		}
		character.reactionComponent.AddCharacterThatSawThisDead(actor);
		if (actor.traitContainer.HasTrait("Psychopath"))
		{
			if (character.isNormalCharacter)
			{
				if (UnityEngine.Random.Range(0, 2) == 0)
				{
					actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, character);
				}
				else
				{
					actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, character);
				}
			}
			return;
		}
		string opinionLabel = actor.relationshipContainer.GetOpinionLabel(character);
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
		{
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Saw_Dead");
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, character, "", null, "Saw_Dead");
			}
		}
		else if (actor.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
		{
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Saw_Dead");
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, character, "", null, "Saw_Dead");
			}
		}
		else if (opinionLabel == "Enemy")
		{
			if (UnityEngine.Random.Range(0, 100) < 25)
			{
				if (UnityEngine.Random.Range(0, 2) == 0)
				{
					actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, character);
				}
				else
				{
					actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, character);
				}
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character, "", null, "Shocked_Witness_Reason");
			}
		}
		else if (opinionLabel == "Rival")
		{
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, character);
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, character);
			}
		}
		else if (character.isNormalCharacter && actor.relationshipContainer.HasRelationshipWith(character))
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character, "", null, "Shocked_Witness_Reason");
		}
	}
}
