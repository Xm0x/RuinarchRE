using System.Collections.Generic;

public static class GoapActionStateDB
{
	public static string No_Icon = "None";

	public static string Eat_Icon = "Eat";

	public static string Hostile_Icon = "Hostile";

	public static string Sleep_Icon = "Sleep";

	public static string Social_Icon = "Social";

	public static string Work_Icon = "Work";

	public static string Drink_Icon = "Drink";

	public static string Entertain_Icon = "Entertain";

	public static string Explore_Icon = "Explore";

	public static string FirstAid_Icon = "First Aid";

	public static string Flee_Icon = "Flee";

	public static string Patrol_Icon = "Patrol";

	public static string Watch_Icon = "Watch";

	public static string Anger_Icon = "Anger";

	public static string Approval_Icon = "Approval";

	public static string Build_Icon = "Build";

	public static string Bury_Icon = "Bury";

	public static string Chop_Icon = "Chop";

	public static string Clean_Icon = "Clean";

	public static string Cowering_Icon = "Cowering";

	public static string Cure_Icon = "Cure";

	public static string Douse_Icon = "Douse";

	public static string Harvest_Icon = "Harvest";

	public static string Haul_Icon = "Haul";

	public static string Magic_Icon = "Magic";

	public static string Mine_Icon = "Mine";

	public static string Mock_Icon = "Mock";

	public static string Repair_Icon = "Repair";

	public static string Sad_Icon = "Sad";

	public static string Shock_Icon = "Shock";

	public static string Sick_Icon = "Sick";

	public static string Drink_Blood_Icon = "Drink Blood";

	public static string Flirt_Icon = "Flirt";

	public static string Pray_Icon = "Pray";

	public static string Restrain_Icon = "Restrain";

	public static string Steal_Icon = "Steal";

	public static string Stealth_Icon = "Stealth";

	public static string Joy_Icon = "Joy";

	public static string Fish_Icon = "Fish";

	public static string Happy_Icon = "Happy";

	public static string Inspect_Icon = "Inspect";

	public static string Party_Icon = "Party";

	public static string Heartbroken_Icon = "Heartbroken";

	public static string Injured_Icon = "Injured";

	public static string Gossip_Icon = "Gossip";

	public static string Blueprint_Icon = "Blueprint";

	public static string Cult_Icon = "Cult";

	public static string Butcher_Icon = "Butcher";

	public static string Trap_Icon = "Trap";

	public static string Lycan_Icon = "Lycan";

	public static string Poison_Icon = "Poison";

	public static string Report_Icon = "Report";

	public static string Found_Icon = "Found";

	public static string Divine_Icon = "Divine";

	public static string Burn_Icon = "Burn";

	public static string Train_Icon = "Train";

	public static string Vampire_Turn_Icon = "Vampire Turn";

	public static string Change_Icon = "Change";

	public static string Agree_Icon = "Agree";

	public static string Abduct_Icon = "Abduct";

	public static string Argue_Icon = "Argue";

	public static string Death_Icon = "Death";

	public static string Question_Icon = "Question";

	public static string Daydream_Icon = "Daydream";

	public static string Judge_Icon = "Judge";

	public static string Read_Icon = "Read";

	public static string Sing_Icon = "Sing";

	public static int BuildBlueprintDuration = 20;

