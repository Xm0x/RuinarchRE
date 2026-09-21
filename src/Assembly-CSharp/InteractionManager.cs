using System;
using System.Collections.Generic;
using Interrupts;
using Traits;
using UnityEngine;
using UtilityScripts;

public class InteractionManager : BaseMonoBehaviour
{
	public static InteractionManager Instance = null;

	public const string Goap_State_Success = "Success";

	public const string Goap_State_Fail = "Fail";

	public static readonly int Character_Action_Delay = 5;

	private string dailyInteractionSummary;

	private Dictionary<GOAP_EFFECT_CONDITION, GoapEffect[]> _effectsCategorizedByEffectCondition;

	private Dictionary<string, GoapEffect> _hasPOIGoapEffectActor;

	private Dictionary<string, GoapEffect> _hasPOIGoapEffectTarget;

	private Dictionary<string, GoapEffect> _takePOIGoapEffectActor;

	private Dictionary<string, GoapEffect> _takePOIGoapEffectTarget;

	public string[] vigilantCancellingTraits = new string[4] { "Resting", "Unconscious", "Restrained", "Zapped" };

	[Header("Actions")]
	public StringSpriteDictionary actionIconDictionary;

	public Dictionary<INTERACTION_TYPE, GoapAction> goapActionData { get; private set; }

	public List<GoapAction> goapActionList { get; private set; }

	public Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffectCondition { get; private set; }

	public Dictionary<INTERRUPT, Interrupt> interruptData { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
		ConstructHasPOIGoapEffectActorData();
		ConstructHasPOIGoapEffectTargetData();
		ConstructTakePOIGoapEffectActorData();
		ConstructTakePOIGoapEffectTargetData();
		ConstructGoapEffectData();
		ConstructGoapActionData();
		ConstructInterruptData();
	}

