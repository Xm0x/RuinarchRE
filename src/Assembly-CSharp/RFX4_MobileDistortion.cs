using UnityEngine;
using UnityEngine.Rendering;

public class RFX4_MobileDistortion : MonoBehaviour
{
	public bool IsActive = true;

	private CommandBuffer buf;

	private Camera cam;

	private bool bufferIsAdded;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		CreateBuffer();
	}

	private void CreateBuffer()
	{
		_ = Camera.main;
		buf = new CommandBuffer();
		buf.name = "_GrabOpaqueColor";
		int num = Shader.PropertyToID("_ScreenCopyOpaqueColor");
		int num2 = -1;
		RenderTextureFormat format = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565) ? RenderTextureFormat.RGB565 : RenderTextureFormat.Default);
		buf.GetTemporaryRT(num, num2, num2, 0, FilterMode.Bilinear, format);
		buf.Blit(BuiltinRenderTextureType.CurrentActive, num);
		buf.SetGlobalTexture("_GrabTexture", num);
		buf.SetGlobalTexture("_GrabTextureMobile", num);
	}

	private void OnEnable()
	{
		AddBuffer();
	}

	private void OnDisable()
	{
		RemoveBuffer();
	}

	private void AddBuffer()
	{
		cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, buf);
		bufferIsAdded = true;
	}

	private void RemoveBuffer()
	{
		cam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, buf);
		bufferIsAdded = false;
	}

	private void Update()
	{
		if (IsActive)
		{
			if (!bufferIsAdded)
			{
				AddBuffer();
			}
		}
		else if (bufferIsAdded)
		{
			RemoveBuffer();
		}
	}

	private bool IsSupportedHdr()
	{
		return Camera.main.allowHDR;
	}
}