	public static readonly Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>
	{
		{
			INTERACTION_TYPE.EAT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Eat Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RELEASE_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Release Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.ASSAULT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Combat Start",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.MINE_STONE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Mine Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.SLEEP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Rest Success",
					status = "Success",
					duration = 480
				}
			}
		},
		{
			INTERACTION_TYPE.SLEEP_OUTSIDE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Rest Success",
					status = "Success",
					duration = 120,
					animationName = "Sleep"
				}
			}
		},
		{
			INTERACTION_TYPE.PICK_UP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DAYDREAM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Daydream Success",
					status = "Success",
					duration = 30
				}
			}
		},
		{
			INTERACTION_TYPE.PLAY_GUITAR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Play Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RETURN_HOME,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Return Home Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DRINK,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drink Success",
					status = "Success",
					duration = 30
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_POISON,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_TRAP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_FREEZING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_UNCONSCIOUS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_RESTRAINED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.POISON,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Poison Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.PRAY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Pray Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CHOP_WOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Chop Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.STEAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Steal Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.PICKPOCKET,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Pickpocket Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.STEAL_COINS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Steal Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Deposit Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DROP_RESOURCE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.TAKE_RESOURCE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.RESTRAIN_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Restrain Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.FIRST_AID_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "First Aid Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.CURE_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cure Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.JUDGE_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Judge Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.FEED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Feed Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.STAND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Stand Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.STAND_STILL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Stand Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.SIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sit Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.NAP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Nap Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BURY_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Bury Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.REMEMBER_FALLEN,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remember Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.SPIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Spit Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.INVITE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Invite Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.MAKE_LOVE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Make Love Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.DRINK_BLOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drink Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.SHARE_INFORMATION,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Share Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.REPORT_CRIME,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Report Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.INSPECT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Inspect Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CARRY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Carry Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DROP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CARRY_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Carry Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DROP_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.KNOCKOUT_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Knockout Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.RITUAL_KILLING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Killing Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.BUTCHER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Transform Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.WELL_JUMP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Well Jump Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.STRANGLE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Strangle Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.REPAIR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Repair Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CRY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cry Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CRAFT_TILE_OBJECT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Craft Success",
					status = "Success",
					duration = -1
				}
			}
		},
		{
			INTERACTION_TYPE.HAVE_AFFAIR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Affair Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.GO_TO,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Goto Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.SING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sing Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.DANCE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Dance Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.SCREAM_FOR_HELP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Scream Success",
					status = "Success",
					duration = GameManager.Instance.GetTicksBasedOnMinutes(9)
				}
			}
		},
		{
			INTERACTION_TYPE.REACT_TO_SCREAM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "React Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.RESOLVE_COMBAT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Combat Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CHANGE_CLASS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Change Class Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.VISIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Visit Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.PLACE_BLUEPRINT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Place Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_BLUEPRINT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = -1
				}
			}
		},
		{
			INTERACTION_TYPE.STEALTH_TRANSFORM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Transform Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.HARVEST_PLANT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Harvest Success",
					status = "Success",
					duration = 4
				}
			}
		},
		{
			INTERACTION_TYPE.REPAIR_STRUCTURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Repair Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.NEUTRALIZE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Neutralize Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.ROAM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Roam Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.DROP_ITEM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_HEALING_POTION,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_ANTIDOTE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_POISON_FLASK,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.EXTRACT_ITEM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Extract Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BOOBY_TRAP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Trap Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Report Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DOUSE_FIRE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Douse Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.HEAL_SELF,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Heal Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.OPEN,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Open Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.EXILE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Exile Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.EXECUTE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Execute Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.ABSOLVE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Absolve Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.WHIP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Whip Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.TEND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Tend Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.START_DOUSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Start Douse Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.START_CLEANSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Start Cleanse Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.START_DRY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Start Dry Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.START_PATROL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Start Patrol Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CLEANSE_TILE,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Cleanse Success",
					status = "Success",
					duration = 2
				},
				new StateNameAndDuration
				{
					name = "Ice Cleanse Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CLEAN_UP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Clean Success",
					status = "Success",
					duration = 2
				}
			}
		},
		{
			INTERACTION_TYPE.PATROL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Patrol Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DIG,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Dig Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_LAIR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.SPAWN_SKELETON,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Spawn Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RAISE_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Raise Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.PLACE_FREEZING_TRAP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Place Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.PLACE_SNARE_TRAP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Place Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.EAT_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Eat Success",
					status = "Success",
					duration = 6
				}
			}
		},
		{
			INTERACTION_TYPE.READ_NECRONOMICON,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Read Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.MEDITATE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Meditate Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.REGAIN_ENERGY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Regain Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.MURDER,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Murder Success",
					status = "Success",
					duration = 3
				},
				new StateNameAndDuration
				{
					name = "Slay Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.EAT_ALIVE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Eat Alive Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_BUFF,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Buff Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_CULTIST_KIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.IS_CULTIST,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cultist Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.SPAWN_POISON_CLOUD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Spawn Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.DECREASE_MOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Decrease Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.GO_TO_TILE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Go Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.GO_TO_SPECIFIC_TILE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Go Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DISABLE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Disable Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.LAY_EGG,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Lay Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.BURN,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Burn Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.TAKE_SHELTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Take Shelter Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.IS_PLAGUED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Plague Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DARK_RITUAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Ritual Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RECONCILIATION_RITUAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Ritual Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.GUARDIAN_RITUAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Ritual Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.GOD_DAY_RITUAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Ritual Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.DRAW_MAGIC_CIRCLE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Draw Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.JOIN_GATHERING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Join Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.MONSTER_INVADE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Invade Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DISGUISE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Disguise Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.RECRUIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Recruit Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.COOK,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cook Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_TROLL_CAULDRON,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.FLEE_CRIME,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Flee Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.PLAY_CARDS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Play Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_WOLF_LAIR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_CAMPFIRE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.WARM_UP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Warm Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.TRESPASSING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Trespass Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.EVANGELIZE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Evangelize Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.LIBERATE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Liberate Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.REMOVE_ENSNARED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Remove Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.VAMPIRIC_EMBRACE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Embrace Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.BURN_AT_STAKE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Burn Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.IS_VAMPIRE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Vampire Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.FEED_SELF,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Feed Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CARRY_RESTRAINED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Carry Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DROP_RESTRAINED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_NEW_VILLAGE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.IS_WEREWOLF,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Werewolf Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DISPEL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Dispel Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.SUMMON_BONE_GOLEM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Summon Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BIRTH_RATMAN,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Birth Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.TORTURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Torture Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CARRY_PATIENT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Carry Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.QUARANTINE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Quarantine Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.START_PLAGUE_CARE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Care Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CARE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Care Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.LONG_STAND_STILL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Stand Success",
					status = "Success",
					duration = GameManager.Instance.GetTicksBasedOnHour(8)
				}
			}
		},
		{
			INTERACTION_TYPE.BURROW,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Burrow Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.DISPOSE_FOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Dispose Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.IS_IMPRISONED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Imprisoned Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.STEAL_ANYTHING,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Steal Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.ABSORB_POWER_CRYSTAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Absorb Crystal Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.MINE_ORE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Mine Ore Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.FIND_FISH,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Find Fish Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.TILL_TILE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Till Tile Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.SHEAR_ANIMAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Shear Animal Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.SKIN_ANIMAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Skin Animal Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.HARVEST_CROPS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Harvest Crops Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RECUPERATE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Recuperate Success",
					status = "Success",
					duration = 480
				}
			}
		},
		{
			INTERACTION_TYPE.HEALER_CURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Healer Cure Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.GATHER_HERB,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Gather Herb Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_WORKPLACE_POTION,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Workplace Potion Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Hospice Antidote Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BUY_FOOD,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Buy Success",
					status = "Success",
					duration = 0
				},
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.STOCKPILE_FOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Stockpile Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CRAFT_FURNITURE_WOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Craft Success",
					status = "Success",
					duration = -1
				}
			}
		},
		{
			INTERACTION_TYPE.BUY_WOOD,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Buy Success",
					status = "Success",
					duration = 0
				},
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CRAFT_FURNITURE_STONE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Craft Success",
					status = "Success",
					duration = -1
				}
			}
		},
		{
			INTERACTION_TYPE.BUY_STONE,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Buy Success",
					status = "Success",
					duration = 0
				},
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BUY_ITEM,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Buy Success",
					status = "Success",
					duration = 0
				},
				new StateNameAndDuration
				{
					name = "Take Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CRAFT_EQUIPMENT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Craft Equipment Success",
					status = "Success",
					duration = -1
				}
			}
		},
		{
			INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drop Resource To Work Structure Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DRINK_WATER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Drink Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CAST_MESMERIZED,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cast Mesmerized Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.SPAWN_GHOST,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Spawn Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.ABOMINATION_GERM_MUSHROOM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Produce Success",
					status = "Success",
					duration = 2
				}
			}
		},
		{
			INTERACTION_TYPE.STEAL_TRAIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Steal Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.GIVE_TRAIT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Give Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Order Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.TRANSFORM_CENTAUR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Transform Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.ABSORB_WISP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Absorb Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.REPORT_MURDER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Report Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.REPORT_ABDUCT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Report Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.TRAIN_COMBAT_MAGIC,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Train Combat Magic Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.STUDY_MAGIC,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Study Magic Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.TRAIN_HEALING_MAGIC,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Train Healing Magic Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.FOLLOW_ACTION,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Follow Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.ABDUCT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Abduct Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.DEVASTATION_RITUAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Ritual Success",
					status = "Success",
					duration = 80
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_BANDIT_CAMP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.KICK_OUT_OF_HOSPICE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Kick Out Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.SACRIFICE_SELF,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sacrifice Success",
					status = "Success",
					duration = 4
				}
			}
		},
		{
			INTERACTION_TYPE.ENHANCE_RELATIONSHIP,
			new StateNameAndDuration[2]
			{
				new StateNameAndDuration
				{
					name = "Enhance Success",
					status = "Success",
					duration = 0
				},
				new StateNameAndDuration
				{
					name = "Enhance Fail",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Train Physical Combat Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.DEMON_STEAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Steal Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.EAT_INVENTORY_ITEM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Eat Success",
					status = "Success",
					duration = 4
				}
			}
		},
		{
			INTERACTION_TYPE.PACK_FOOD,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Pack Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.IS_CANNIBAL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cannibal Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.TEND_WYVERN_COOP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Tend Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.PURIFY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Purify Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.PILGRIMAGE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Pilgrimage Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.CLAIM_HALLOWED_GROUND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Claim Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.CLEANSE_HALLOWED_GROUND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Cleanse Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.SACRIFICE_FOR_CHAOS_ORBS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sacrifice Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.READ_STRUCTURE_SCROLL,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Read Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.CLAIM_LEGENDARY_FORGE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Claim Success",
					status = "Success",
					duration = 1
				}
			}
		},
		{
			INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Craft Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.MUMMIFY,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Mummify Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.CLEAN_MUMMIFIED_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Clean Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.ADORE_MUMMIFIED_CORPSE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Adore Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.CHECK_OUT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Check Success",
					status = "Success",
					duration = 5
				}
			}
		},
		{
			INTERACTION_TYPE.SMELL_HAIR,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Smell Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.WATCH_SLEEP,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Watch Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.SNIFF_CLOTHES,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sniff Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.LICK_TILE_OBJECT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Lick Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.RUB_TILE_OBJECT,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Rub Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.SEDUCE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Seduce Success",
					status = "Success",
					duration = 2
				}
			}
		},
		{
			INTERACTION_TYPE.DESTROY_HOME,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Destroy Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.SUMMON_EPHEMERAL_BEASTS,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Summon Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.SEARCH_FOR_DEMONIC_AREA,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Search Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Build Success",
					status = "Success",
					duration = 20
				}
			}
		},
		{
			INTERACTION_TYPE.START_PURIFYING_GROUND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Start Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.PURIFY_GROUND,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Purify Success",
					status = "Success",
					duration = 2
				}
			}
		},
		{
			INTERACTION_TYPE.IS_CAPTIVE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Captive Success",
					status = "Success",
					duration = 0
				}
			}
		},
		{
			INTERACTION_TYPE.CREATE_GOLEM,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Create Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.BLOOD_SACRIFICE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Sacrifice Success",
					status = "Success",
					duration = 10
				}
			}
		},
		{
			INTERACTION_TYPE.SLAY_CHARACTER,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Slay Success",
					status = "Success",
					duration = 3
				}
			}
		},
		{
			INTERACTION_TYPE.IMPREGNATE,
			new StateNameAndDuration[1]
			{
				new StateNameAndDuration
				{
					name = "Impregnate Success",
					status = "Success",
					duration = 10
				}
			}
		}
	};

	public static string GetStateResult(INTERACTION_TYPE goapType, string stateName)
	{
		if (goapActionStates.ContainsKey(goapType))
		{
			StateNameAndDuration[] array = goapActionStates[goapType];
			for (int i = 0; i < array.Length; i++)
			{
				StateNameAndDuration stateNameAndDuration = array[i];
				if (stateNameAndDuration.name == stateName)
				{
					return stateNameAndDuration.status;
				}
			}
		}
		return string.Empty;
	}

	public static StateNameAndDuration[] GetActionStates(INTERACTION_TYPE goapType)
	{
		if (goapActionStates.ContainsKey(goapType))
		{
			return goapActionStates[goapType];
		}
		return null;
	}
}
