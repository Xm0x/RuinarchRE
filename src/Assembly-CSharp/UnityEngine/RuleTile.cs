using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UnityEngine;

public class RuleTile<T> : RuleTile
{
	public sealed override Type m_NeighborType => typeof(T);
}
[Serializable]
[CreateAssetMenu]
public class RuleTile : TileBase
{
	[Serializable]
	public class TilingRule
	{
		public class Neighbor
		{
			public const int DontCare = 0;

			public const int This = 1;

			public const int NotThis = 2;
		}

		public enum Transform
		{
			Fixed,
			Rotated,
			MirrorX,
			MirrorY
		}

		public enum OutputSprite
		{
			Single,
			Random,
			Animation
		}

		public int[] m_Neighbors;

		public Sprite[] m_Sprites;

		public float m_AnimationSpeed;

		public float m_PerlinScale;

		public Transform m_RuleTransform;

		public OutputSprite m_Output;

		public Tile.ColliderType m_ColliderType;

		public Transform m_RandomTransform;

		public TilingRule()
		{
			m_Output = OutputSprite.Single;
			m_Neighbors = new int[NeighborCount];
			m_Sprites = new Sprite[1];
			m_AnimationSpeed = 1f;
			m_PerlinScale = 0.5f;
			m_ColliderType = Tile.ColliderType.Sprite;
			for (int i = 0; i < m_Neighbors.Length; i++)
			{
				m_Neighbors[i] = 0;
			}
		}
	}

	private static readonly int[,] RotatedOrMirroredIndexes = new int[5, 8]
	{
		{ 2, 4, 7, 1, 6, 0, 3, 5 },
		{ 7, 6, 5, 4, 3, 2, 1, 0 },
		{ 5, 3, 0, 6, 1, 7, 4, 2 },
		{ 2, 1, 0, 4, 3, 7, 6, 5 },
		{ 5, 6, 7, 3, 4, 0, 1, 2 }
	};

	private static readonly int NeighborCount = 8;

	public Sprite m_DefaultSprite;

	public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

	private TileBase[] m_CachedNeighboringTiles = new TileBase[NeighborCount];

	private TileBase m_OverrideSelf;

	[HideInInspector]
	public List<TilingRule> m_TilingRules;

	public virtual Type m_NeighborType => typeof(TilingRule.Neighbor);

