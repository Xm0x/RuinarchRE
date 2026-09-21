using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Localization;
using UtilityScripts;

public class CharacterClass
{
	private string _className;

	private string _displayName;

	private string _displayDescription;

	private int _baseAttackPower;

	private int _baseHP;

	private int _baseAttackSpeed;

	private int _inventoryCapacity;

	private float _attackRange;

	private float _staminaReduction;

	private bool _lungeOnMeleeAttack;

	private bool _showHair;

	private ELEMENTAL_TYPE _elementalType;

	private ATTACK_TYPE _attackType;

	private RANGE_TYPE _rangeType;

	private Dictionary<RACE, CharacterSpritesPerAnimation> _animationSprites;

	private string _characterPortraitSpriteKeyID;

	private string _smallPortraitSpriteKeyID;

	private CharacterClassData _constantData;

	public string displayName
	{
		get
		{
			if (string.IsNullOrEmpty(_displayName) || string.IsNullOrWhiteSpace(_displayName))
			{
				_displayName = LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", className);
			}
			return _displayName;
		}
	}

	public string displayDescription
	{
		get
		{
			if (string.IsNullOrEmpty(_displayDescription) || string.IsNullOrWhiteSpace(_displayDescription))
			{
				_displayDescription = LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", className + "_Description");
			}
			return _displayDescription;
		}
	}

	public string className => _className;

	public int baseAttackPower => _baseAttackPower;

	public int baseHP => _baseHP;

	public int baseAttackSpeed => _baseAttackSpeed;

	[Obsolete("Use Character.CombatComponent.attackRange instead! This should only be used by that getter too.")]
	public float attackRange => _attackRange;

	public float staminaReduction => _staminaReduction;

	public int inventoryCapacity => _inventoryCapacity;

	public ELEMENTAL_TYPE elementalType => _elementalType;

	public ATTACK_TYPE attackType => _attackType;

	[Obsolete("Use Character.CombatComponent.rangeType instead! This should only be used by that getter too.")]
	public RANGE_TYPE rangeType => _rangeType;

	public bool lungeOnMeleeAttack => _lungeOnMeleeAttack;

	public bool showHair => _showHair;

	public Dictionary<RACE, CharacterSpritesPerAnimation> animationSprites => _animationSprites;

	public CHARACTER_COMBAT_BEHAVIOUR combatBehaviourType => _constantData.combatBehaviourType;

	public COMBAT_SPECIAL_SKILL combatSpecialSkillType => _constantData.combatSpecialSkillType;

	public STRUCTURE_TYPE workStructureType => _constantData.workStructureType;

	public int summonCost => _constantData.summonCost;

	public int monsterSpawnerDamageOnSpawn => _constantData.monsterSpawnerDamageOnSpawn;

	public string identifier => _constantData.identifier;

	public bool isVillagerType => _constantData.isVillagerType;

	public bool cantBeAgitated => _constantData.cantBeAgitated;

	public string[] traitNames => _constantData.traitNames;

	public string[] interestedItemNames => _constantData.interestedItemNames;

	public JOB_TYPE[] ableJobs => _constantData.ableJobs;

	public Sprite portraitSprite => GetCharacterPortraitSprite();

	public Sprite smallPortraitSprite => GetSmallPortraitSprite();

	public float initialVillagerPiercing => _constantData.initialVillagerPiercing;

	public float[] initialVillagerPhysicalResistances => _constantData.initialVillagerPhysicalResistances;

	public float[] initialVillagerMentalResistances => _constantData.initialVillagerMentalResistances;

	public float[] initialVillagerElementalResistances => _constantData.initialVillagerElementalResistances;

	public float[] initialVillagerSecondaryResistances => _constantData.initialVillagerSecondaryResistances;

	public List<TILE_OBJECT_TYPE> craftableWeapons => _constantData.craftableWeapons;

	public List<TILE_OBJECT_TYPE> craftableArmors => _constantData.craftableArmors;

	public List<TILE_OBJECT_TYPE> craftableAccessories => _constantData.craftableAccessories;

	public AK.Wwise.Event uiSFX => _constantData.uiSFX;

	public CharacterProgressionBonusData characterSkillUpdateData => _constantData.characterSkillUpdateData;

	public CharacterClass()
	{
		BaseCharacterClass();
	}

	public CharacterClass(string p_className, XmlNode p_xmlNode)
	{
		SetData(p_className, p_xmlNode, Application.streamingAssetsPath);
		BaseCharacterClass();
	}

	public void OnLocaleChanged(Locale p_obj)
	{
		_displayName = string.Empty;
		_displayDescription = string.Empty;
	}

