using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

public class TraitManager : BaseMonoBehaviour
{
	public static TraitManager Instance;

	private Dictionary<string, Trait> _allTraits;

	public const string Collision_Trait = "Collision_Trait";

	public const string Enter_Grid_Tile_Trait = "Enter_Grid_Tile_Trait";

	public const string Initiate_Map_Visual_Trait = "Initiate_Map_Visual_Trait";

	public const string Destroy_Map_Visual_Trait = "Destroy_Map_Visual_Trait";

	public const string Execute_Pre_Effect_Trait = "Execute_Pre_Effect_Trait";

	public const string Execute_Per_Tick_Effect_Trait = "Execute_Pre_Effect_Trait";

	public const string Execute_After_Effect_Trait = "Execute_Pre_Effect_Trait";

	public const string Start_Perform_Trait = "Start_Perform_Trait";

	public const string Death_Trait = "Death_Trait";

	public const string Tick_Ended_Trait = "Tick_Ended_Trait";

	public const string Tick_Started_Trait = "Tick_Started_Trait";

	public const string Hour_Started_Trait = "Hour_Started_Trait";

	public const string See_Poi_Trait = "See_Poi_Trait";

	public const string See_Poi_Cannot_Witness_Trait = "See_Poi_Cannot_Witness_Trait";

	public const string Before_Start_Flee = "Before_Start_Flee";

	public const string After_Exiting_Combat = "After_Exiting_Combat";

	public const string Per_Tick_While_Stationary_Unoccupied = "Per_Tick_While_Stationary_Unoccupied";

	public const string After_Death = "After_Death";

	public const string Villager_Reaction = "Villager_Reaction";

	public const string Change_Element = "Change_Element";

	public static string[] instancedTraitsAndStatuses = new string[201]
	{
		"Restrained", "Injured", "Kleptomaniac", "Lycanthrope", "Vampire", "Poisoned", "Resting", "Sick", "Unconscious", "Zapped",
		"Spooked", "Cannibal", "Lethargic", "Dead", "Unfaithful", "Drunk", "Burning", "Burnt", "Agoraphobic", "Music Lover",
		"Music Hater", "Psychopath", "Plagued", "Poisonous", "Diplomatic", "Wet", "Character Trait", "Nocturnal", "Glutton", "Suspicious",
		"Narcoleptic", "Hothead", "Inspiring", "Pyrophobic", "Angry", "Alcoholic", "Pessimist", "Lazy", "Coward", "Berserked",
		"Catatonic", "Griefstricken", "Heartbroken", "Chaste", "Lustful", "Edible", "Paralyzed", "Malnourished", "Withdrawal", "Suicidal",
		"Criminal", "Dazed", "Hiding", "Bored", "Overheating", "Freezing", "Frozen", "Ravenous", "Feeble", "Forlorn",
		"Accident Prone", "Disoriented", "Consumable", "Fire Prone", "Electric", "Venomous", "Booby Trapped", "Betrayed", "Abomination Germ", "Ensnared",
		"Melting", "Fervor", "Tended", "Tending", "Cleansing", "Dousing", "Drying", "Patrolling", "Necromancer", "Webbed",
		"Demon Cultist", "Stealthy", "Invisible", "Noxious Wanderer", "DeMooder", "Defender", "Invader", "Disabler", "Infestor", "Abductor",
		"Arsonist", "Hibernating", "Baby Infestor", "Tower", "Mighty", "Stoned", "Transforming", "Subterranean", "Petrasol", "Snatcher",
		"Agitated", "Hunting", "Chained Electric", "Prisoner", "Hemophiliac", "Hemophobic", "Burning At Stake", "Lycanphiliac", "Lycanphobic", "Interesting",
		"Pest", "Night Zombie", "Finite Zombie", "Plague Reservoir", "Quarantined", "Plague Caring", "Plague Cared", "Enslaved", "Travelling", "Walker Zombie",
		"Boomer Zombie", "Protection", "Being Drained", "Empowered", "Monster Slayer", "Monster Ward", "Elf Slayer", "Elf Ward", "Human Slayer", "Human Ward",
		"Undead Slayer", "Undead Ward", "Demon Slayer", "Demon Ward", "Flying", "Enhanced Power", "Corn Fed", "Potato Fed", "Pineapple Fed", "Iceberry Fed",
		"Stocked Up", "Dirty", "Uncomfortable", "Recuperating", "Fish Fed", "Animal Fed", "Mesmerized", "Ice Prone", "Poison Resistant", "Ice Resistant",
		"Thunder Master", "Wind Master", "Fire Master", "Elemental Protection", "Stoneskin", "Sharpened", "Thorns", "Temporal", "Pyromaniac", "Enraged",
		"Ephemeral", "Favored", "Knight Party Bonus", "Newcomer", "Hidden", "Battle Cry Buff", "Endure Buff", "Blitz Buff", "Pierce Buff", "Shielded",
		"Snare Trapped", "Freezing Trapped", "Landmined", "Cleric", "Witch", "Religion Buff", "Great Witch Buff", "Devout", "Polymorphed", "Heavy",
		"Slick", "Nullchild", "Grounded", "Downcast", "Negative Nancy", "Jinxed", "Obsessed", "Cursed", "Mummified", "Aroused",
		"Anxious", "Hellspawned", "Bloated", "Food Coma", "Rabid", "Bad Weather", "Frostbite", "Danger Remnant", "Surprised Remnant", "Lightning Remnant",
		"Impregnated"
	};