	public TileBase m_Self
	{
		get
		{
			if (!m_OverrideSelf)
			{
				return this;
			}
			return m_OverrideSelf;
		}
		set
		{
			m_OverrideSelf = value;
		}
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		TileBase[] neighboringTiles = null;
		GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
		Matrix4x4 identity = Matrix4x4.identity;
		tileData.sprite = m_DefaultSprite;
		tileData.colliderType = m_DefaultColliderType;
		tileData.flags = TileFlags.LockTransform;
		tileData.transform = identity;
		foreach (TilingRule tilingRule in m_TilingRules)
		{
			Matrix4x4 transform = identity;
			if (!RuleMatches(tilingRule, ref neighboringTiles, ref transform))
			{
				continue;
			}
			switch (tilingRule.m_Output)
			{
			case TilingRule.OutputSprite.Single:
			case TilingRule.OutputSprite.Animation:
				tileData.sprite = tilingRule.m_Sprites[0];
				break;
			case TilingRule.OutputSprite.Random:
			{
				int num = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, tilingRule.m_PerlinScale, 100000f) * (float)tilingRule.m_Sprites.Length), 0, tilingRule.m_Sprites.Length - 1);
				tileData.sprite = tilingRule.m_Sprites[num];
				if (tilingRule.m_RandomTransform != TilingRule.Transform.Fixed)
				{
					transform = ApplyRandomTransform(tilingRule.m_RandomTransform, transform, tilingRule.m_PerlinScale, position);
				}
				break;
			}
			}
			tileData.transform = transform;
			tileData.colliderType = tilingRule.m_ColliderType;
			break;
		}
	}

	private static float GetPerlinValue(Vector3Int position, float scale, float offset)
	{
		return Mathf.PerlinNoise(((float)position.x + offset) * scale, ((float)position.y + offset) * scale);
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		TileBase[] neighboringTiles = null;
		Matrix4x4 identity = Matrix4x4.identity;
		foreach (TilingRule tilingRule in m_TilingRules)
		{
			if (tilingRule.m_Output == TilingRule.OutputSprite.Animation)
			{
				Matrix4x4 transform = identity;
				GetMatchingNeighboringTiles(tilemap, position, ref neighboringTiles);
				if (RuleMatches(tilingRule, ref neighboringTiles, ref transform))
				{
					tileAnimationData.animatedSprites = tilingRule.m_Sprites;
					tileAnimationData.animationSpeed = tilingRule.m_AnimationSpeed;
					return true;
				}
			}
		}
		return false;
	}

	public override void RefreshTile(Vector3Int location, ITilemap tileMap)
	{
		if (m_TilingRules != null && m_TilingRules.Count > 0)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					base.RefreshTile(location + new Vector3Int(j, i, 0), tileMap);
				}
			}
		}
		else
		{
			base.RefreshTile(location, tileMap);
		}
	}

	public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, ref Matrix4x4 transform)
	{
		for (int i = 0; i <= ((rule.m_RuleTransform == TilingRule.Transform.Rotated) ? 270 : 0); i += 90)
		{
			if (RuleMatches(rule, ref neighboringTiles, i))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -i), Vector3.one);
				return true;
			}
		}
		if (rule.m_RuleTransform == TilingRule.Transform.MirrorX && RuleMatches(rule, ref neighboringTiles, mirrorX: true, mirrorY: false))
		{
			transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
			return true;
		}
		if (rule.m_RuleTransform == TilingRule.Transform.MirrorY && RuleMatches(rule, ref neighboringTiles, mirrorX: false, mirrorY: true))
		{
			transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
			return true;
		}
		return false;
	}

	private static Matrix4x4 ApplyRandomTransform(TilingRule.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
	{
		float perlinValue = GetPerlinValue(position, perlinScale, 200000f);
		switch (type)
		{
		case TilingRule.Transform.MirrorX:
			return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(((double)perlinValue < 0.5) ? 1f : (-1f), 1f, 1f));
		case TilingRule.Transform.MirrorY:
			return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, ((double)perlinValue < 0.5) ? 1f : (-1f), 1f));
		case TilingRule.Transform.Rotated:
		{
			int num = Mathf.Clamp(Mathf.FloorToInt(perlinValue * 4f), 0, 3) * 90;
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -num), Vector3.one);
		}
		default:
			return original;
		}
	}

	public virtual bool RuleMatch(int neighbor, TileBase tile)
	{
		return neighbor switch
		{
			1 => tile == m_Self, 
			2 => tile != m_Self, 
			_ => true, 
		};
	}

	public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, int angle)
	{
		for (int i = 0; i < NeighborCount; i++)
		{
			int rotatedIndex = GetRotatedIndex(i, angle);
			TileBase tile = neighboringTiles[rotatedIndex];
			if (!RuleMatch(rule.m_Neighbors[i], tile))
			{
				return false;
			}
		}
		return true;
	}

	public bool RuleMatches(TilingRule rule, ref TileBase[] neighboringTiles, bool mirrorX, bool mirrorY)
	{
		for (int i = 0; i < NeighborCount; i++)
		{
			int mirroredIndex = GetMirroredIndex(i, mirrorX, mirrorY);
			TileBase tile = neighboringTiles[mirroredIndex];
			if (!RuleMatch(rule.m_Neighbors[i], tile))
			{
				return false;
			}
		}
		return true;
	}

	private void GetMatchingNeighboringTiles(ITilemap tilemap, Vector3Int position, ref TileBase[] neighboringTiles)
	{
		if (neighboringTiles != null)
		{
			return;
		}
		if (m_CachedNeighboringTiles == null || m_CachedNeighboringTiles.Length < NeighborCount)
		{
			m_CachedNeighboringTiles = new TileBase[NeighborCount];
		}
		int num = 0;
		for (int num2 = 1; num2 >= -1; num2--)
		{
			for (int i = -1; i <= 1; i++)
			{
				if (i != 0 || num2 != 0)
				{
					Vector3Int position2 = new Vector3Int(position.x + i, position.y + num2, position.z);
					m_CachedNeighboringTiles[num++] = tilemap.GetTile(position2);
				}
			}
		}
		neighboringTiles = m_CachedNeighboringTiles;
	}

	private int GetRotatedIndex(int original, int rotation)
	{
		return rotation switch
		{
			0 => original, 
			90 => RotatedOrMirroredIndexes[0, original], 
			180 => RotatedOrMirroredIndexes[1, original], 
			270 => RotatedOrMirroredIndexes[2, original], 
			_ => original, 
		};
	}

	private int GetMirroredIndex(int original, bool mirrorX, bool mirrorY)
	{
		if (mirrorX && mirrorY)
		{
			return RotatedOrMirroredIndexes[1, original];
		}
		if (mirrorX)
		{
			return RotatedOrMirroredIndexes[3, original];
		}
		if (mirrorY)
		{
			return RotatedOrMirroredIndexes[4, original];
		}
		return original;
	}

	private int GetIndexOfOffset(Vector3Int offset)
	{
		int num = offset.x + 1 + (-offset.y + 1) * 3;
		if (num >= 4)
		{
			num--;
		}
		return num;
	}

	public Vector3Int GetRotatedPos(Vector3Int original, int rotation)
	{
		return rotation switch
		{
			0 => original, 
			90 => new Vector3Int(-original.y, original.x, original.z), 
			180 => new Vector3Int(-original.x, -original.y, original.z), 
			270 => new Vector3Int(original.y, -original.x, original.z), 
			_ => original, 
		};
	}

	public Vector3Int GetMirroredPos(Vector3Int original, bool mirrorX, bool mirrorY)
	{
		return new Vector3Int(original.x * ((!mirrorX) ? 1 : (-1)), original.y * ((!mirrorY) ? 1 : (-1)), original.z);
	}
}
