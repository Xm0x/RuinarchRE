using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

namespace UnityEngine;

[Serializable]
[CreateAssetMenu]
public class RuleOverrideTile : TileBase
{
	[Serializable]
	public class TileSpritePair
	{
		public Sprite m_OriginalSprite;

		public Sprite m_OverrideSprite;
	}

	[Serializable]
	public class OverrideTilingRule
	{
		public bool m_Enabled;

		public RuleTile.TilingRule m_TilingRule = new RuleTile.TilingRule();
	}

	public RuleTile m_Tile;

	public bool m_Advanced;

	public List<TileSpritePair> m_Sprites = new List<TileSpritePair>();

	public List<OverrideTilingRule> m_OverrideTilingRules = new List<OverrideTilingRule>();

	public OverrideTilingRule m_OverrideDefault = new OverrideTilingRule();

	private RuleTile m_RuntimeTile;

	public Sprite this[Sprite originalSprite]
	{
		get
		{
			foreach (TileSpritePair sprite in m_Sprites)
			{
				if (sprite.m_OriginalSprite == originalSprite)
				{
					return sprite.m_OverrideSprite;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_Sprites = m_Sprites.Where((TileSpritePair spritePair) => spritePair.m_OriginalSprite != originalSprite).ToList();
				return;
			}
			foreach (TileSpritePair sprite in m_Sprites)
			{
				if (sprite.m_OriginalSprite == originalSprite)
				{
					sprite.m_OverrideSprite = value;
					return;
				}
			}
			m_Sprites.Add(new TileSpritePair
			{
				m_OriginalSprite = originalSprite,
				m_OverrideSprite = value
			});
		}
	}

	public RuleTile.TilingRule this[RuleTile.TilingRule originalRule]
	{
		get
		{
			if (!m_Tile)
			{
				return null;
			}
			int num = m_Tile.m_TilingRules.IndexOf(originalRule);
			if (num == -1)
			{
				return null;
			}
			if (m_OverrideTilingRules.Count < num + 1)
			{
				return null;
			}
			if (!m_OverrideTilingRules[num].m_Enabled)
			{
				return null;
			}
			return m_OverrideTilingRules[num].m_TilingRule;
		}
		set
		{
			if (!m_Tile)
			{
				return;
			}
			int num = m_Tile.m_TilingRules.IndexOf(originalRule);
			if (num == -1)
			{
				return;
			}
			if (value == null)
			{
				if (m_OverrideTilingRules.Count >= num + 1)
				{
					m_OverrideTilingRules[num].m_Enabled = false;
					while (m_OverrideTilingRules.Count > 0 && !m_OverrideTilingRules[m_OverrideTilingRules.Count - 1].m_Enabled)
					{
						m_OverrideTilingRules.RemoveAt(m_OverrideTilingRules.Count - 1);
					}
				}
			}
			else
			{
				while (m_OverrideTilingRules.Count < num + 1)
				{
					m_OverrideTilingRules.Add(new OverrideTilingRule());
				}
				m_OverrideTilingRules[num].m_Enabled = true;
				m_OverrideTilingRules[num].m_TilingRule = CloneTilingRule(value);
				m_OverrideTilingRules[num].m_TilingRule.m_Neighbors = null;
			}
		}
	}

	public RuleTile.TilingRule m_OriginalDefault
	{
		get
		{
			RuleTile.TilingRule tilingRule = new RuleTile.TilingRule();
			tilingRule.m_Sprites = new Sprite[1] { (m_Tile != null) ? m_Tile.m_DefaultSprite : null };
			tilingRule.m_ColliderType = ((m_Tile != null) ? m_Tile.m_DefaultColliderType : Tile.ColliderType.None);
			return tilingRule;
		}
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		if (!m_RuntimeTile)
		{
			Override();
		}
		return m_RuntimeTile.GetTileAnimationData(position, tilemap, ref tileAnimationData);
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		if (!m_RuntimeTile)
		{
			Override();
		}
		m_RuntimeTile.GetTileData(position, tilemap, ref tileData);
	}

	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		if (!m_RuntimeTile)
		{
			Override();
		}
		m_RuntimeTile.RefreshTile(position, tilemap);
	}

	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
	{
		Override();
		return m_RuntimeTile.StartUp(position, tilemap, go);
	}

