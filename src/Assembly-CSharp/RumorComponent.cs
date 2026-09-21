using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Interrupts;
using UnityEngine;
using UtilityScripts;

public class RumorComponent : CharacterComponent
{
	private List<string> _rumorPool;

	private List<ActualGoapNode> _negativeInfoPool;

	private List<IPointOfInterest> _rumorTargetPool;

	private const int Max_Negative_Info = 40;

	public List<ActualGoapNode> negativeInfoPool => _negativeInfoPool;

	public RumorComponent()
	{
		_rumorPool = new List<string>();
		_rumorTargetPool = new List<IPointOfInterest>();
		_negativeInfoPool = new List<ActualGoapNode>();
	}

	public RumorComponent(SaveDataRumorComponent data)
	{
		_rumorPool = new List<string>();
		_rumorTargetPool = new List<IPointOfInterest>();
		_negativeInfoPool = new List<ActualGoapNode>();
	}

	public ActualGoapNode GetRandomKnownNegativeInfo(Character spreadTargetCharacter, Character negativeCharacter)
	{
		if (_negativeInfoPool.Count == 0)
		{
			return null;
		}
		List<ActualGoapNode> list = RuinarchListPool<ActualGoapNode>.Claim();
		for (int i = 0; i < _negativeInfoPool.Count; i++)
		{
			ActualGoapNode actualGoapNode = _negativeInfoPool[i];
			if (actualGoapNode.descriptionLog != null && actualGoapNode.actor == negativeCharacter && actualGoapNode.poiTarget != spreadTargetCharacter)
			{
				list.Add(actualGoapNode);
			}
		}
		ActualGoapNode result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<ActualGoapNode>.Release(list);
		return result;
	}

	public void AddAssumedWitnessedOrInformedNegativeInfo(ActualGoapNode node)
	{
		if (_negativeInfoPool.Contains(node))
		{
			return;
		}
		_negativeInfoPool.Add(node);
		node.SetIsNegativeInfo(p_state: true);
		if (_negativeInfoPool.Count <= 40)
		{
			return;
		}
		ActualGoapNode actualGoapNode = _negativeInfoPool[0];
		_negativeInfoPool.RemoveAt(0);
		if (actualGoapNode != null)
		{
			actualGoapNode.SetIsNegativeInfo(p_state: false);
			if (actualGoapNode.isSupposedToBeInPool)
			{
				actualGoapNode.ProcessReturnToPool();
			}
		}
	}

	public Rumor GenerateNewRandomRumor(Character spreadTargetCharacter, Character rumoredCharacter)
	{
		_rumorPool.Clear();
		_rumorPool.AddRange(CharacterManager.Instance.rumorWorthyActions);
		string identifier = string.Empty;
		IPointOfInterest pointOfInterest = null;
		while (_rumorPool.Count > 0 && pointOfInterest == null)
		{
			string text = _rumorPool[Random.Range(0, _rumorPool.Count)];
			IPointOfInterest targetOfRumorCharacter = GetTargetOfRumorCharacter(spreadTargetCharacter, rumoredCharacter, text);
			if (targetOfRumorCharacter != null)
			{
				identifier = text;
				pointOfInterest = targetOfRumorCharacter;
			}
		}
		if (pointOfInterest != null)
		{
			return CreateNewRumor(rumoredCharacter, pointOfInterest, identifier);
		}
		return null;
	}