	private void ConstructGoapActionData()
	{
		goapActionData = new Dictionary<INTERACTION_TYPE, GoapAction>();
		goapActionList = new List<GoapAction>();
		actionsCategorizedByEffectCondition = new Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>>();
		INTERACTION_TYPE[] enumValues = CollectionUtilities.GetEnumValues<INTERACTION_TYPE>();
		foreach (INTERACTION_TYPE iNTERACTION_TYPE in enumValues)
		{
			Type type = Type.GetType(Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(iNTERACTION_TYPE.ToStringEnum()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (!(type != null))
			{
				continue;
			}
			GoapAction goapAction = Activator.CreateInstance(type) as GoapAction;
			goapActionData.Add(iNTERACTION_TYPE, goapAction);
			goapActionList.Add(goapAction);
			if (goapAction.possibleExpectedEffectsTypeAndTargetMatching.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < goapAction.possibleExpectedEffectsTypeAndTargetMatching.Count; j++)
			{
				GoapEffectConditionTypeAndTargetType goapEffectConditionTypeAndTargetType = goapAction.possibleExpectedEffectsTypeAndTargetMatching[j];
				if (actionsCategorizedByEffectCondition.ContainsKey(goapEffectConditionTypeAndTargetType.conditionType))
				{
					actionsCategorizedByEffectCondition[goapEffectConditionTypeAndTargetType.conditionType].Add(goapAction);
					continue;
				}
				actionsCategorizedByEffectCondition.Add(goapEffectConditionTypeAndTargetType.conditionType, new List<GoapAction> { goapAction });
			}
		}
	}

	private void ConstructInterruptData()
	{
		interruptData = new Dictionary<INTERRUPT, Interrupt>();
		INTERRUPT[] enumValues = CollectionUtilities.GetEnumValues<INTERRUPT>();
		foreach (INTERRUPT iNTERRUPT in enumValues)
		{
			Type type = Type.GetType("Interrupts." + Utilities.NotNormalizedConversionEnumToStringNoSpaces(iNTERRUPT.ToStringEnum()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (type != null)
			{
				Interrupt value = Activator.CreateInstance(type) as Interrupt;
				interruptData.Add(iNTERRUPT, value);
			}
		}
	}

	public Interrupt GetInterruptData(INTERRUPT interrupt)
	{
		if (interruptData.ContainsKey(interrupt))
		{
			return interruptData[interrupt];
		}
		return null;
	}

	public bool CanSatisfyGoapActionRequirements(INTERACTION_TYPE goapType, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (goapActionData.ContainsKey(goapType))
		{
			return goapActionData[goapType].CanSatisfyRequirements(actor, poiTarget, otherData, job, shouldCheckTimeOfDays: false);
		}
		throw new Exception($"No Goap Action Data for {goapType}");
	}

	public ActualGoapNode CreateNewIllusionAction(Character actor, IPointOfInterest target, INTERACTION_TYPE actionType, bool shouldLog, bool executeAfterEffect = false)
	{
		ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(goapActionData[actionType], actor, target, null, 0);
		actualGoapNode.SetAsIllusion();
		actualGoapNode.SetCrimeType();
		if (executeAfterEffect)
		{
			actualGoapNode.currentState.afterEffect?.Invoke(actualGoapNode);
		}
		if (shouldLog)
		{
			if (actionType == INTERACTION_TYPE.ASSAULT)
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", "Abduct");
				if (LocalizationManager.Instance.HasLocalizedValue(localizedValue))
				{
					actualGoapNode.descriptionLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
				}
			}
			actualGoapNode.LogAction(actualGoapNode.descriptionLog);
		}
		return actualGoapNode;
	}

	public ActualGoapNode CreateNewIllusionAction(Character actor, IPointOfInterest target, INTERACTION_TYPE actionType, string p_stateName, bool shouldLog, bool executeAfterEffect = false)
	{
		ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(goapActionData[actionType], actor, target, null, 0);
		actualGoapNode.SetAsIllusion(p_stateName);
		actualGoapNode.SetCrimeType();
		if (executeAfterEffect)
		{
			actualGoapNode.currentState.afterEffect?.Invoke(actualGoapNode);
		}
		if (shouldLog)
		{
			if (actionType == INTERACTION_TYPE.ASSAULT)
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", "Abduct");
				if (LocalizationManager.Instance.HasLocalizedValue(localizedValue))
				{
					actualGoapNode.descriptionLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
				}
			}
			actualGoapNode.LogAction(actualGoapNode.descriptionLog);
		}
		return actualGoapNode;
	}

	public void PopulateActionTypesBasedOnCrimeType(List<INTERACTION_TYPE> p_actionTypes, Character p_actor, List<CRIME_TYPE> p_crimeTypes)
	{
		for (int i = 0; i < goapActionList.Count; i++)
		{
			GoapAction goapAction = goapActionList[i];
			if (p_crimeTypes.Contains(goapAction.GetRawCrimeType(p_actor)))
			{
				p_actionTypes.Add(goapAction.goapType);
			}
		}
	}

	public ActionIntel CreateNewIntel(ActualGoapNode node)
	{
		return new ActionIntel(node);
	}

	public InterruptIntel CreateNewIntel(InterruptHolder interruptHolder)
	{
		return new InterruptIntel(interruptHolder);
	}

	public bool TargetHasNegativeTraitEffect(Character actor, IPointOfInterest target)
	{
		return target.traitContainer.HasTraitOrStatusOf(TRAIT_EFFECT.NEGATIVE);
	}

	private void ConstructGoapEffectData()
	{
		_effectsCategorizedByEffectCondition = new Dictionary<GOAP_EFFECT_CONDITION, GoapEffect[]>
		{
			{
				GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY,
				new GoapEffect[2]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.STAMINA_RECOVERY,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.CANNOT_MOVE,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.INVITED,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.INVITED, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.STARTS_COMBAT,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.STARTS_COMBAT, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.PRODUCE_FOOD,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.CARRIED_PATIENT,
				new GoapEffect[1]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.CARRIED_PATIENT, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.BUY_OBJECT,
				new GoapEffect[18]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Wood Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Stone Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Metal Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Cloth Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Leather Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Animal Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Corn", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Elf Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Fish Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Human Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Hypno Herb", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Iceberry", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Mushroom", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Pineapple", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Potato", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Rat Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Vegetables", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.FEED,
				new GoapEffect[13]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Animal Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Corn", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Elf Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Fish Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Human Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Hypno Herb", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Iceberry", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Mushroom", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Pineapple", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Potato", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Rat Meat", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Vegetables", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.TRAIN_TALENT,
				new GoapEffect[2]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Magic", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Martial Arts", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR)
				}
			},
			{
				GOAP_EFFECT_CONDITION.DEATH,
				new GoapEffect[2]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.REMOVE_TRAIT,
				new GoapEffect[16]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Unconscious", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Poisoned", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Freezing", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Frozen", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burning", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Ensnared", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Restrained", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Booby Trapped", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Freezing Trapped", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Snare Trapped", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Landmined", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Plagued", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Injured", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Cursed", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.TAKE_POI,
				new GoapEffect[8]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Cloth Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Leather Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Cloth Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Leather Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.HAS_POI,
				new GoapEffect[11]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Metal Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cloth Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Leather Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Metal Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cloth Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Leather Pile", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Corpse", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Restrained", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			},
			{
				GOAP_EFFECT_CONDITION.HAS_TRAIT,
				new GoapEffect[6]
				{
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Lethargic", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET),
					new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Injured", isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)
				}
			}
		};
	}

	private void ConstructHasPOIGoapEffectActorData()
	{
		_hasPOIGoapEffectActor = new Dictionary<string, GoapEffect>();
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToStringEnumWithSpace();
			_hasPOIGoapEffectActor.Add(text, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, text, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		ARTIFACT_TYPE[] enumValues2 = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		for (int j = 0; j < enumValues2.Length; j++)
		{
			if (enumValues2[j] != ARTIFACT_TYPE.None)
			{
				string text2 = enumValues2[j].ToStringEnumWithSpace();
				if (!_hasPOIGoapEffectActor.ContainsKey(text2))
				{
					_hasPOIGoapEffectActor.Add(text2, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, text2, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
				}
			}
		}
	}

	private void ConstructHasPOIGoapEffectTargetData()
	{
		_hasPOIGoapEffectTarget = new Dictionary<string, GoapEffect>();
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToStringEnumWithSpace();
			_hasPOIGoapEffectTarget.Add(text, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, text, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		ARTIFACT_TYPE[] enumValues2 = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		for (int j = 0; j < enumValues2.Length; j++)
		{
			if (enumValues2[j] != ARTIFACT_TYPE.None)
			{
				string text2 = enumValues2[j].ToStringEnumWithSpace();
				if (!_hasPOIGoapEffectTarget.ContainsKey(text2))
				{
					_hasPOIGoapEffectTarget.Add(text2, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, text2, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
				}
			}
		}
	}

	private void ConstructTakePOIGoapEffectActorData()
	{
		_takePOIGoapEffectActor = new Dictionary<string, GoapEffect>();
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToStringEnumWithSpace();
			_takePOIGoapEffectActor.Add(text, new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, text, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		ARTIFACT_TYPE[] enumValues2 = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		for (int j = 0; j < enumValues2.Length; j++)
		{
			if (enumValues2[j] != ARTIFACT_TYPE.None)
			{
				string text2 = enumValues2[j].ToStringEnumWithSpace();
				if (!_takePOIGoapEffectActor.ContainsKey(text2))
				{
					_takePOIGoapEffectActor.Add(text2, new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, text2, isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
				}
			}
		}
	}

	private void ConstructTakePOIGoapEffectTargetData()
	{
		_takePOIGoapEffectTarget = new Dictionary<string, GoapEffect>();
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToStringEnumWithSpace();
			_takePOIGoapEffectTarget.Add(text, new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, text, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		ARTIFACT_TYPE[] enumValues2 = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		for (int j = 0; j < enumValues2.Length; j++)
		{
			if (enumValues2[j] != ARTIFACT_TYPE.None)
			{
				string text2 = enumValues2[j].ToStringEnumWithSpace();
				if (!_takePOIGoapEffectTarget.ContainsKey(text2))
				{
					_takePOIGoapEffectTarget.Add(text2, new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, text2, isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
				}
			}
		}
	}

	public GoapEffect GetGoapEffectData(GoapEffect p_effect)
	{
		if (p_effect != null)
		{
			return GetGoapEffectData(p_effect.conditionType, p_effect.conditionKey, p_effect.isKeyANumber, p_effect.target);
		}
		return null;
	}

	public GoapEffect GetGoapEffectData(GOAP_EFFECT_CONDITION p_condition, string p_conditionKey, bool p_isKeyANumber, GOAP_EFFECT_TARGET p_targetType)
	{
		if (p_condition == GOAP_EFFECT_CONDITION.HAS_POI && p_targetType == GOAP_EFFECT_TARGET.ACTOR)
		{
			if (_hasPOIGoapEffectActor.ContainsKey(p_conditionKey))
			{
				return _hasPOIGoapEffectActor[p_conditionKey];
			}
		}
		else if (p_condition == GOAP_EFFECT_CONDITION.HAS_POI && p_targetType == GOAP_EFFECT_TARGET.TARGET)
		{
			if (_hasPOIGoapEffectTarget.ContainsKey(p_conditionKey))
			{
				return _hasPOIGoapEffectTarget[p_conditionKey];
			}
		}
		else if (p_condition == GOAP_EFFECT_CONDITION.TAKE_POI && p_targetType == GOAP_EFFECT_TARGET.ACTOR)
		{
			if (_takePOIGoapEffectActor.ContainsKey(p_conditionKey))
			{
				return _takePOIGoapEffectActor[p_conditionKey];
			}
		}
		else if (p_condition == GOAP_EFFECT_CONDITION.TAKE_POI && p_targetType == GOAP_EFFECT_TARGET.TARGET && _takePOIGoapEffectTarget.ContainsKey(p_conditionKey))
		{
			return _takePOIGoapEffectTarget[p_conditionKey];
		}
		if (_effectsCategorizedByEffectCondition.ContainsKey(p_condition))
		{
			GoapEffect[] array = _effectsCategorizedByEffectCondition[p_condition];
			foreach (GoapEffect goapEffect in array)
			{
				if (goapEffect.conditionKey == p_conditionKey && goapEffect.isKeyANumber == p_isKeyANumber && goapEffect.target == p_targetType)
				{
					return goapEffect;
				}
			}
		}
		return null;
	}

	public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter)
	{
		if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion)
		{
			if (character.isFactionless || character.isVagrantOrFactionless)
			{
				if (character.race == targetCharacter.race && character.homeRegion == targetCharacter.homeRegion)
				{
					return !targetCharacter.relationshipContainer.IsEnemiesWith(character);
				}
				return false;
			}
			return !character.relationshipContainer.IsEnemiesWith(targetCharacter);
		}
		return false;
	}

	public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter)
	{
		if (!character.traitContainer.HasTrait("Coward"))
		{
			bool flag = !CharacterManager.Instance.IsCultistOfSameReligion(character, targetCharacter);
			if (character.traitContainer.HasTrait("Hemophiliac"))
			{
				Vampire traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && traitOrStatus.DoesCharacterKnowThisVampire(character))
				{
					flag = false;
				}
			}
			if (character.traitContainer.HasTrait("Lycanphiliac") && targetCharacter.isLycanthrope && targetCharacter.lycanData.DoesCharacterKnowThisLycan(character))
			{
				flag = false;
			}
			if (flag)
			{
				if (character.relationshipContainer.IsFriendsWith(targetCharacter))
				{
					return false;
				}
				if ((character.relationshipContainer.IsFamilyMember(targetCharacter) || character.relationshipContainer.IsLoverOrAffair(targetCharacter)) && !character.relationshipContainer.IsEnemiesWith(targetCharacter))
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public bool CanCharacterTakeRepairJob(Character character, TileObject targetTileObject)
	{
		return targetTileObject.canBeRepaired;
	}
}