	[FormerlySerializedAs("traitIconDictionary")]
	[SerializeField]
	private StringSpriteDictionary traitPortraitDictionary;

	[SerializeField]
	private StringSpriteDictionary traitIconDictionary;

	public GameObject traitIconPrefab;

	public static TraitProcessor characterTraitProcessor;

	public static TraitProcessor tileObjectTraitProcessor;

	public static TraitProcessor defaultTraitProcessor;

	private List<string> _removeStatusTraits = new List<string> { "Unconscious", "Poisoned", "Infected", "Freezing", "Frozen", "Ensnared" };

	private Dictionary<string, Trait> instancedSingletonTraits;

	public List<string> buffTraitPool { get; private set; }

	public List<string> flawTraitPool { get; private set; }

	public List<string> neutralTraitPool { get; private set; }

	public List<string> unhiddenTraitsNotStatuses { get; private set; }

	public Dictionary<string, Trait> allTraits => _allTraits;

	public List<string> removeStatusTraits => _removeStatusTraits;

	private void Awake()
	{
		Instance = this;
		CreateTraitProcessors();
	}

	public void Initialize()
	{
		instancedSingletonTraits = new Dictionary<string, Trait>();
		_allTraits = new Dictionary<string, Trait>();
		unhiddenTraitsNotStatuses = new List<string>();
		string[] files = Directory.GetFiles(Utilities.coreStreamingDataPath + "/Traits/", "*.json");
		for (int i = 0; i < files.Length; i++)
		{
			Trait trait = JsonUtility.FromJson<Trait>(File.ReadAllText(files[i]));
			if (trait.type == TRAIT_TYPE.STATUS)
			{
				trait = JsonUtility.FromJson<Status>(File.ReadAllText(files[i]));
			}
			if (!_allTraits.ContainsKey(trait.name))
			{
				_allTraits.Add(trait.name, trait);
				if (trait.type != TRAIT_TYPE.STATUS && !trait.isHidden)
				{
					unhiddenTraitsNotStatuses.Add(trait.name);
				}
			}
		}
		AddInstancedTraits();
		buffTraitPool = new List<string>();
		flawTraitPool = new List<string>();
		neutralTraitPool = new List<string>();
		string[] traitPoolForWorld = GetTraitPoolForWorld();
		foreach (string text in traitPoolForWorld)
		{
			if (allTraits.ContainsKey(text))
			{
				Trait trait2 = allTraits[text];
				if (trait2.type == TRAIT_TYPE.BUFF)
				{
					buffTraitPool.Add(text);
				}
				else if (trait2.type == TRAIT_TYPE.FLAW)
				{
					flawTraitPool.Add(text);
				}
				else
				{
					neutralTraitPool.Add(text);
				}
				continue;
			}
			throw new Exception("There is no trait named: " + text);
		}
	}

