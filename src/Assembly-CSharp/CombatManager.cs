using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CombatManager : BaseMonoBehaviour
{
	public delegate void ElementalTraitProcessor(ITraitable target, Trait trait);

	public static CombatManager Instance;

	public int searchLength;

	public int spread;

	public int aimStrength;

	public const int pursueDuration = 4;

	public const string Hostility = "Hostility";

	public const string Retaliation = "Retaliation";

	public const string Berserked = "Berserked";

	public const string Action = "Action";

	public const string Threatened = "Threatened";

	public const string Anger = "Anger";

	public const string Join_Combat = "Join Combat";

	public const string Drunk = "Drunk";

	public const string Rage = "Rage";

	public const string Demon_Kill = "Demon Kill";

	public const string Dig = "Dig";

	public const string Avoiding_Witnesses = "Avoiding Witnesses";

	public const string Encountered_Hostile = "Encountered_Hostile";

	public const string Clear_Demonic_Intrusion = "Clear_Demonic_Intrusion";

	public const string Abduct = "Abduct";

	public const string Apprehend = "Apprehend";

	public const string Monster_Scent = "Monster_Scent";

	public const string Fullness_Recovery = "Fullness_Recovery";

	public const string Taunted = "Taunted";

	public const string Tanking = "Tanking";

	public const string Snatch = "Snatch";

	public const string Music_Hater_Knockout = "Music_Hater_Knockout";

	public const string Music_Hater_Murder = "Music_Hater_Murder";

	public const string Slay_Target = "Slay_Target";

	public const string Destroy_Dangerous = "Destroy_Dangerous";

	public const string Stalker_Hunt = "Stalker_Hunt";

	public const string Grudge = "Grudge";

	public const string Pet_Owner_Grudge = "Pet_Owner_Grudge";

	public const string Assassination = "Assassination";

	public const string Critical_Break = "Critical_Break";

	public const string Raid = "Raid";

	public const string Warring_Factions = "Warring_Factions";

	public const string Slaying_Monster = "Slaying_Monster";

	public const string Slaying_Undead = "Slaying_Undead";

	public const string Slaying_Demon = "Slaying_Demon";

	public const string Slaying_Villager = "Slaying_Villager";

	public const string Incapacitating_Monster = "Incapacitating_Monster";

	public const string Incapacitating_Undead = "Incapacitating_Undead";

	public const string Incapacitating_Demon = "Incapacitating_Demon";

	public const string Incapacitating_Villager = "Incapacitating_Villager";

	public const string Fighting_Vagrant = "Fighting_Vagrant";

	public const string Feral_Monster = "Feral_Monster";

	public const string Hostile_Undead = "Hostile_Undead";

	public const string Defending_Territory = "Defending_Territory";

	public const string Defending_Home = "Defending_Home";

	public const string Destroy_Defenses = "Destroy_Defenses";

	public const string Resisting_Arrest = "Resisting_Arrest";

	public const string Resisting_Abduction = "Resisting_Abduction";

	public const string Defending_Self = "Defending_Self";

	public const string Vulnerable = "Vulnerable";

	public const string Coward = "Coward";

	public const string Unkillable_Target = "Unkillable";

	public const string Unknown = "Unknown";

	public const string Destroy_Music_Hater = "Destroy_Music_Hater";

	public const string Destroy_Cultist_Kit = "Destroy_Cultist_Kit";

	public const string Destroy_Angry = "Destroy_Angry";

	public const string Destroy_Suspicious = "Destroy_Suspicious";

	public const string Destroy_Blocker = "Destroy_Blocker";

	public const string Destroy_Stalker_Werewolf_Pelt = "Destroy_Stalker_Werewolf_Pelt";

	private COMBAT_SPECIAL_SKILL[] combatSpecialSkillTypes;

	public Dictionary<CHARACTER_COMBAT_BEHAVIOUR, CharacterCombatBehaviour> characterCombatBehaviours { get; private set; }

	public Dictionary<COMBAT_SPECIAL_SKILL, CombatSpecialSkill> combatSpecialSkills { get; private set; }

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
		ConstructAllCharacterCombatBehaviours();
		ConstructAllCombatSpecialSkills();
	}

	public void ApplyElementalDamage(int damage, ELEMENTAL_TYPE elementalType, ITraitable target, Character characterResponsible = null, ElementalTraitProcessor elementalTraitProcessor = null, bool createHitEffect = true, bool setAsPlayerSource = false, float piercing = 0f)
	{
		ITraitable traitable = target;
		if (target is Character character)
		{
			traitable = character.mountComponent.GetRider();
		}
		if (traitable == null)
		{
			traitable = target;
		}
		ElementalDamageData elementalDamageData = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
		if (traitable != null && createHitEffect)
		{
			CreateHitEffectAt(traitable, elementalType);
		}
		if (damage < 0)
		{
			if (traitable.traitContainer.HasTrait("Resting") && traitable is Character character2)
			{
				character2.jobQueue.CancelFirstJob();
			}
			if (traitable is Character character3)
			{
				character3.reactionComponent.SetDisguisedCharacter(null);
			}
		}
		if (!string.IsNullOrEmpty(elementalDamageData.addedTraitName) && traitable.traitContainer.AddTrait(traitable, elementalDamageData.addedTraitName, out var trait, characterResponsible, bypassElementalChance: false, -1, piercing, elementalType))
		{
			IElementalTrait elementalTrait = traitable.traitContainer.GetTraitOrStatus<Trait>(elementalDamageData.addedTraitName) as IElementalTrait;
			if (elementalTrait != null && setAsPlayerSource)
			{
				elementalTrait.SetIsPlayerSource(p_state: true);
			}
			if (elementalType == ELEMENTAL_TYPE.Electric)
			{
				ChainElectricDamage(traitable, damage, characterResponsible, traitable, setAsPlayerSource);
			}
			if (elementalTraitProcessor != null)
			{
				elementalTraitProcessor(traitable, trait);
			}
			else
			{
				DefaultElementalTraitProcessor(traitable, trait);
			}
		}
		GeneralElementProcess(traitable, characterResponsible);
		switch (elementalType)
		{
		case ELEMENTAL_TYPE.Earth:
			EarthElementProcess(traitable);
			break;
		case ELEMENTAL_TYPE.Wind:
			WindElementProcess(traitable, characterResponsible, setAsPlayerSource);
			break;
		case ELEMENTAL_TYPE.Fire:
			FireElementProcess(traitable);
			break;
		case ELEMENTAL_TYPE.Water:
			WaterElementProcess(traitable);
			break;
		case ELEMENTAL_TYPE.Electric:
			ElectricElementProcess(traitable);
			break;
		case ELEMENTAL_TYPE.Ice:
			IceElementProcess(traitable);
			break;
		case ELEMENTAL_TYPE.Normal:
			NormalElementProcess(traitable);
			break;
		}
	}

	public void ModifyDamage(ref int damage, ELEMENTAL_TYPE elementalType, float piercingPower, ITraitable target)
	{
		if (damage >= 0)
		{
			return;
		}
		if (target.traitContainer.HasTrait("Immune"))
		{
			damage = 0;
			return;
		}
		if (HasSpecialImmunityToElement(target, elementalType))
		{
			if (target is Vapor)
			{
				damage = 0;
				return;
			}
			damage = Mathf.RoundToInt((float)damage * 0.15f);
			if (damage >= 0)
			{
				damage = -1;
			}
		}
		if (elementalType == ELEMENTAL_TYPE.Fire && target.traitContainer.HasTrait("Fire Prone"))
		{
			damage *= 2;
		}
		if (target is Character character)
		{
			character.piercingAndResistancesComponent.ModifyValueByResistance(ref damage, elementalType, piercingPower);
			if (character.traitContainer.HasTrait("Endure Buff"))
			{
				damage = Mathf.FloorToInt((float)damage - (float)damage * 0.5f);
			}
		}
		else if (elementalType == ELEMENTAL_TYPE.Electric && target is TileObject && !(target is GenericTileObject))
		{
			damage = Mathf.RoundToInt((float)damage * 0.25f);
			if (damage >= 0)
			{
				damage = -1;
			}
		}
	}

	public bool HasSpecialImmunityToElement(ITraitable target, ELEMENTAL_TYPE elementalType)
	{
		if (target is Vapor && elementalType != ELEMENTAL_TYPE.Ice && elementalType != ELEMENTAL_TYPE.Poison && elementalType != ELEMENTAL_TYPE.Fire)
		{
			return true;
		}
		if (elementalType != ELEMENTAL_TYPE.Fire && target is WinterRose)
		{
			return true;
		}
		if (elementalType != ELEMENTAL_TYPE.Water && target is DesertRose)
		{
			return true;
		}
		return false;
	}

	public bool IsImmuneToElement(ITraitable target, ELEMENTAL_TYPE elementalType)
	{
		if (HasSpecialImmunityToElement(target, elementalType))
		{
			return true;
		}
		switch (elementalType)
		{
		case ELEMENTAL_TYPE.Fire:
			if (target.traitContainer.HasTrait("Fire Prone"))
			{
				return false;
			}
			if (target.traitContainer.HasTrait("Fire Resistant"))
			{
				return true;
			}
			break;
		case ELEMENTAL_TYPE.Electric:
			if (target.traitContainer.HasTrait("Electric"))
			{
				return true;
			}
			break;
		case ELEMENTAL_TYPE.Ice:
			if (target.traitContainer.HasTrait("Cold Blooded", "Iceproof"))
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void CreateHitEffectAt(IDamageable poi, ELEMENTAL_TYPE elementalType)
	{
		ElementalDamageData elementalDamageData = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
		if (poi.gridTileLocation != null)
		{
			GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(elementalDamageData.hitEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
			if (poi.projectileReceiver != null)
			{
				gameObject.transform.position = poi.projectileReceiver.transform.position;
			}
			else if (poi.mapObjectVisual != null)
			{
				gameObject.transform.position = poi.mapObjectVisual.transform.position;
			}
			else
			{
				gameObject.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
			}
			gameObject.SetActive(value: true);
		}
	}

	public void PoisonExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius, bool isPlayerSource)
	{
		StartCoroutine(PoisonExplosionCoroutine(target, targetTile, stacks, characterResponsible, radius, isPlayerSource));
		if (characterResponsible == null)
		{
			Messenger.Broadcast(PlayerSignals.POISON_EXPLOSION_TRIGGERED_BY_PLAYER, target);
		}
	}

	private IEnumerator PoisonExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius, bool isPlayerSource)
	{
		while (GameManager.Instance.isPaused)
		{
			yield return null;
		}
		yield return GameUtilities.waitForQuarterOfSecond;
		GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Explosion);
		AkSoundEngine.PostEvent("Play_Poison_Explosion", in_gameObjectID);
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list2, radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		float num = 0.05f * (float)stacks;
		if (num > 1f)
		{
			num = 1f;
		}
		BurningSource bs = null;
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		for (int j = 0; j < list.Count; j++)
		{
			ITraitable traitable = list[j];
			PoisonExplosionEffect(traitable, num, characterResponsible, ref bs, isPlayerSource);
		}
		RuinarchListPool<ITraitable>.Release(list);
	}

	private void PoisonExplosionEffect(ITraitable traitable, float damagePercentage, Character characterResponsible, ref BurningSource bs, bool isPlayerSource)
	{
		int num = Mathf.RoundToInt((float)traitable.maxHP * damagePercentage);
		traitable.AdjustHP(-num, ELEMENTAL_TYPE.Fire, triggerDeath: true, characterResponsible, null, showHPBar: true, 0f, isPlayerSource);
		if (!traitable.traitContainer.HasTrait("Burning"))
		{
			return;
		}
		Burning traitOrStatus = traitable.traitContainer.GetTraitOrStatus<Burning>("Burning");
		if (traitOrStatus != null && traitOrStatus.sourceOfBurning == null)
		{
			if (bs == null)
			{
				bs = new BurningSource();
			}
			traitOrStatus.SetSourceOfBurning(bs, traitable);
		}
	}

	public void FrozenExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks, bool isPlayerSource)
	{
		StartCoroutine(FrozenExplosionCoroutine(target, targetTile, stacks, isPlayerSource));
	}

	private IEnumerator FrozenExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks, bool isPlayerSource)
	{
		while (GameManager.Instance.isPaused)
		{
			yield return null;
		}
		yield return GameUtilities.waitForQuarterOfSecond;
		GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Frozen_Explosion);
		AkSoundEngine.PostEvent("Play_Freezing_Trap_Explosion", in_gameObjectID);
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list2, 2, 0, includeCenterTile: false, includeTilesInDifferentStructure: true);
		float num = 0.2f * (float)stacks;
		if (num > 1f)
		{
			num = 1f;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		for (int j = 0; j < list.Count; j++)
		{
			ITraitable traitable = list[j];
			FrozenExplosionEffect(traitable, num, isPlayerSource);
		}
		RuinarchListPool<ITraitable>.Release(list);
	}

	private void FrozenExplosionEffect(ITraitable traitable, float damagePercentage, bool isPlayerSource)
	{
		int num = Mathf.RoundToInt((float)traitable.maxHP * damagePercentage);
		traitable.AdjustHP(-num, ELEMENTAL_TYPE.Water, triggerDeath: true, null, null, showHPBar: true, 0f, isPlayerSource);
	}

	public void ChainElectricDamage(ITraitable traitable, int damage, Character characterResponsible, ITraitable origin, bool setAsPlayerSource = false)
	{
		if (characterResponsible == null)
		{
			Messenger.Broadcast(PlayerSignals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER);
		}
		if (traitable.gridTileLocation != null && !traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Chained Electric"))
		{
			traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable.gridTileLocation.tileObjectComponent.genericTileObject, "Chained Electric", characterResponsible);
			ChainedElectric traitOrStatus = traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<ChainedElectric>("Chained Electric");
			traitOrStatus.SetDamage(damage);
			traitOrStatus?.SetIsPlayerSource(setAsPlayerSource);
		}
	}

	private void ChainElectricEffect(ITraitable traitable, int damage, Character responsibleCharacter, ITraitable origin)
	{
		traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, triggerDeath: true, responsibleCharacter, null, showHPBar: true);
	}

	private void EarthElementProcess(ITraitable target)
	{
		string text = string.Empty;
		if (target.traitContainer.HasTrait("Zapped"))
		{
			text += " Zapped";
		}
		if (target.traitContainer.HasTrait("Burning"))
		{
			text += " Burning";
		}
		if (target.traitContainer.HasTrait("Poisoned"))
		{
			text += " Poisoned";
		}
		if (target.traitContainer.HasTrait("Wet"))
		{
			text += " Wet";
		}
		if (target.traitContainer.HasTrait("Freezing"))
		{
			text += " Freezing";
		}
		if (text != string.Empty)
		{
			text = text.TrimStart(' ');
			string[] array = text.Split(' ');
			target.traitContainer.RemoveTrait(target, array[UnityEngine.Random.Range(0, array.Length)]);
		}
	}

	private void WindElementProcess(ITraitable target, Character responsibleCharacter, bool setAsPlayerSource)
	{
		if (target.traitContainer.HasTrait("Poisoned"))
		{
			int stacks = target.traitContainer.stacks["Poisoned"];
			target.traitContainer.RemoveStatusAndStacks(target, "Poisoned");
			InnerMapManager.Instance.SpawnPoisonCloud(target.gridTileLocation, stacks, GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
			if (setAsPlayerSource)
			{
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_POISON_CLOUD);
			}
		}
		if (target.traitContainer.HasTrait("Wet"))
		{
			bool flag = true;
			if (target is GenericTileObject { gridTileLocation: not null } genericTileObject && genericTileObject.gridTileLocation.structure.structureType == STRUCTURE_TYPE.OCEAN)
			{
				flag = false;
			}
			else if (target is FishingSpot || target is WaterWell)
			{
				flag = false;
			}
			int stacks2 = target.traitContainer.stacks["Wet"];
			if (flag)
			{
				target.traitContainer.RemoveStatusAndStacks(target, "Wet");
			}
			Vapor vapor = new Vapor();
			vapor.SetGridTileLocation(target.gridTileLocation);
			vapor.OnPlacePOI();
			vapor.SetStacks(stacks2);
			if (responsibleCharacter == null)
			{
				Messenger.Broadcast(PlayerSignals.VAPOR_FROM_WIND_TRIGGERED_BY_PLAYER);
			}
		}
	}

	private void FireElementProcess(ITraitable target)
	{
		if (target is WinterRose winterRose)
		{
			winterRose.WinterRoseEffect();
		}
		else if (target is PoisonCloud poisonCloud)
		{
			poisonCloud.Explode();
		}
		else if (target is IceBlockWall iceBlockWall)
		{
			iceBlockWall.OnHitByFire();
		}
	}

	private void WaterElementProcess(ITraitable target)
	{
		if (target is Cinder cinder)
		{
			cinder.OnHitByWater();
		}
	}

	private void ElectricElementProcess(ITraitable target)
	{
		if (target is Golem)
		{
			if (target.traitContainer.HasTrait("Hibernating"))
			{
				target.traitContainer.RemoveTrait(target, "Hibernating");
			}
			target.traitContainer.RemoveTrait(target, "Indestructible");
		}
		else if (target is IceBlockWall iceBlockWall)
		{
			iceBlockWall.OnHitByLightning();
		}
	}

	private void IceElementProcess(ITraitable target)
	{
		if (target is Cinder cinder)
		{
			cinder.OnHitByIce();
		}
	}

	private void NormalElementProcess(ITraitable target)
	{
	}

	private void GeneralElementProcess(ITraitable target, Character source)
	{
		if (source != null && source.faction != null && source.faction.isPlayerFaction && target is Dragon { isAwakened: not false } dragon)
		{
			dragon.SetIsAttackingPlayer(state: true);
		}
	}

	public void DefaultElementalTraitProcessor(ITraitable traitable, Trait trait)
	{
		if (trait is Burning burning)
		{
			BurningSource source = new BurningSource();
			burning.SetSourceOfBurning(source, traitable);
		}
	}

	public Projectile CreateNewProjectile(Character actor, ELEMENTAL_TYPE elementalType, Transform parent, Vector3 worldPos)
	{
		if (actor != null && actor is Dragon)
		{
			return ObjectPoolManager.Instance.CreateNewDragonProjectile(worldPos, parent);
		}
		return ObjectPoolManager.Instance.CreateNewProjectile(elementalType, worldPos, parent);
	}

	public Projectile CreateNewProjectile(ELEMENTAL_TYPE elementalType, Transform parent, Vector3 worldPos)
	{
		return ObjectPoolManager.Instance.CreateNewProjectile(elementalType, worldPos, parent);
	}

	public static void ModifyValueByPiercingAndResistance(ref int p_value, float p_piercingPower, float p_resistance)
	{
		float num = (100f - (p_resistance - p_piercingPower)) / 100f;
		if (num > 1f)
		{
			num = 1f;
		}
		else if (num < 0f)
		{
			num = 0f;
		}
		float f = (float)p_value * num;
		p_value = Mathf.RoundToInt(f);
		if (p_resistance < 0f)
		{
			float num2 = (100f - p_resistance) / 100f;
			float f2 = (float)p_value * num2;
			p_value = Mathf.RoundToInt(f2);
		}
	}

	public static void ModifyValueByPiercingAndResistance(ref float p_value, float p_piercingPower, float p_resistance)
	{
		float num = (100f - (p_resistance - p_piercingPower)) / 100f;
		if (num > 1f)
		{
			num = 1f;
		}
		else if (num < 0f)
		{
			num = 0f;
		}
		float num2 = p_value * num;
		p_value = num2;
	}

	private void ConstructAllCharacterCombatBehaviours()
	{
		CHARACTER_COMBAT_BEHAVIOUR[] enumValues = CollectionUtilities.GetEnumValues<CHARACTER_COMBAT_BEHAVIOUR>();
		characterCombatBehaviours = new Dictionary<CHARACTER_COMBAT_BEHAVIOUR, CharacterCombatBehaviour>();
		foreach (CHARACTER_COMBAT_BEHAVIOUR cHARACTER_COMBAT_BEHAVIOUR in enumValues)
		{
			if (cHARACTER_COMBAT_BEHAVIOUR != CHARACTER_COMBAT_BEHAVIOUR.None)
			{
				string text = cHARACTER_COMBAT_BEHAVIOUR.ToStringEnumNoSpace() + "CombatBehaviour, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				CharacterCombatBehaviour value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating character combat behaviour for " + text)) as CharacterCombatBehaviour;
				characterCombatBehaviours.Add(cHARACTER_COMBAT_BEHAVIOUR, value);
			}
		}
	}

	public CharacterCombatBehaviour GetCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType)
	{
		if (characterCombatBehaviours.ContainsKey(p_behaviourType))
		{
			return characterCombatBehaviours[p_behaviourType];
		}
		return null;
	}

	private void ConstructAllCombatSpecialSkills()
	{
		combatSpecialSkillTypes = CollectionUtilities.GetEnumValues<COMBAT_SPECIAL_SKILL>();
		combatSpecialSkills = new Dictionary<COMBAT_SPECIAL_SKILL, CombatSpecialSkill>();
		for (int i = 0; i < combatSpecialSkillTypes.Length; i++)
		{
			COMBAT_SPECIAL_SKILL cOMBAT_SPECIAL_SKILL = combatSpecialSkillTypes[i];
			if (cOMBAT_SPECIAL_SKILL != COMBAT_SPECIAL_SKILL.None)
			{
				string text = Utilities.NotNormalizedConversionEnumToStringNoSpaces(cOMBAT_SPECIAL_SKILL.ToString()) + "SpecialSkill, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				CombatSpecialSkill value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating combat special skill for " + text)) as CombatSpecialSkill;
				combatSpecialSkills.Add(cOMBAT_SPECIAL_SKILL, value);
			}
		}
	}

	public CombatSpecialSkill GetCombatSpecialSkill(COMBAT_SPECIAL_SKILL p_skillType)
	{
		if (combatSpecialSkills.ContainsKey(p_skillType))
		{
			return combatSpecialSkills[p_skillType];
		}
		return null;
	}

	public void PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(Character p_actor, int p_currentSkillTier, COMBAT_SPECIAL_SKILL_CATEGORY p_category, List<COMBAT_SPECIAL_SKILL> p_higherTierSkills)
	{
		for (int i = 0; i < combatSpecialSkillTypes.Length; i++)
		{
			COMBAT_SPECIAL_SKILL cOMBAT_SPECIAL_SKILL = combatSpecialSkillTypes[i];
			CombatSpecialSkill combatSpecialSkill = GetCombatSpecialSkill(cOMBAT_SPECIAL_SKILL);
			if (combatSpecialSkill != null && combatSpecialSkill.category == p_category && cOMBAT_SPECIAL_SKILL.CanBeLearned() && combatSpecialSkill.CanBeLearnedByCharacter(p_actor) && combatSpecialSkill.tier > p_currentSkillTier)
			{
				p_higherTierSkills.Add(cOMBAT_SPECIAL_SKILL);
			}
		}
	}

	public bool IsDamageSourceFromPlayerSpell(object source)
	{
		if (source != null)
		{
			if (source is SkillData { category: PLAYER_SKILL_CATEGORY.SPELL })
			{
				return true;
			}
			if (source is LocustSwarm { isPlayerSource: not false })
			{
				return true;
			}
			if (source is Tornado { isPlayerSource: not false })
			{
				return true;
			}
		}
		return false;
	}

	public void CreateProjectile(CharacterMarker actorMarker, IDamageable target, CombatState state, Action<Character, IDamageable, CombatState, Projectile> onHitAction = null)
	{
		CreateProjectile(actorMarker.character, target, state, actorMarker.projectileParent.transform.position, onHitAction);
	}

	public void CreateProjectile(Character actor, IDamageable target, CombatState state, Vector3 projectileStartPosition, Action<Character, IDamageable, CombatState, Projectile> onHitAction = null)
	{
		if (target == null || target.currentHP <= 0 || target.gridTileLocation == null)
		{
			return;
		}
		Projectile projectile = CreateNewProjectile(actor, actor.combatComponent.currentElement.type, GridMap.Instance.mainRegion.innerMap.objectsParent, projectileStartPosition);
		Vector3 projectileTargetPosition = target.GetProjectileTargetPosition();
		projectile.SetTarget(projectileTargetPosition, target, state, actor);
		if (onHitAction != null)
		{
			projectile.onHitAction = onHitAction;
		}
		else
		{
			projectile.onHitAction = OnProjectileHit;
		}
		_ = actor.gridTileLocation;
		if (actor.hasMarker)
		{
			if (actor.race == RACE.ANGEL)
			{
				AkSoundEngine.PostEvent("Play_Angel_Magic_Attack", actor.marker.gameObject);
			}
			else if (actor.combatComponent.currentElement.type == ELEMENTAL_TYPE.Water)
			{
				AkSoundEngine.PostEvent("Play_Water_Projectile", actor.marker.gameObject);
			}
			else if (actor.combatComponent.currentElement.type == ELEMENTAL_TYPE.Fire)
			{
				AkSoundEngine.PostEvent("Play_Fire_Shoot", actor.marker.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_Arrow_Shoot", actor.marker.gameObject);
			}
		}
	}

	public void CreateDummyProjectile(CharacterMarker actorMarker, IDamageable target)
	{
		if (target != null && target.currentHP > 0 && target.gridTileLocation != null)
		{
			Projectile projectile = CreateNewProjectile(actorMarker.character, actorMarker.character.combatComponent.currentElement.type, actorMarker.character.currentRegion.innerMap.objectsParent, actorMarker.projectileParent.transform.position);
			projectile.SetTarget(target.GetProjectileTargetPosition(), target, null, actorMarker.character);
			projectile.onHitAction = OnDummyProjectileHit;
			if (actorMarker.character.race == RACE.ANGEL)
			{
				AkSoundEngine.PostEvent("Play_Angel_Magic_Attack", actorMarker.gameObject);
			}
			else if (actorMarker.character.combatComponent.currentElement.type == ELEMENTAL_TYPE.Water)
			{
				AkSoundEngine.PostEvent("Play_Water_Projectile", actorMarker.gameObject);
			}
			else if (actorMarker.character.combatComponent.currentElement.type == ELEMENTAL_TYPE.Fire)
			{
				AkSoundEngine.PostEvent("Play_Fire_Shoot", actorMarker.character.marker.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_Arrow_Shoot", actorMarker.gameObject);
			}
		}
	}

	private void OnProjectileHit(Character actor, IDamageable target, CombatState fromState, Projectile projectile)
	{
		if (actor == null)
		{
			return;
		}
		if (target.mapObjectVisual != null)
		{
			if (projectile.projectileElement == ELEMENTAL_TYPE.Fire)
			{
				AkSoundEngine.PostEvent("Play_Fire_Hit", target.mapObjectVisual.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_Arrow_Hit_Body", target.mapObjectVisual.gameObject);
			}
		}
		if (actor.stateComponent.currentState is CombatState combatState)
		{
			if (projectile.isAOE)
			{
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				List<ITraitable> list2 = RuinarchListPool<ITraitable>.Claim();
				target.gridTileLocation.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].PopulateAliveTraitablesOnTile(list2);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					ITraitable traitable = list2[j];
					combatState.OnAttackHit((traitable == actor) ? null : traitable);
				}
				RuinarchListPool<ITraitable>.Release(list2);
				RuinarchListPool<LocationGridTile>.Release(list);
			}
			else
			{
				combatState.OnAttackHit(target);
			}
			return;
		}
		string attackSummary = string.Empty;
		if (projectile.isAOE)
		{
			List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
			List<ITraitable> list4 = RuinarchListPool<ITraitable>.Claim();
			target.gridTileLocation.PopulateTilesInRadius(list3, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int k = 0; k < list3.Count; k++)
			{
				list3[k].PopulateAliveTraitablesOnTile(list4);
			}
			for (int l = 0; l < list4.Count; l++)
			{
				ITraitable traitable2 = list4[l];
				traitable2.OnHitByAttackFrom((traitable2 == actor) ? null : actor, fromState, ref attackSummary);
			}
			RuinarchListPool<ITraitable>.Release(list4);
			RuinarchListPool<LocationGridTile>.Release(list3);
		}
		else
		{
			target.OnHitByAttackFrom(actor, fromState, ref attackSummary);
		}
	}

	private void OnDummyProjectileHit(Character actor, IDamageable target, CombatState fromState, Projectile projectile)
	{
		if (actor == null)
		{
			return;
		}
		if (target is TrainingDummy || target is ArcheryTarget)
		{
			if (target.mapObjectVisual != null)
			{
				AkSoundEngine.PostEvent("Play_Arrow_Hit_Body", target.mapObjectVisual.gameObject);
			}
			CreateHitEffectAt(target, actor.combatComponent.currentElement.type);
		}
		else
		{
			OnProjectileHit(actor, target, null, projectile);
		}
	}

	public void PlayAudioForMeleeAttack(Character actor, IDamageable p_target)
	{
		if (!actor.hasMarker)
		{
			return;
		}
		if (actor.race == RACE.ENT)
		{
			AkSoundEngine.PostEvent("Play_Ent_Attack", actor.marker.gameObject);
			return;
		}
		if (actor.race == RACE.ANGEL)
		{
			AkSoundEngine.PostEvent((p_target is Character) ? "Play_Angel_Hit_Character" : "Play_Angel_Hit_Structure", actor.marker.gameObject);
			return;
		}
		if (actor is Summon)
		{
			AkSoundEngine.PostEvent("Play_Punch_Attack", actor.marker.gameObject);
			return;
		}
		switch (actor.characterClass.className)
		{
		case "Crafter":
		case "Miner":
			AkSoundEngine.PostEvent("Play_Blunt_Attack", actor.marker.gameObject);
			break;
		case "Noble":
		case "Knight":
		case "Barbarian":
		case "Marauder":
			AkSoundEngine.PostEvent((p_target is Character) ? "Play_Sword_Attack_Flesh" : "Play_Sword_Attack_Object", actor.marker.gameObject);
			break;
		default:
			AkSoundEngine.PostEvent("Play_Punch_Attack", actor.marker.gameObject);
			break;
		}
	}
}
