using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Behaviour;
using UnityEngine;

public class CharacterBehaviourManager : MonoBehaviour
{
	public const string Default_Resident_Behaviour = "Default Resident Behaviour";

	public const string Default_Monster_Behaviour = "Default Monster Behaviour";

	public const string Default_Minion_Behaviour = "Default Minion Behaviour";

	public const string Default_Wanderer_Behaviour = "Default Wanderer Behaviour";

	public const string Default_Angel_Behaviour = "Default Angel Behaviour";

	public const string Noxious_Wanderer_Behaviour = "Noxious Wanderer Behaviour";

	public const string DeMooder_Behaviour = "DeMooder Behaviour";

	public const string Defender_Behaviour = "Defender Behaviour";

	public const string Invader_Behaviour = "Invader Behaviour";

	public const string Disabler_Behaviour = "Disabler Behaviour";

	public const string Infestor_Behaviour = "Infestor Behaviour";

	public const string Abductor_Behaviour = "Abductor Behaviour";

	public const string Arsonist_Behaviour = "Arsonist Behaviour";

	public const string Baby_Infestor_Behaviour = "Baby Infestor Behaviour";

	public const string Tower_Behaviour = "Tower Behaviour";

	public const string Snatcher_Behaviour = "Snatcher Behaviour";

	public const string Pest_Behaviour = "Pest Behaviour";

	public const string Enslaved_Behaviour = "Enslaved Behaviour";

	public const string Pet_Behaviour = "Pet Behaviour";

	public const string Structure_Protector_Behaviour = "Structure Protector Behaviour";

	public const string Bandit_Behaviour = "Bandit Behaviour";

	public const string Settlement_Protector_Behaviour = "Settlement Protector Behaviour";

	public const string Abomination_Behaviour = "Abomination Behaviour";

	public const string Small_Spider_Behaviour = "Small Spider Behaviour";

	public const string Golem_Behaviour = "Golem Behaviour";

	public const string Ravager_Behaviour = "Ravager Behaviour";

	public const string Dire_Wolf_Behaviour = "Dire Wolf Behaviour";

	public const string Kobold_Behaviour = "Kobold Behaviour";

	public const string Giant_Spider_Behaviour = "Giant Spider Behaviour";

	public const string Vengeful_Ghost_Behaviour = "Vengeful Ghost Behaviour";

	public const string Ghost_Behaviour = "Ghost Behaviour";

	public const string Wurm_Behaviour = "Wurm Behaviour";

	public const string Revenant_Behaviour = "Revenant Behaviour";

	public const string Ent_Behaviour = "Ent Behaviour";

	public const string Mimic_Behaviour = "Mimic Behaviour";

	public const string Succubus_Behaviour = "Succubus Behaviour";

	public const string Dragon_Behaviour = "Dragon Behaviour";

	public const string Troll_Behaviour = "Troll Behaviour";

	public const string Bone_Golem_Behaviour = "Bone Golem Behaviour";

	public const string Rat_Behaviour = "Rat Behaviour";

	public const string Ratman_Behaviour = "Ratman Behaviour";

	public const string Fire_Elemental_Behaviour = "Fire Elemental Behaviour";

	public const string Sludge_Behaviour = "Sludge Behaviour";

	public const string Scorpion_Behaviour = "Scorpion Behaviour";

	public const string Harpy_Behaviour = "Harpy Behaviour";

	public const string Triton_Behaviour = "Triton Behaviour";

	public const string Incubus_Behaviour = "Incubus Behaviour";

	public const string Mothman_Behaviour = "Mothman Behaviour";

	public const string Broodmother_Behaviour = "Broodmother Behaviour";

	public const string Ghoul_Behaviour = "Ghoul Behaviour";

	public const string Centaur_Behaviour = "Centaur Behaviour";

	public const string Orc_Behaviour = "Orc Behaviour";

	public const string Whisperer_Behaviour = "Whisperer Behaviour";

	public const string Tarantula_Behaviour = "Tarantula Behaviour";

	public const string Fallen_Angel_Behaviour = "Fallen Angel Behaviour";

	public const string Nature_Spirit_Behaviour = "Nature Spirit Behaviour";

	public const string Unicorn_Behaviour = "Unicorn Behaviour";

	public const string Gorgon_Behaviour = "Gorgon Behaviour";

	public const string Splatter_Behaviour = "Splatter Behaviour";

	public const string Goblin_Behaviour = "Goblin Behaviour";

	public const string Dwarf_King_Behaviour = "Dwarf King Behaviour";

	public const string Dwarf_Paladin_Behaviour = "Dwarf Paladin Behaviour";

	private Dictionary<Type, CharacterBehaviour> behaviourComponents;