	private IPointOfInterest GetTargetOfRumorCharacter(Character spreadTargetCharacter, Character rumoredCharacter, string identifier)
	{
		_rumorTargetPool.Clear();
		switch (identifier)
		{
		case "Make Love":
		{
			for (int l = 0; l < base.owner.relationshipContainer.charactersWithOpinion.Count; l++)
			{
				Character character3 = base.owner.relationshipContainer.charactersWithOpinion[l];
				if (character3 != spreadTargetCharacter && character3 != rumoredCharacter && !character3.isDead && !rumoredCharacter.relationshipContainer.HasRelationshipWith(character3, RELATIONSHIP_TYPE.LOVER))
				{
					_rumorTargetPool.Add(character3);
				}
			}
			if (_rumorTargetPool.Count > 0)
			{
				return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
			}
			break;
		}
		case "Steal":
		{
			for (int num2 = 0; num2 < base.owner.relationshipContainer.charactersWithOpinion.Count; num2++)
			{
				Character character6 = base.owner.relationshipContainer.charactersWithOpinion[num2];
				if (character6 != spreadTargetCharacter && character6 != rumoredCharacter && !character6.isDead && character6.ownedItems.Count > 0)
				{
					_rumorTargetPool.Add(character6);
				}
				if (_rumorTargetPool.Count > 0)
				{
					Character character7 = _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)] as Character;
					return character7.ownedItems[Random.Range(0, character7.ownedItems.Count)];
				}
			}
			break;
		}
		case "Poison Food":
		{
			for (int j = 0; j < base.owner.relationshipContainer.charactersWithOpinion.Count; j++)
			{
				Character character2 = base.owner.relationshipContainer.charactersWithOpinion[j];
				if (character2 != spreadTargetCharacter && character2 != rumoredCharacter && !character2.isDead && character2.ownedItems.Count > 0)
				{
					for (int k = 0; k < character2.ownedItems.Count; k++)
					{
						TileObject tileObject = character2.ownedItems[k];
						if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TABLE && tileObject.gridTileLocation != null)
						{
							_rumorTargetPool.Add(tileObject);
						}
					}
				}
				if (_rumorTargetPool.Count > 0)
				{
					return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
				}
			}
			break;
		}
		case "Place Trap":
		{
			for (int m = 0; m < base.owner.relationshipContainer.charactersWithOpinion.Count; m++)
			{
				Character character4 = base.owner.relationshipContainer.charactersWithOpinion[m];
				if (character4 != spreadTargetCharacter && character4 != rumoredCharacter && !character4.isDead && character4.ownedItems.Count > 0)
				{
					for (int n = 0; n < character4.ownedItems.Count; n++)
					{
						TileObject tileObject2 = character4.ownedItems[n];
						if (!(tileObject2 is StructureTileObject) && tileObject2.gridTileLocation != null && base.owner.gridTileLocation != null && tileObject2.gridTileLocation.structure.region == base.owner.gridTileLocation.structure.region)
						{
							_rumorTargetPool.Add(tileObject2);
						}
					}
				}
				if (_rumorTargetPool.Count > 0)
				{
					return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
				}
			}
			break;
		}
		case "Drink Blood":
		{
			for (int num = 0; num < base.owner.relationshipContainer.charactersWithOpinion.Count; num++)
			{
				Character character5 = base.owner.relationshipContainer.charactersWithOpinion[num];
				if (character5 != spreadTargetCharacter && character5 != rumoredCharacter && !character5.isDead)
				{
					_rumorTargetPool.Add(character5);
				}
			}
			if (_rumorTargetPool.Count > 0)
			{
				return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
			}
			break;
		}
		case "Flirt":
		{
			for (int i = 0; i < base.owner.relationshipContainer.charactersWithOpinion.Count; i++)
			{
				Character character = base.owner.relationshipContainer.charactersWithOpinion[i];
				if (character != spreadTargetCharacter && character != rumoredCharacter && !character.isDead && !rumoredCharacter.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER))
				{
					_rumorTargetPool.Add(character);
				}
			}
			if (_rumorTargetPool.Count > 0)
			{
				return _rumorTargetPool[Random.Range(0, _rumorTargetPool.Count)];
			}
			break;
		}
		case "Transform To Wolf":
			return rumoredCharacter;
		}
		return null;
	}

	public Character GetRandomSpreadRumorOrNegativeInfoTarget(Character rumoredCharacter)
	{
		Character result = null;
		if (base.owner.homeSettlement != null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < base.owner.relationshipContainer.charactersWithOpinion.Count; i++)
			{
				Character character = base.owner.relationshipContainer.charactersWithOpinion[i];
				if (character != rumoredCharacter && CanShareInfoTo(character) && character.homeSettlement == base.owner.homeSettlement && character.limiterComponent.canPerform && character.IsAtHome())
				{
					list.Add(character);
				}
			}
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<Character>.Release(list);
		}
		return result;
	}

	private bool CanShareInfoTo(Character character)
	{
		if (!character.isDead)
		{
			return !character.traitContainer.HasTrait("Enslaved", "Travelling");
		}
		return false;
	}

	public Rumor CreateNewRumor(Character rumoredCharacter, IPointOfInterest targetOfRumoredCharacter, string identifier)
	{
		IRumorable rumorable = null;
		if (identifier == "Flirt" || identifier == "Transform To Wolf")
		{
			Interrupt interrupt = null;
			Log effectLog = null;
			if (identifier == "Flirt")
			{
				interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Flirt);
				effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter, "flirted_back");
			}
			else if (identifier == "Transform To Wolf")
			{
				interrupt = InteractionManager.Instance.GetInterruptData(INTERRUPT.Transform_To_Wolf);
				effectLog = interrupt.CreateEffectLog(rumoredCharacter, targetOfRumoredCharacter);
			}
			InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
			interruptHolder.Initialize(interrupt, rumoredCharacter, targetOfRumoredCharacter, string.Empty, string.Empty);
			interruptHolder.SetEffectLog(effectLog);
			rumorable = interruptHolder;
		}
		else
		{
			INTERACTION_TYPE key = INTERACTION_TYPE.NONE;
			switch (identifier)
			{
			case "Make Love":
				key = INTERACTION_TYPE.MAKE_LOVE;
				break;
			case "Steal":
				key = INTERACTION_TYPE.STEAL;
				break;
			case "Poison Food":
				key = INTERACTION_TYPE.POISON;
				break;
			case "Place Trap":
				key = INTERACTION_TYPE.BOOBY_TRAP;
				break;
			case "Drink Blood":
				key = INTERACTION_TYPE.DRINK_BLOOD;
				break;
			}
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[key], rumoredCharacter, targetOfRumoredCharacter, null, 0);
			actualGoapNode.SetCrimeType();
			if (identifier == "Poison Food" && targetOfRumoredCharacter.gridTileLocation != null)
			{
				actualGoapNode.SetTargetStructure(targetOfRumoredCharacter.gridTileLocation.structure);
			}
			rumorable = actualGoapNode;
		}
		if (rumorable != null)
		{
			Rumor rumor = new Rumor(base.owner, rumoredCharacter);
			rumorable.SetAsRumor(rumor);
			return rumor;
		}
		return null;
	}

	public Rumor CreateNewRumor(Character rumoredCharacter, IPointOfInterest targetOfRumoredCharacter, INTERACTION_TYPE actionType)
	{
		if (rumoredCharacter != null && targetOfRumoredCharacter != null)
		{
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[actionType], rumoredCharacter, targetOfRumoredCharacter, null, 0);
			if (actionType == INTERACTION_TYPE.POISON && targetOfRumoredCharacter.gridTileLocation != null)
			{
				actualGoapNode.SetTargetStructure(targetOfRumoredCharacter.gridTileLocation.structure);
			}
			Rumor rumor = new Rumor(base.owner, rumoredCharacter);
			actualGoapNode.SetAsRumor(rumor);
			return rumor;
		}
		return null;
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<ActualGoapNode>(ObjectPoolSignals.ACTUAL_GOAP_NODE_OBJECT_POOLED, OnActualGoapNodeObjectPooled);
	}

	public void UnsubscribeListeners()
	{
		Messenger.RemoveListener<ActualGoapNode>(ObjectPoolSignals.ACTUAL_GOAP_NODE_OBJECT_POOLED, OnActualGoapNodeObjectPooled);
	}

	private void OnActualGoapNodeObjectPooled(ActualGoapNode p_goapNode)
	{
		if (_negativeInfoPool.Contains(p_goapNode))
		{
			_negativeInfoPool.Remove(p_goapNode);
		}
	}

	public void LoadReferences(SaveDataRumorComponent data)
	{
		for (int i = 0; i < data.negativeInfoIDs.Count; i++)
		{
			string id = data.negativeInfoIDs[i];
			ActualGoapNode actionByPersistentID = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(id);
			if (actionByPersistentID.actor != null && actionByPersistentID.poiTarget != null)
			{
				_negativeInfoPool.Add(actionByPersistentID);
			}
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		_rumorTargetPool.Remove(p_character);
		if (_negativeInfoPool.Count <= 0)
		{
			return;
		}
		List<ActualGoapNode> list = RuinarchListPool<ActualGoapNode>.Claim(_negativeInfoPool.Count);
		list.AddRange(_negativeInfoPool);
		for (int i = 0; i < list.Count; i++)
		{
			ActualGoapNode actualGoapNode = list[i];
			if (actualGoapNode.IsNodeObjectInvalid() || actualGoapNode.IsCharacterReferenced(p_character))
			{
				_negativeInfoPool.Remove(actualGoapNode);
				actualGoapNode.SetIsNegativeInfo(p_state: false);
				if (actualGoapNode.isSupposedToBeInPool)
				{
					actualGoapNode.ProcessReturnToPool();
				}
			}
		}
		RuinarchListPool<ActualGoapNode>.Release(list);
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (_negativeInfoPool.Count <= 0)
		{
			return;
		}
		List<ActualGoapNode> list = RuinarchListPool<ActualGoapNode>.Claim(_negativeInfoPool.Count);
		list.AddRange(_negativeInfoPool);
		for (int i = 0; i < list.Count; i++)
		{
			ActualGoapNode actualGoapNode = list[i];
			if (actualGoapNode.IsNodeObjectInvalid() || actualGoapNode.IsStructureReferenced(p_structure))
			{
				_negativeInfoPool.Remove(actualGoapNode);
				actualGoapNode.SetIsNegativeInfo(p_state: false);
				if (actualGoapNode.isSupposedToBeInPool)
				{
					actualGoapNode.ProcessReturnToPool();
				}
			}
		}
		RuinarchListPool<ActualGoapNode>.Release(list);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < _negativeInfoPool.Count; i++)
		{
			_negativeInfoPool[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_rumorTargetPool.Contains(p_character);
		for (int i = 0; i < _negativeInfoPool.Count; i++)
		{
			_negativeInfoPool[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
