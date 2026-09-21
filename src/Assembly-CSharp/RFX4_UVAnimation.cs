using UnityEngine;

[ExecuteInEditMode]
public class RFX4_UVAnimation : MonoBehaviour
{
	public int TilesX = 4;

	public int TilesY = 4;

	[Range(1f, 360f)]
	public int FPS = 32;

	public int StartFrameOffset;

	public bool IsLoop = true;

	public bool IsReverse;

	public bool IsInterpolateFrames = true;

	public RFX4_TextureShaderProperties[] TextureNames = new RFX4_TextureShaderProperties[1];

	private int count;

	private Renderer currentRenderer;

	private Projector projector;

	private Material instanceMaterial;

	private float animationStartTime;

	private bool canUpdate;

	private int previousIndex;

	private int totalFrames;

	private float currentInterpolatedTime;

	private int currentIndex;

	private Vector2 size;

	private bool isInitialized;

	private float prevRealTime;

	private void OnEnable()
	{
		if (isInitialized)
		{
			InitDefaultVariables();
		}
	}

	private void Start()
	{
		InitDefaultVariables();
		isInitialized = true;
	}

	private void OnWillRenderObject()
	{
		if (!Application.isPlaying)
		{
			ManualUpdate();
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			ManualUpdate();
		}
	}

	private void InitDefaultVariables()
	{
		currentRenderer = GetComponent<Renderer>();
		UpdateMaterial();
		totalFrames = TilesX * TilesY;
		previousIndex = 0;
		canUpdate = true;
		count = TilesY * TilesX;
		Vector3 zero = Vector3.zero;
		StartFrameOffset -= StartFrameOffset / count * count;
		size = new Vector2(1f / (float)TilesX, 1f / (float)TilesY);
		animationStartTime = (Application.isPlaying ? Time.time : Time.realtimeSinceStartup);
		if (instanceMaterial != null)
		{
			RFX4_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX4_TextureShaderProperties rFX4_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX4_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX4_TextureShaderProperties.ToString(), zero);
			}
		}
	}

	private void ManualUpdate()
	{
		if (canUpdate)
		{
			UpdateMaterial();
			SetSpriteAnimation();
			if (IsInterpolateFrames)
			{
				SetSpriteAnimationIterpolated();
			}
		}
	}

	private void UpdateMaterial()
	{
		if (!(currentRenderer == null))
		{
			if (Application.isPlaying)
			{
				instanceMaterial = currentRenderer.material;
			}
			instanceMaterial = currentRenderer.sharedMaterial;
			if (IsInterpolateFrames)
			{
				instanceMaterial.EnableKeyword("USE_SCRIPT_FRAMEBLENDING");
			}
			else
			{
				instanceMaterial.DisableKeyword("USE_SCRIPT_FRAMEBLENDING");
			}
		}
	}

	private void SetSpriteAnimation()
	{
		int num = (int)(((Application.isPlaying ? Time.time : Time.realtimeSinceStartup) - animationStartTime) * (float)FPS);
		num %= totalFrames;
		if (!IsLoop && num < previousIndex)
		{
			canUpdate = false;
			return;
		}
		if (IsInterpolateFrames && num != previousIndex)
		{
			currentInterpolatedTime = 0f;
		}
		previousIndex = num;
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 value = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			RFX4_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX4_TextureShaderProperties rFX4_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX4_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX4_TextureShaderProperties.ToString(), value);
			}
		}
	}

	public float DeltaTime()
	{
		if (Application.isPlaying)
		{
			return Time.deltaTime;
		}
		float result = Time.realtimeSinceStartup - prevRealTime;
		prevRealTime = Time.realtimeSinceStartup;
		return result;
	}

	private void SetSpriteAnimationIterpolated()
	{
		currentInterpolatedTime += DeltaTime();
		int num = previousIndex + 1;
		if (num == totalFrames)
		{
			num = previousIndex;
		}
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 vector = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			instanceMaterial.SetVector("_MainTex_NextFrame", new Vector4(size.x, size.y, vector.x, vector.y));
			instanceMaterial.SetFloat("InterpolationValue", Mathf.Clamp01(currentInterpolatedTime * (float)FPS));
		}
	}
}
