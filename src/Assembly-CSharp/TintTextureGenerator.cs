using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TintTextureGenerator : MonoBehaviour
{
	public int k_TintMapSize = 256;

	private Texture2D m_TintTexture;

	private Texture2D tintTexture
	{
		get
		{
			if (m_TintTexture == null)
			{
				m_TintTexture = new Texture2D(k_TintMapSize, k_TintMapSize, TextureFormat.ARGB32, mipChain: false);
				m_TintTexture.hideFlags = HideFlags.HideAndDontSave;
				m_TintTexture.wrapMode = TextureWrapMode.Clamp;
				m_TintTexture.filterMode = FilterMode.Bilinear;
				RefreshGlobalShaderValues();
			}
			return m_TintTexture;
		}
	}

	public void Start()
	{
		Refresh(GetComponent<Grid>());
	}

	public void Refresh(Grid grid)
	{
		if (grid == null)
		{
			return;
		}
		int width = tintTexture.width;
		int height = tintTexture.height;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				Vector3Int position = TextureToWorld(new Vector3Int(j, i, 0));
				tintTexture.SetPixel(j, i, GetGridInformation(grid).GetPositionProperty(position, "Tint", Color.white));
			}
		}
		tintTexture.Apply();
	}

	public void Refresh(Grid grid, Vector3Int position)
	{
		if (!(grid == null))
		{
			RefreshGlobalShaderValues();
			Vector3Int vector3Int = WorldToTexture(position);
			tintTexture.SetPixel(vector3Int.x, vector3Int.y, GetGridInformation(grid).GetPositionProperty(position, "Tint", Color.white));
			tintTexture.Apply();
		}
	}

	public Color GetColor(Grid grid, Vector3Int position)
	{
		if (grid == null)
		{
			return Color.white;
		}
		return GetGridInformation(grid).GetPositionProperty(position, "Tint", Color.white);
	}

	public void SetColor(Grid grid, Vector3Int position, Color color)
	{
		if (!(grid == null))
		{
			GetGridInformation(grid).SetPositionProperty(position, "Tint", color);
			Refresh(grid, position);
		}
	}

	private Vector3Int WorldToTexture(Vector3Int world)
	{
		return new Vector3Int(world.x + tintTexture.width / 2, world.y + tintTexture.height / 2, 0);
	}

	private Vector3Int TextureToWorld(Vector3Int texpos)
	{
		return new Vector3Int(texpos.x - tintTexture.width / 2, texpos.y - tintTexture.height / 2, 0);
	}

	private GridInformation GetGridInformation(Grid grid)
	{
		GridInformation gridInformation = grid.GetComponent<GridInformation>();
		if (gridInformation == null)
		{
			gridInformation = grid.gameObject.AddComponent<GridInformation>();
		}
		return gridInformation;
	}

	private void RefreshGlobalShaderValues()
	{
		Shader.SetGlobalTexture("_TintMap", m_TintTexture);
		Shader.SetGlobalFloat("_TintMapSize", k_TintMapSize);
	}
}
