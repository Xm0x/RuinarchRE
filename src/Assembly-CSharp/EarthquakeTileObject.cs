using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class EarthquakeTileObject : AOESpellTileObject, GridTileEventDispatcher.ITileObjectTileListener
{
	private bool _hasEarthquakeStarted;

	private uint _sfxID;

	public List<IPointOfInterest> earthquakeTileObjects { get; private set; }

	public List<IPointOfInterest> pendingEarthquakeTileObjects { get; private set; }

	public int currentEarthquakeDuration { get; private set; }

	public override Type serializedData => typeof(SaveDataEarthquakeTileObject);

	protected override int effectRadius => 6;

	public EarthquakeTileObject()
		: base(TILE_OBJECT_TYPE.EARTHQUAKE_TILE_OBJECT)
	{
		earthquakeTileObjects = new List<IPointOfInterest>();
		pendingEarthquakeTileObjects = new List<IPointOfInterest>();
		currentEarthquakeDuration = 0;
	}

	public EarthquakeTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataEarthquakeTileObject saveDataEarthquakeTileObject = data as SaveDataEarthquakeTileObject;
		earthquakeTileObjects = new List<IPointOfInterest>();
		pendingEarthquakeTileObjects = new List<IPointOfInterest>();
		currentEarthquakeDuration = saveDataEarthquakeTileObject.remainingEarthquakeDuration;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		StartEarthquake();
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		StopEarthquake();
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	protected override void ProcessTileAffectedByAOESpell(LocationGridTile p_tile)
	{
		base.ProcessTileAffectedByAOESpell(p_tile);
		p_tile.eventDispatcher.SubscribeToTileObjectTileEvents(this);
	}

	protected override void ProcessTileNoLongerAffectedByAOESpell(LocationGridTile p_tile)
	{
		base.ProcessTileNoLongerAffectedByAOESpell(p_tile);
		p_tile.eventDispatcher.UnsubscribeToTileObjectTileEvents(this);
	}

	private void StartEarthquake()
	{
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Earthquake", base.mapObjectVisual.gameObject);
		earthquakeTileObjects.Clear();
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			IPointOfInterest objHere = affectedTiles[i].tileObjectComponent.objHere;
			if (objHere != null)
			{
				AddEarthquakeTileObject(objHere);
			}
		}
		if (!GameManager.Instance.isPaused)
		{
			OnStartEarthquake();
		}
		else
		{
			_hasEarthquakeStarted = false;
		}
	}

	private void OnStartEarthquake()
	{
		_hasEarthquakeStarted = true;
		CameraShake();
		Messenger.AddListener(Signals.TICK_STARTED, PerTickEarthquake);
		if (allCharactersInsideAOE == null)
		{
			return;
		}
		for (int i = 0; i < allCharactersInsideAOE.Count; i++)
		{
			Character character = allCharactersInsideAOE[i];
			if (character is Dragon dragon)
			{
				if (!dragon.isAwakened)
				{
					dragon.Awaken();
				}
			}
			else
			{
				character.traitContainer.AddTrait(character, "Disoriented");
			}
		}
	}

	private void StopEarthquake()
	{
		AkSoundEngine.StopPlayingID(_sfxID);
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEarthquake);
		StopCameraShake();
		earthquakeTileObjects.Clear();
	}

	private void PauseEarthquake()
	{
		if (_hasEarthquakeStarted)
		{
			StopCameraShake();
			Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEarthquake);
		}
	}

	private void ResumeEarthquake()
	{
		if (!_hasEarthquakeStarted)
		{
			OnStartEarthquake();
		}
		else
		{
			Messenger.AddListener(Signals.TICK_STARTED, PerTickEarthquake);
		}
	}

	private void AddEarthquakeTileObject(IPointOfInterest poi)
	{
		if (!(poi is MovingTileObject) && !poi.traitContainer.HasTrait("Immovable"))
		{
			earthquakeTileObjects.Add(poi);
		}
	}

	private void AddPendingEarthquakeTileObject(IPointOfInterest poi)
	{
		if (!(poi is MovingTileObject) && !poi.traitContainer.HasTrait("Immovable"))
		{
			pendingEarthquakeTileObjects.Add(poi);
		}
	}

	private void RemoveEarthquakeTileObject(IPointOfInterest poi)
	{
		earthquakeTileObjects.Remove(poi);
	}

	private void CameraShake()
	{
		InnerMapCameraMove.Instance.EarthquakeShake();
	}

	private void StopCameraShake()
	{
		GameManager.Instance.StartCoroutine(StopCameraShakeCoroutine());
	}

	private IEnumerator StopCameraShakeCoroutine()
	{
		yield return null;
		InnerMapCameraMove.Instance.camera.DOKill();
		InnerMapCameraMove.Instance.camera.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
	}

	private void POIShake(IPointOfInterest poi)
	{
		poi.mapObjectVisual.transform.DOShakeRotation(1f, new Vector3(0f, 0f, 5f), 40, 90f, fadeOut: false).OnComplete(delegate
		{
			OnCompletePOIShake(poi);
		});
	}

	private void OnCompletePOIShake(IPointOfInterest poi)
	{
		if (gridTileLocation != null)
		{
			POIShake(poi);
		}
	}

	private void POIMove(IPointOfInterest poi, LocationGridTile to)
	{
		if (poi.gridTileLocation != null)
		{
			poi.gridTileLocation.structure.RemovePOIWithoutDestroying(poi);
		}
		to.structure.AddPOI(poi, to);
	}

	private void PerTickEarthquake()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingLocation == gridTileLocation.parentMap.region)
		{
			if (InnerMapCameraMove.Instance.IsVisible(gridTileLocation.centeredWorldLocation))
			{
				if (!DOTween.IsTweening(InnerMapCameraMove.Instance.camera))
				{
					CameraShake();
				}
			}
			else if (DOTween.IsTweening(InnerMapCameraMove.Instance.camera))
			{
				StopCameraShake();
			}
		}
		else
		{
			StopCameraShake();
		}
		currentEarthquakeDuration++;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.EARTHQUAKE);
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		list.AddRange(affectedStructures.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			bool flag = true;
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				flag = manMadeStructure.CanBeDamagedByPlayerSpells();
			}
			else if (locationStructure is NaturalStructure || locationStructure is DemonicStructure)
			{
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			if (!locationStructure.structureType.IsOpenSpace() && locationStructure.charactersHere.Count > 0)
			{
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				list2.AddRange(locationStructure.charactersHere);
				for (int j = 0; j < list2.Count; j++)
				{
					list2[j].Death("structure_destruction", null, null, null, null, null, null, isPlayerSource: true);
				}
			}
			locationStructure.AdjustHP(-locationStructure.maxHP, null, isPlayerSource: true);
		}
		RuinarchListPool<LocationStructure>.Release(list);
		int amount = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		for (int k = 0; k < earthquakeTileObjects.Count; k++)
		{
			IPointOfInterest pointOfInterest = earthquakeTileObjects[k];
			LocationGridTile locationGridTile = pointOfInterest.gridTileLocation;
			if (locationGridTile == null)
			{
				RemoveEarthquakeTileObject(pointOfInterest);
				k--;
				continue;
			}
			float piercingPower = pierceBasedOnCurrentLevel;
			pointOfInterest.AdjustHP(amount, ELEMENTAL_TYPE.Normal, triggerDeath: false, spellData, null, showHPBar: true, piercingPower, isPlayerSource: true);
			if (pointOfInterest.gridTileLocation != null && (bool)pointOfInterest.mapObjectVisual && !DOTween.IsTweening(pointOfInterest.mapObjectVisual.transform) && UnityEngine.Random.Range(0, 100) < 30)
			{
				List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
				locationGridTile.PopulateUnoccupiedNeighboursWithNoCharactersInSameAreaAndStructure(list3);
				if (list3.Count > 0)
				{
					POIMove(pointOfInterest, list3[GameUtilities.RandomBetweenTwoNumbers(0, list3.Count - 1)]);
				}
				RuinarchListPool<LocationGridTile>.Release(list3);
			}
		}
		if (pendingEarthquakeTileObjects.Count > 0)
		{
			for (int l = 0; l < pendingEarthquakeTileObjects.Count; l++)
			{
				AddEarthquakeTileObject(pendingEarthquakeTileObjects[l]);
			}
			pendingEarthquakeTileObjects.Clear();
		}
		if (currentEarthquakeDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(spellData))
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public void ResetDuration()
	{
		currentEarthquakeDuration = 0;
	}

	public void OnTileObjectPlacedOnTile(TileObject p_tileObject, LocationGridTile p_tile)
	{
		AddPendingEarthquakeTileObject(p_tileObject);
	}

	public void OnTileObjectRemovedFromTile(TileObject p_tileObject, LocationGridTile p_tile)
	{
		RemoveEarthquakeTileObject(p_tileObject);
	}

	private void OnGamePaused(bool p_isPaused)
	{
		if (p_isPaused)
		{
			PauseEarthquake();
		}
		else
		{
			ResumeEarthquake();
		}
	}

	public override string GetAOESpellTestingData()
	{
		return base.GetAOESpellTestingData() + "\n\tCurrent Duration: " + currentEarthquakeDuration;
	}
}
