using UnityEngine;

namespace SpriteGlow;

[AddComponentMenu("Effects/Sprite Glow")]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class SpriteGlowEffect : MonoBehaviour
{
	[Tooltip("Base color of the glow.")]
	[SerializeField]
	private Color glowColor = Color.white;

	[Tooltip("The brightness (power) of the glow.")]
	[Range(1f, 10f)]
	[SerializeField]
	private float glowBrightness = 2f;

	[Tooltip("Width of the outline, in texels.")]
	[Range(0f, 10f)]
	[SerializeField]
	private int outlineWidth = 1;

	[Tooltip("Threshold to determine sprite borders.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float alphaThreshold = 0.01f;

	[Tooltip("Whether the outline should only be drawn outside of the sprite borders. Make sure sprite texture has sufficient transparent space for the required outline width.")]
	[SerializeField]
	private bool drawOutside;

	[Tooltip("Whether to enable GPU instancing.")]
	[SerializeField]
	private bool enableInstancing;

	private static readonly int isOutlineEnabledId = Shader.PropertyToID("_IsOutlineEnabled");

	private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");

	private static readonly int outlineSizeId = Shader.PropertyToID("_OutlineSize");

	private static readonly int alphaThresholdId = Shader.PropertyToID("_AlphaThreshold");

	private MaterialPropertyBlock materialProperties;

	public SpriteRenderer Renderer { get; private set; }

	public Color GlowColor
	{
		get
		{
			return glowColor;
		}
		set
		{
			if (glowColor != value)
			{
				glowColor = value;
				SetMaterialProperties();
			}
		}
	}

	public float GlowBrightness
	{
		get
		{
			return glowBrightness;
		}
		set
		{
			if (glowBrightness != value)
			{
				glowBrightness = value;
				SetMaterialProperties();
			}
		}
	}

	public int OutlineWidth
	{
		get
		{
			return outlineWidth;
		}
		set
		{
			if (outlineWidth != value)
			{
				outlineWidth = value;
				SetMaterialProperties();
			}
		}
	}

	public float AlphaThreshold
	{
		get
		{
			return alphaThreshold;
		}
		set
		{
			if (alphaThreshold != value)
			{
				alphaThreshold = value;
				SetMaterialProperties();
			}
		}
	}

	public bool DrawOutside
	{
		get
		{
			return drawOutside;
		}
		set
		{
			if (drawOutside != value)
			{
				drawOutside = value;
				SetMaterialProperties();
			}
		}
	}

	public bool EnableInstancing
	{
		get
		{
			return enableInstancing;
		}
		set
		{
			if (enableInstancing != value)
			{
				enableInstancing = value;
				SetMaterialProperties();
			}
		}
	}

	private void Awake()
	{
		Renderer = GetComponent<SpriteRenderer>();
	}

	private void OnEnable()
	{
		SetMaterialProperties();
	}

	private void OnDisable()
	{
		SetMaterialProperties();
	}

	private void OnValidate()
	{
		if (base.isActiveAndEnabled)
		{
			SetMaterialProperties();
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		SetMaterialProperties();
	}

	private void SetMaterialProperties()
	{
		if ((bool)Renderer)
		{
			Renderer.sharedMaterial = SpriteGlowMaterial.GetSharedFor(this);
			if (materialProperties == null)
			{
				materialProperties = new MaterialPropertyBlock();
			}
			materialProperties.SetFloat(isOutlineEnabledId, base.isActiveAndEnabled ? 1 : 0);
			materialProperties.SetColor(outlineColorId, GlowColor * GlowBrightness);
			materialProperties.SetFloat(outlineSizeId, OutlineWidth);
			materialProperties.SetFloat(alphaThresholdId, AlphaThreshold);
			Renderer.SetPropertyBlock(materialProperties);
		}
	}
}