	private readonly Dictionary<string, Type[]> defaultBehaviourSets = new Dictionary<string, Type[]>
	{
		{
			"Default Resident Behaviour",
			new Type[10]
			{
				typeof(DefaultFactionRelated),
				typeof(DefaultHomeless),
				typeof(WorkBehaviour),
				typeof(SleepBehaviour),
				typeof(FreeTimeBehaviour),
				typeof(DefaultOutside),
				typeof(DefaultBaseStructure),
				typeof(DefaultOtherStructure),
				typeof(DefaultExtraCatcher),
				typeof(MovementProcessing)
			}
		},
		{
			"Default Monster Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Default Minion Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DefaultMinion),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Default Wanderer Behaviour",
			new Type[5]
			{
				typeof(MovementProcessing),
				typeof(DefaultFactionRelated),
				typeof(DefaultHomeless),
				typeof(DefaultWanderer),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Default Angel Behaviour",
			new Type[2]
			{
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Ravager Behaviour",
			new Type[4]
			{
				typeof(WolfBehaviour),
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Dire Wolf Behaviour",
			new Type[4]
			{
				typeof(WolfBehaviour),
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Kobold Behaviour",
			new Type[4]
			{
				typeof(KoboldBehaviour),
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Giant Spider Behaviour",
			new Type[4]
			{
				typeof(GiantSpiderBehaviour),
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Noxious Wanderer Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(NoxiousWandererBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"DeMooder Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DeMooderBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Defender Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DefendBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Invader Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(InvadeBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Disabler Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DisablerBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Infestor Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(InfestorBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Abductor Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(AbductorBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Arsonist Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(ArsonistBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Abomination Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(AbominationBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Small Spider Behaviour",
			new Type[4]
			{
				typeof(SmallSpiderBehaviour),
				typeof(MovementProcessing),
				typeof(DefaultMonster),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Golem Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(GolemBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Vengeful Ghost Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(VengefulGhostBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Wurm Behaviour",
			new Type[1] { typeof(WurmBehaviour) }
		},
		{
			"Tower Behaviour",
			new Type[1] { typeof(TowerBehaviour) }
		},
		{
			"Ghost Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(GhostBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Revenant Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(RevenantBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Ent Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(EntBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Mimic Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(MimicBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Succubus Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SuccubusBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Incubus Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(IncubusBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Dragon Behaviour",
			new Type[1] { typeof(DragonBehaviour) }
		},
		{
			"Troll Behaviour",
			new Type[1] { typeof(TrollBehaviour) }
		},
		{
			"Baby Infestor Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(BabyInfestorBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Snatcher Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SnatcherBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Bone Golem Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(BoneGolemBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Pest Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(PestBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Rat Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(RatBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Ratman Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(RatmanBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Enslaved Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SlaveBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Fire Elemental Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(FireElementalBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Sludge Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SludgeBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Scorpion Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(ScorpionBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Harpy Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(HarpyBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Triton Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(TritonBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Mothman Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(MothmanBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Broodmother Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(BroodmotherBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Ghoul Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(GhoulBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Centaur Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(CentaurBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Orc Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(OrcBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Whisperer Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(WhispererBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Tarantula Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(TarantulaBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Pet Behaviour",
			new Type[2]
			{
				typeof(MovementProcessing),
				typeof(PetBehaviour)
			}
		},
		{
			"Structure Protector Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(StructureProtectorBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Settlement Protector Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SettlementProtectorBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Bandit Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(BanditBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Fallen Angel Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(FallenAngelBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Nature Spirit Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(NatureSpiritBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Unicorn Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(UnicornBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Gorgon Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(GorgonBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Splatter Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(SplatterBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Goblin Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(GoblinBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Dwarf King Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DwarfKingBehaviour),
				typeof(DefaultExtraCatcher)
			}
		},
		{
			"Dwarf Paladin Behaviour",
			new Type[3]
			{
				typeof(MovementProcessing),
				typeof(DwarfPaladinBehaviour),
				typeof(DefaultExtraCatcher)
			}
		}
	};

	private string[] traitDefaultBehaviourSets = new string[13]
	{
		"Abductor", "Arsonist", "Baby Infestor", "Defender", "DeMooder", "Disabler", "Enslaved", "Infestor", "Invader", "Noxious Wanderer",
		"Pest", "Snatcher", "Tower"
	};

	public void Initialize()
	{
		ConstructCharacterBehaviours();
	}

	private void ConstructCharacterBehaviours()
	{
		List<CharacterBehaviour> list = ReflectiveEnumerator.GetEnumerableOfType<CharacterBehaviour>(Array.Empty<object>()).ToList();
		behaviourComponents = new Dictionary<Type, CharacterBehaviour>();
		for (int i = 0; i < list.Count; i++)
		{
			CharacterBehaviour characterBehaviour = list[i];
			behaviourComponents.Add(characterBehaviour.GetType(), characterBehaviour);
		}
	}

	public Type[] GetDefaultBehaviourSet(string setName)
	{
		if (defaultBehaviourSets.ContainsKey(setName))
		{
			return defaultBehaviourSets[setName];
		}
		return null;
	}

	public bool HasDefaultBehaviourSet(string setName)
	{
		return defaultBehaviourSets.ContainsKey(setName);
	}

	public CharacterBehaviour GetCharacterBehaviourComponent(Type type)
	{
		if (behaviourComponents.ContainsKey(type))
		{
			return behaviourComponents[type];
		}
		return null;
	}

	public string GetTraitBehaviourSetOf(Character p_character)
	{
		for (int i = 0; i < traitDefaultBehaviourSets.Length; i++)
		{
			string text = traitDefaultBehaviourSets[i];
			if (p_character.traitContainer.HasTrait(text))
			{
				return text + " Behaviour";
			}
		}
		return string.Empty;
	}

	public T GetCharacterBehaviourComponent<T>(Type type) where T : CharacterBehaviour
	{
		if (behaviourComponents.ContainsKey(type) && behaviourComponents[type] is T result)
		{
			return result;
		}
		return null;
	}
}