	public void SetData(string p_className, XmlNode p_xmlNode, string p_folderPath)
	{
		_className = p_className;
		if (p_xmlNode["Name"] != null)
		{
			_displayName = p_xmlNode["Name"].InnerText;
		}
		if (p_xmlNode["Description"] != null)
		{
			_displayDescription = p_xmlNode["Description"].InnerText;
		}
		if (p_xmlNode["BaseAttack"] != null)
		{
			_baseAttackPower = int.Parse(p_xmlNode["BaseAttack"].InnerText);
		}
		if (p_xmlNode["BaseHP"] != null)
		{
			_baseHP = int.Parse(p_xmlNode["BaseHP"].InnerText);
		}
		if (p_xmlNode["BaseAttackSpeed"] != null)
		{
			_baseAttackSpeed = int.Parse(p_xmlNode["BaseAttackSpeed"].InnerText);
		}
		if (p_xmlNode["InventoryCapacity"] != null)
		{
			_inventoryCapacity = int.Parse(p_xmlNode["InventoryCapacity"].InnerText);
		}
		if (p_xmlNode["BaseAttackRange"] != null)
		{
			_attackRange = float.Parse(p_xmlNode["BaseAttackRange"].InnerText);
		}
		if (p_xmlNode["StaminaReduction"] != null)
		{
			_staminaReduction = float.Parse(p_xmlNode["StaminaReduction"].InnerText);
		}
		if (p_xmlNode["ElementType"] != null)
		{
			int length = Enum.GetValues(typeof(ELEMENTAL_TYPE)).Length;
			int num = int.Parse(p_xmlNode["ElementType"].InnerText);
			_elementalType = ((num >= 0 && num < length) ? ((ELEMENTAL_TYPE)num) : ELEMENTAL_TYPE.Normal);
		}
		if (p_xmlNode["AttackType"] != null)
		{
			int length2 = Enum.GetValues(typeof(ATTACK_TYPE)).Length;
			int num2 = int.Parse(p_xmlNode["AttackType"].InnerText);
			_attackType = ((num2 >= 0 && num2 < length2) ? ((ATTACK_TYPE)num2) : ATTACK_TYPE.PHYSICAL);
		}
		if (p_xmlNode["RangeType"] != null)
		{
			int length3 = Enum.GetValues(typeof(RANGE_TYPE)).Length;
			int num3 = int.Parse(p_xmlNode["RangeType"].InnerText);
			_rangeType = ((num3 >= 0 && num3 < length3) ? ((RANGE_TYPE)num3) : RANGE_TYPE.MELEE);
		}
		if (p_xmlNode["LungeOnMeleeAttack"] != null)
		{
			_lungeOnMeleeAttack = bool.Parse(p_xmlNode["LungeOnMeleeAttack"].InnerText);
		}
		if (p_xmlNode["ShowHair"] != null)
		{
			_showHair = bool.Parse(p_xmlNode["ShowHair"].InnerText);
		}
		if (p_xmlNode["CharacterPortrait"] != null)
		{
			string value = p_xmlNode["CharacterPortrait"].Attributes["Path"].Value;
			string text = Path.Combine(p_folderPath, value);
			if (File.Exists(text))
			{
				_characterPortraitSpriteKeyID = _className + "_CharacterPortrait";
				TextureAtlas2D characterSpritesAtlas = TextureManager.Instance.characterSpritesAtlas;
				if (characterSpritesAtlas.HasTextureID(_characterPortraitSpriteKeyID))
				{
					characterSpritesAtlas.GetPackedTextureByKey(_characterPortraitSpriteKeyID).SetPath(text);
				}
				else
				{
					TextureManager.Instance.characterSpritesAtlas.AddTexturePathToAtlas(_characterPortraitSpriteKeyID, text, 100);
				}
			}
		}
		if (p_xmlNode["SmallPortrait"] != null)
		{
			string value2 = p_xmlNode["SmallPortrait"].Attributes["Path"].Value;
			string text2 = Path.Combine(p_folderPath, value2);
			if (File.Exists(text2))
			{
				_smallPortraitSpriteKeyID = _className + "SmallPortrait";
				TextureAtlas2D characterSpritesAtlas2 = TextureManager.Instance.characterSpritesAtlas;
				if (characterSpritesAtlas2.HasTextureID(_smallPortraitSpriteKeyID))
				{
					characterSpritesAtlas2.GetPackedTextureByKey(_smallPortraitSpriteKeyID).SetPath(text2);
				}
				else
				{
					TextureManager.Instance.characterSpritesAtlas.AddTexturePathToAtlas(_smallPortraitSpriteKeyID, text2, 100);
				}
			}
		}
		if (_animationSprites == null)
		{
			_animationSprites = new Dictionary<RACE, CharacterSpritesPerAnimation>();
		}
		XmlNodeList xmlNodeList = p_xmlNode.SelectNodes("descendant::AnimationSprites");
		if (xmlNodeList == null)
		{
			return;
		}
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			RACE rACE = (RACE)int.Parse(xmlNode.Attributes["Race"].Value);
			string text3 = rACE.ToStringEnumWithSpaceNormalized();
			if (rACE == RACE.NONE)
			{
				text3 = "Default";
			}
			if (!_animationSprites.ContainsKey(rACE))
			{
				_animationSprites.Add(rACE, new CharacterSpritesPerAnimation());
			}
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("descendant::Animation");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			for (int j = 0; j < xmlNodeList2.Count; j++)
			{
				XmlNode xmlNode2 = xmlNodeList2[j];
				string value3 = xmlNode2.Attributes["ID"].Value;
				XmlNodeList xmlNodeList3 = xmlNode2.SelectNodes("descendant::ImgSprite");
				if (xmlNodeList3 == null)
				{
					continue;
				}
				for (int k = 0; k < xmlNodeList3.Count; k++)
				{
					XmlNode xmlNode3 = xmlNodeList3[k];
					string value4 = xmlNode3.Attributes["Path"].Value;
					string text4 = Path.Combine(p_folderPath, value4);
					if (File.Exists(text4))
					{
						string value5 = xmlNode3.Attributes["ID"].Value;
						string text5 = _className + "_" + text3 + "_" + value5;
						_animationSprites[rACE].Add(value3, value5, text5);
						TextureAtlas2D characterSpritesAtlas3 = TextureManager.Instance.characterSpritesAtlas;
						if (characterSpritesAtlas3.HasTextureID(text5))
						{
							characterSpritesAtlas3.GetPackedTextureByKey(text5).SetPath(text4);
						}
						else
						{
							TextureManager.Instance.characterSpritesAtlas.AddTexturePathToAtlas(text5, text4, 80);
						}
					}
				}
			}
		}
	}

	private void BaseCharacterClass()
	{
		_constantData = Resources.Load<CharacterClassData>("Character Class Data/" + _className + " Data");
		if (_constantData == null)
		{
			throw new Exception("There are no class assets for " + _className);
		}
	}

	public bool IsCombatant()
	{
		if (traitNames != null)
		{
			for (int i = 0; i < traitNames.Length; i++)
			{
				if (traitNames[i] == "Combatant")
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsZombie()
	{
		return identifier == "Zombie";
	}

	public bool CanDoJob(JOB_TYPE jobType)
	{
		if (ableJobs != null)
		{
			return ableJobs.Contains(jobType);
		}
		return false;
	}

	public bool IsSpecialClass()
	{
		return identifier == "Special";
	}

	public bool IsBasicResourceProducer(FACTION_TYPE p_factionType)
	{
		switch (p_factionType)
		{
		case FACTION_TYPE.Elven_Kingdom:
			return className == "Logger";
		case FACTION_TYPE.Human_Empire:
			return className == "Miner";
		default:
			if (!(className == "Logger"))
			{
				return className == "Miner";
			}
			return true;
		}
	}

	public int GetComputedAttackPowerWithPrimordialPool(RACE p_race)
	{
		if (PlayerManager.Instance.player == null)
		{
			return baseAttackPower;
		}
		RaceData raceData = RaceManager.Instance.GetRaceData(p_race);
		float allBonus = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[raceData.category].GetAllBonus(PRIMORDIAL_STATS_BONUS.Str);
		float allBonus2 = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[raceData.category].GetAllBonus(PRIMORDIAL_STATS_BONUS.Str);
		float num = ((attackType == ATTACK_TYPE.PHYSICAL) ? allBonus : allBonus2);
		return Mathf.RoundToInt((float)baseAttackPower * (num / 100f + 1f));
	}

	public int GetSummonCost()
	{
		return SpellUtilities.GetModifiedSpellCost(summonCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
	}

	public CharacterSpritesPerAnimation GetAssets(RACE p_race)
	{
		if (_animationSprites.ContainsKey(p_race))
		{
			return _animationSprites[p_race];
		}
		return _animationSprites[RACE.NONE];
	}

	public Sprite GetCharacterPortraitSprite()
	{
		Sprite result = null;
		if (!string.IsNullOrEmpty(_characterPortraitSpriteKeyID))
		{
			result = TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(_characterPortraitSpriteKeyID);
		}
		return result;
	}

	public Sprite GetSmallPortraitSprite()
	{
		Sprite result = null;
		if (!string.IsNullOrEmpty(_smallPortraitSpriteKeyID))
		{
			result = TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(_smallPortraitSpriteKeyID);
		}
		return result;
	}

	public int GetBaseDPS()
	{
		float num = (float)baseAttackSpeed / 1000f;
		return (int)((float)baseAttackPower / num);
	}

	public bool IsReligiousCultLeaderClass()
	{
		if (!(className == "Demon Cult Leader") && !(className == "Priest"))
		{
			return className == "Great Witch";
		}
		return true;
	}

	public bool IsReligiousCultLeaderClass(out RELIGION p_religion)
	{
		if (className == "Demon Cult Leader")
		{
			p_religion = RELIGION.Demon_Worship;
			return true;
		}
		if (className == "Priest")
		{
			p_religion = RELIGION.Divine_Worship;
			return true;
		}
		if (className == "Great Witch")
		{
			p_religion = RELIGION.Nature_Worship;
			return true;
		}
		p_religion = RELIGION.None;
		return false;
	}

	public bool IsReligiousCultLeaderClass(RELIGION p_religion)
	{
		if (p_religion == RELIGION.Demon_Worship && className == "Demon Cult Leader")
		{
			return true;
		}
		if (p_religion == RELIGION.Divine_Worship && className == "Priest")
		{
			return true;
		}
		if (p_religion == RELIGION.Nature_Worship && className == "Great Witch")
		{
			return true;
		}
		return false;
	}
}