	public void ApplyOverrides(IList<KeyValuePair<Sprite, Sprite>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		for (int i = 0; i < overrides.Count; i++)
		{
			this[overrides[i].Key] = overrides[i].Value;
		}
	}

	public void GetOverrides(List<KeyValuePair<Sprite, Sprite>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		overrides.Clear();
		if (!m_Tile)
		{
			return;
		}
		List<Sprite> list = new List<Sprite>();
		if ((bool)m_Tile.m_DefaultSprite)
		{
			list.Add(m_Tile.m_DefaultSprite);
		}
		foreach (RuleTile.TilingRule tilingRule in m_Tile.m_TilingRules)
		{
			Sprite[] sprites = tilingRule.m_Sprites;
			foreach (Sprite sprite in sprites)
			{
				if ((bool)sprite && !list.Contains(sprite))
				{
					list.Add(sprite);
				}
			}
		}
		foreach (Sprite item in list)
		{
			overrides.Add(new KeyValuePair<Sprite, Sprite>(item, this[item]));
		}
	}

	public void ApplyOverrides(IList<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRule>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		for (int i = 0; i < overrides.Count; i++)
		{
			this[overrides[i].Key] = overrides[i].Value;
		}
	}

	public void GetOverrides(List<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRule>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		overrides.Clear();
		if (!m_Tile)
		{
			return;
		}
		foreach (RuleTile.TilingRule tilingRule in m_Tile.m_TilingRules)
		{
			RuleTile.TilingRule value = this[tilingRule];
			overrides.Add(new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRule>(tilingRule, value));
		}
		overrides.Add(new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRule>(m_OriginalDefault, m_OverrideDefault.m_TilingRule));
	}

	public void Override()
	{
		m_RuntimeTile = (m_Tile ? Object.Instantiate(m_Tile) : new RuleTile());
		m_RuntimeTile.m_Self = this;
		if (!m_Advanced)
		{
			if ((bool)m_RuntimeTile.m_DefaultSprite)
			{
				m_RuntimeTile.m_DefaultSprite = this[m_RuntimeTile.m_DefaultSprite];
			}
			if (m_RuntimeTile.m_TilingRules == null)
			{
				return;
			}
			{
				foreach (RuleTile.TilingRule tilingRule2 in m_RuntimeTile.m_TilingRules)
				{
					for (int i = 0; i < tilingRule2.m_Sprites.Length; i++)
					{
						if ((bool)tilingRule2.m_Sprites[i])
						{
							tilingRule2.m_Sprites[i] = this[tilingRule2.m_Sprites[i]];
						}
					}
				}
				return;
			}
		}
		if (m_OverrideDefault.m_Enabled)
		{
			m_RuntimeTile.m_DefaultSprite = ((m_OverrideDefault.m_TilingRule.m_Sprites.Length != 0) ? m_OverrideDefault.m_TilingRule.m_Sprites[0] : null);
			m_RuntimeTile.m_DefaultColliderType = m_OverrideDefault.m_TilingRule.m_ColliderType;
		}
		if (m_RuntimeTile.m_TilingRules == null)
		{
			return;
		}
		for (int j = 0; j < m_RuntimeTile.m_TilingRules.Count; j++)
		{
			RuleTile.TilingRule to = m_RuntimeTile.m_TilingRules[j];
			RuleTile.TilingRule tilingRule = this[m_Tile.m_TilingRules[j]];
			if (tilingRule != null)
			{
				CopyTilingRule(tilingRule, to, copyRule: false);
			}
		}
	}

	public RuleTile.TilingRule CloneTilingRule(RuleTile.TilingRule from)
	{
		RuleTile.TilingRule tilingRule = new RuleTile.TilingRule();
		CopyTilingRule(from, tilingRule, copyRule: true);
		return tilingRule;
	}

	public void CopyTilingRule(RuleTile.TilingRule from, RuleTile.TilingRule to, bool copyRule)
	{
		if (copyRule)
		{
			to.m_Neighbors = from.m_Neighbors;
			to.m_RuleTransform = from.m_RuleTransform;
		}
		to.m_Sprites = from.m_Sprites.Clone() as Sprite[];
		to.m_AnimationSpeed = from.m_AnimationSpeed;
		to.m_PerlinScale = from.m_PerlinScale;
		to.m_Output = from.m_Output;
		to.m_ColliderType = from.m_ColliderType;
		to.m_RandomTransform = from.m_RandomTransform;
	}
}
