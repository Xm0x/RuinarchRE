using UnityEngine;

public class RFX4_MobileBloom : MonoBehaviour
{
	[Range(0.2f, 1f)]
	[Tooltip("Camera render texture resolution")]
	public float RenderTextureResolutoinFactor = 0.5f;

	[Range(0.05f, 2f)]
	[Tooltip("Blend factor of the result image.")]
	public float Intensity = 0.5f;

	private static float Threshold = 1.3f;

	private const string shaderName = "Hidden/KriptoFX/PostEffects/RFX4_Bloom";

	private const int kMaxIterations = 16;

	private readonly RenderTexture[] m_blurBuffer1 = new RenderTexture[16];

	private readonly RenderTexture[] m_blurBuffer2 = new RenderTexture[16];

	private RenderTexture Source;

	private Material _bloomMaterial;

	private Material bloomMaterial
	{
		get
		{
			if (_bloomMaterial == null)
			{
				Shader shader = Shader.Find("Hidden/KriptoFX/PostEffects/RFX4_Bloom");
				if (shader == null)
				{
					Debug.LogError("Can't find shader Hidden/KriptoFX/PostEffects/RFX4_Bloom");
				}
				_bloomMaterial = new Material(shader);
			}
			return _bloomMaterial;
		}
	}

	private void Start()
	{
	}

	private void OnPreRender()
	{
		Source = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, SupportedHdrFormat());
		Camera.main.targetTexture = Source;
	}

	private void OnPostRender()
	{
		Camera.main.targetTexture = null;
		UpdateBloom(Source, null);
		RenderTexture.ReleaseTemporary(Source);
	}

	private RenderTextureFormat SupportedHdrFormat()
	{
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float))
		{
			return RenderTextureFormat.RGB111110Float;
		}
		return RenderTextureFormat.DefaultHDR;
	}

	private void UpdateBloom(RenderTexture source, RenderTexture dest)
	{
		int num = Screen.width / 2;
		int num2 = Screen.height / 2;
		RenderTextureFormat format = RenderTextureFormat.Default;
		int width = (int)((float)num * RenderTextureResolutoinFactor);
		num2 = (int)((float)num2 * RenderTextureResolutoinFactor);
		float num3 = Mathf.Log(num2, 2f) - 1f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float value = Mathf.GammaToLinearSpace(Threshold);
		bloomMaterial.SetFloat("_Threshold", value);
		float num6 = 0.5f + num3 - (float)num4;
		bloomMaterial.SetFloat("_SampleScale", num6 * 0.5f);
		bloomMaterial.SetFloat("_Intensity", Mathf.Max(0f, Intensity));
		RenderTexture temporary = RenderTexture.GetTemporary(width, num2, 0, format);
		Graphics.Blit(source, temporary, bloomMaterial, 0);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			m_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer1[i], bloomMaterial, 1);
			renderTexture = m_blurBuffer1[i];
		}
		for (int num7 = num5 - 2; num7 >= 0; num7--)
		{
			RenderTexture renderTexture2 = m_blurBuffer1[num7];
			bloomMaterial.SetTexture("_BaseTex", renderTexture2);
			m_blurBuffer2[num7] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer2[num7], bloomMaterial, 2);
			renderTexture = m_blurBuffer2[num7];
		}
		RenderTexture temporary2 = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format);
		Graphics.Blit(renderTexture, temporary2, bloomMaterial, 3);
		bloomMaterial.SetTexture("_BaseTex", source);
		Graphics.Blit(temporary2, dest, bloomMaterial, 4);
		for (int j = 0; j < 16; j++)
		{
			if (m_blurBuffer1[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer1[j]);
			}
			if (m_blurBuffer2[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer2[j]);
			}
			m_blurBuffer1[j] = null;
			m_blurBuffer2[j] = null;
		}
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