	private void AddInstancedTraits()
	{
		for (int i = 0; i < instancedTraitsAndStatuses.Length; i++)
		{
			string text = instancedTraitsAndStatuses[i];
			string text2 = Utilities.RemoveAllWhiteSpace(text);
			Trait trait = Activator.CreateInstance(Type.GetType("Traits." + text2 + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as Trait;
			if (_allTraits.ContainsKey(text))
			{
				_allTraits[text] = trait;
				continue;
			}
			_allTraits.Add(text, trait);
			if (trait.type != TRAIT_TYPE.STATUS && !trait.isHidden)
			{
				unhiddenTraitsNotStatuses.Add(text);
			}
		}
	}

	public Sprite GetTraitPortrait(string traitName)
	{
		if (!traitPortraitDictionary.ContainsKey(traitName))
		{
			return traitPortraitDictionary.Values.First();
		}
		return traitPortraitDictionary[traitName];
	}

	public Sprite GetTraitIcon(string traitName)
	{
		if (!traitIconDictionary.ContainsKey(traitName))
		{
			return traitIconDictionary.Values.First();
		}
		return traitIconDictionary[traitName];
	}

	public Trait GetTrait(string p_traitName)
	{
		if (_allTraits.ContainsKey(p_traitName))
		{
			return _allTraits[p_traitName];
		}
		return null;
	}

	public bool HasTraitIcon(string traitName)
	{
		return traitPortraitDictionary.ContainsKey(traitName);
	}

	public bool IsInstancedTrait(string traitName)
	{
		for (int i = 0; i < instancedTraitsAndStatuses.Length; i++)
		{
			if (string.Equals(instancedTraitsAndStatuses[i], traitName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTraitElemental(string traitName)
	{
		switch (traitName)
		{
		default:
			return traitName == "Frozen";
		case "Burning":
		case "Freezing":
		case "Poisoned":
		case "Wet":
		case "Zapped":
		case "Overheating":
			return true;
		}
	}

	public T CreateNewInstancedTraitClass<T>(string traitName) where T : Trait
	{
		if (instancedSingletonTraits.ContainsKey(traitName))
		{
			return instancedSingletonTraits[traitName] as T;
		}
		string text = Utilities.RemoveAllWhiteSpace(traitName);
		T val = Activator.CreateInstance(Type.GetType("Traits." + text + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as T;
		if (val.isSingleton)
		{
			instancedSingletonTraits.Add(traitName, val);
		}
		val.InitializeInstancedTrait();
		return val;
	}

	public Status GetInstancedStackingStatus(string p_statusName)
	{
		return null;
	}

	public Trait LoadTrait(SaveDataTrait saveDataTrait)
	{
		string text = Utilities.RemoveAllWhiteSpace(saveDataTrait.name);
		Trait trait = Activator.CreateInstance(Type.GetType("Traits." + text + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as Trait;
		if (trait.isSingleton)
		{
			instancedSingletonTraits.Add(saveDataTrait.name, trait);
		}
		trait.LoadFirstWaveInstancedTrait(saveDataTrait);
		if (trait.shouldBeLoadedInMainThread)
		{
			DatabaseManager.Instance.traitDatabase.AddTraitsToBeLoadedOnMainThread(trait);
		}
		return trait;
	}

	public bool CanStillTriggerFlaws(Character character)
	{
		if (character.isDead || character.faction.isPlayerFaction || GameUtilities.IsRaceBeast(character.race) || character is Summon || character.hasBeenRaisedFromDead)
		{
			return false;
		}
		return true;
	}

	public void CopyTraitOrStatus(Trait trait, ITraitable from, ITraitable to)
	{
		if (from.traitContainer.HasTrait(trait.name))
		{
			int num = 1;
			if (from.traitContainer.stacks.ContainsKey(trait.name))
			{
				num = from.traitContainer.stacks[trait.name];
			}
			Trait duplicateTrait = null;
			for (int i = 0; i < num; i++)
			{
				to.traitContainer.AddTrait(to, trait.name, out duplicateTrait, trait.responsibleCharacter, bypassElementalChance: true, 0);
				to.traitContainer.GetTraitOrStatus<Trait>(trait.name)?.SetGainedFromDoingAction(trait.gainedFromDoingType, trait.isGainedFromDoingStealth);
			}
			if (duplicateTrait == null)
			{
				return;
			}
			if (duplicateTrait is Status status && trait is Status statusToCopy)
			{
				status.OnCopyStatus(statusToCopy, from, to);
			}
			if (trait.responsibleCharacters != null && trait.responsibleCharacters.Count > 0)
			{
				for (int j = 0; j < trait.responsibleCharacters.Count; j++)
				{
					duplicateTrait.AddCharacterResponsibleForTrait(trait.responsibleCharacters[j]);
				}
			}
			if (!from.traitContainer.scheduleTickets.ContainsKey(trait.name))
			{
				return;
			}
			List<TraitRemoveSchedule> list = from.traitContainer.scheduleTickets[trait.name];
			for (int k = 0; k < list.Count; k++)
			{
				TraitRemoveSchedule traitRemoveSchedule = list[k];
				string ticket = SchedulingManager.Instance.AddEntry(traitRemoveSchedule.removeDate, delegate
				{
					to.traitContainer.RemoveTraitOnSchedule(to, duplicateTrait);
				}, to);
				to.traitContainer.AddScheduleTicket(trait.name, ticket, traitRemoveSchedule.removeDate);
			}
			return;
		}
		throw new Exception("Trying to copy trait " + trait.name + " of " + from.name + " to " + to.name + " but " + from.name + " does not have the trait!");
	}

	public void CopyStatuses(ITraitable from, ITraitable to)
	{
		for (int i = 0; i < from.traitContainer.statuses.Count; i++)
		{
			Status status = from.traitContainer.statuses[i];
			if (!to.traitContainer.HasTrait(status.name))
			{
				CopyTraitOrStatus(status, from, to);
				if (status is AbominationGerm && from.traitContainer.RemoveTrait(from, status))
				{
					i--;
				}
			}
		}
	}

	public string GetNeutralizingTraitFor(TileObject tileObject)
	{
		return tileObject.neutralizer;
	}

	private string[] GetTraitPoolForWorld()
	{
		return new string[40]
		{
			"Inspiring", "Blessed", "Diplomatic", "Fast", "Persuasive", "Optimist", "Robust", "Suspicious", "Vigilant", "Fire Resistant",
			"Music Lover", "Authoritative", "Nocturnal", "Lustful", "Chaste", "Music Hater", "Alcoholic", "Accident Prone", "Evil", "Treacherous",
			"Lazy", "Pessimist", "Unattractive", "Hothead", "Coward", "Hemophobic", "Hemophiliac", "Lycanphobic", "Lycanphiliac", "Ruthless",
			"Ice Prone", "Poison Resistant", "Ice Resistant", "Pyromaniac", "Ambitious", "Devout", "Heavy", "Slick", "Grounded", "Negative Nancy"
		};
	}

	private void CreateTraitProcessors()
	{
		characterTraitProcessor = new CharacterTraitProcessor();
		tileObjectTraitProcessor = new TileObjectTraitProcessor();
		defaultTraitProcessor = new DefaultTraitProcessor();
	}

	public void ProcessBurningTrait(ITraitable traitable, Trait trait, ref BurningSource burningSource)
	{
		if (trait is Burning burning && traitable.gridTileLocation != null)
		{
			if (burningSource == null)
			{
				burningSource = new BurningSource();
			}
			burning.SetSourceOfBurning(burningSource, traitable);
		}
	}

	public void PerformActionOnTraitables(List<ITraitable> p_traitables, Action<ITraitable> p_callback)
	{
		for (int i = 0; i < p_traitables.Count; i++)
		{
			ITraitable obj = p_traitables[i];
			p_callback(obj);
		}
	}

	public void PerformActionOnTraitables(List<ITraitable> p_traitables, Character p_actor, Action<ITraitable, Character> p_callback)
	{
		for (int i = 0; i < p_traitables.Count; i++)
		{
			ITraitable arg = p_traitables[i];
			p_callback(arg, p_actor);
		}
	}

	public void PerformActionOnTraitables(List<ITraitable> p_traitables, int p_processedDamage, float p_piercing, Action<ITraitable, int, float> p_callback)
	{
		for (int i = 0; i < p_traitables.Count; i++)
		{
			ITraitable arg = p_traitables[i];
			p_callback(arg, p_processedDamage, p_piercing);
		}
	}

	public ELEMENTAL_TYPE GetElementalTypeByTrait(string traitName)
	{
		switch (traitName)
		{
		case "Burning":
			return ELEMENTAL_TYPE.Fire;
		case "Freezing":
		case "Frozen":
			return ELEMENTAL_TYPE.Ice;
		case "Zapped":
			return ELEMENTAL_TYPE.Electric;
		case "Poisoned":
			return ELEMENTAL_TYPE.Poison;
		case "Wet":
			return ELEMENTAL_TYPE.Water;
		default:
			return ELEMENTAL_TYPE.Normal;
		}
	}

	protected override void OnDestroy()
	{
		characterTraitProcessor = null;
		tileObjectTraitProcessor = null;
		defaultTraitProcessor = null;
		_allTraits?.Clear();
		traitPortraitDictionary?.Clear();
		traitIconDictionary?.Clear();
		buffTraitPool?.Clear();
		buffTraitPool = null;
		flawTraitPool?.Clear();
		flawTraitPool = null;
		neutralTraitPool?.Clear();
		neutralTraitPool = null;
		_removeStatusTraits?.Clear();
		_removeStatusTraits = null;
		instancedSingletonTraits?.Clear();
		instancedSingletonTraits = null;
		base.OnDestroy();
		Instance = null;
	}

	public string GetLocalizedNameOfTrait(string p_traitName)
	{
		if (_allTraits.ContainsKey(p_traitName))
		{
			return _allTraits[p_traitName].localizedName;
		}
		return string.Empty;
	}
}
