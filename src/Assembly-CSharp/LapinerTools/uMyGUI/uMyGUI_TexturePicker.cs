using System;
using UnityEngine;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI;

public class uMyGUI_TexturePicker : MonoBehaviour
{
	[SerializeField]
	private GameObject m_texturePrefab;

	[SerializeField]
	private GameObject m_selectionPrefab;

	[SerializeField]
	private float m_offsetStart;

	[SerializeField]
	private float m_offsetEnd;

	[SerializeField]
	private float m_padding = 4f;

	[SerializeField]
	private Action<int> m_buttonCallback;

	private Texture2D[] m_textures = new Texture2D[0];

	private RectTransform m_rectTransform;

	private float m_elementSize = 1f;

	private GameObject m_selectionInstance;

	private GameObject[] m_instances = new GameObject[0];

	public GameObject TexturePrefab
	{
		get
		{
			return m_texturePrefab;
		}
		set
		{
			m_texturePrefab = value;
		}
	}

	public GameObject SelectionPrefab
	{
		get
		{
			return m_selectionPrefab;
		}
		set
		{
			m_selectionPrefab = value;
		}
	}

	public float OffsetStart
	{
		get
		{
			return m_offsetStart;
		}
		set
		{
			m_offsetStart = value;
		}
	}

	public float OffsetEnd
	{
		get
		{
			return m_offsetEnd;
		}
		set
		{
			m_offsetEnd = value;
		}
	}

	public float Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			m_padding = value;
		}
	}

	public Action<int> ButtonCallback
	{
		get
		{
			return m_buttonCallback;
		}
		set
		{
			m_buttonCallback = value;
		}
	}

	public Texture2D[] Textures
	{
		get
		{
			return m_textures;
		}
		set
		{
			m_textures = value;
		}
	}

	private RectTransform RTransform
	{
		get
		{
			if (!(m_rectTransform != null))
			{
				return m_rectTransform = GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public GameObject[] Instances => m_instances;

	public void SetSelection(int p_selectionIndex)
	{
		if (m_selectionPrefab != null)
		{
			if (p_selectionIndex < 0 || p_selectionIndex >= m_instances.Length)
			{
				UnityEngine.Object.Destroy(m_selectionInstance);
				m_selectionInstance = null;
				return;
			}
			if (m_selectionInstance == null)
			{
				m_selectionInstance = UnityEngine.Object.Instantiate(m_selectionPrefab);
			}
			else
			{
				RectTransform component = m_selectionInstance.GetComponent<RectTransform>();
				Vector2 anchoredPosition = component.anchoredPosition;
				anchoredPosition.x = m_selectionPrefab.GetComponent<RectTransform>().anchoredPosition.x;
				component.anchoredPosition = anchoredPosition;
			}
			SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), p_selectionIndex, m_elementSize);
		}
		else
		{
			Debug.LogError("uMyGUI_TexturePicker: SetSelection: you have passed a non negative selection index '" + p_selectionIndex + "', but the SelectionPrefab was not provided in the inspector or via script!");
		}
	}

	public void SetTextures(Texture2D[] p_textures, int p_selectedIndex)
	{
		if (m_texturePrefab != null)
		{
			m_textures = p_textures;
			UnityEngine.Object.Destroy(m_selectionInstance);
			for (int i = 0; i < m_instances.Length; i++)
			{
				UnityEngine.Object.Destroy(m_instances[i]);
			}
			m_instances = new GameObject[p_textures.Length];
			float num = 0f;
			for (int j = 0; j < p_textures.Length; j++)
			{
				m_instances[j] = UnityEngine.Object.Instantiate(m_texturePrefab);
				RectTransform component = m_instances[j].GetComponent<RectTransform>();
				m_elementSize = component.rect.width;
				SetRectTransformPosition(component, j, m_elementSize);
				RawImage rawImage = TryFindComponent<RawImage>(m_instances[j]);
				if (rawImage != null)
				{
					rawImage.texture = p_textures[j];
				}
				else
				{
					Debug.LogError("uMyGUI_TexturePicker: SetTextures: TexturePrefab must have a RawImage component attached (can be in children).");
				}
				if (m_buttonCallback != null)
				{
					Button button = TryFindComponent<Button>(m_instances[j]);
					if (button != null)
					{
						int indexCopy = j;
						button.onClick.AddListener(delegate
						{
							m_buttonCallback(indexCopy);
						});
					}
				}
				num = component.anchoredPosition.x + m_elementSize;
				if (j == p_selectedIndex)
				{
					if (m_selectionPrefab != null)
					{
						m_selectionInstance = UnityEngine.Object.Instantiate(m_selectionPrefab);
						SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), j, m_elementSize);
					}
					else
					{
						Debug.LogError("uMyGUI_TexturePicker: SetTextures: you have passed a non negative selection index '" + p_selectedIndex + "', but the SelectionPrefab was not provided in the inspector or via script!");
					}
				}
			}
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num - RTransform.rect.xMin + m_offsetEnd);
		}
		else
		{
			Debug.LogError("uMyGUI_TexturePicker: SetTextures: you must provide the TexturePrefab in the inspector or via script!");
		}
	}

	private void OnDestroy()
	{
		m_buttonCallback = null;
	}

	private void SetRectTransformPosition(RectTransform p_transform, int p_positionIndex, float p_size)
	{
		p_transform.SetParent(RTransform, worldPositionStays: false);
		Vector2 anchoredPosition = p_transform.anchoredPosition;
		anchoredPosition.x += m_offsetStart + (float)p_positionIndex * (p_size + m_padding);
		p_transform.anchoredPosition = anchoredPosition;
	}

	private T TryFindComponent<T>(GameObject p_object) where T : Component
	{
		T val = p_object.GetComponent<T>();
		if (val == null)
		{
			T[] componentsInChildren = p_object.GetComponentsInChildren<T>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				val = componentsInChildren[0];
			}
		}
		return val;
	}
}
