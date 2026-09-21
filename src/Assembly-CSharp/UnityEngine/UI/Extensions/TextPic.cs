using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/TextPic")]
[ExecuteInEditMode]
public class TextPic : Text, IPointerClickHandler, IEventSystemHandler, IPointerExitHandler, IPointerEnterHandler, ISelectHandler
{
	[Serializable]
	public struct IconName
	{
		public string name;

		public Sprite sprite;

		public Vector2 offset;

		public Vector2 scale;
	}

	[Serializable]
	public class HrefClickEvent : UnityEvent<string>
	{
	}

	private class HrefInfo
	{
		public int startIndex;

		public int endIndex;

		public string name;

		public readonly List<Rect> boxes = new List<Rect>();
	}

	private readonly List<Image> m_ImagesPool = new List<Image>();

	private readonly List<GameObject> culled_ImagesPool = new List<GameObject>();

	private bool clearImages;

	private Object thisLock = new Object();

	private readonly List<int> m_ImagesVertexIndex = new List<int>();

	private static readonly Regex s_Regex = new Regex("<quad name=(.+?) size=(\\d*\\.?\\d+%?) width=(\\d*\\.?\\d+%?) />", RegexOptions.Singleline);

	private string fixedString;

	[SerializeField]
	[Tooltip("Allow click events to be received by parents, (default) blocks")]
	private bool m_ClickParents;

	private string m_OutputText;

	public IconName[] inspectorIconList;

	[Tooltip("Global scaling factor for all images")]
	public float ImageScalingFactor = 1f;

	public string hyperlinkColor = "blue";

	[SerializeField]
	public Vector2 imageOffset = Vector2.zero;

	private Button button;

	private Selectable highlightselectable;

	private List<Vector2> positions = new List<Vector2>();

	private string previousText = "";

	public bool isCreating_m_HrefInfos = true;

	private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

	private static readonly StringBuilder s_TextBuilder = new StringBuilder();

	private static readonly Regex s_HrefRegex = new Regex("<a href=([^>\\n\\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

	[SerializeField]
	private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

	public bool AllowClickParents
	{
		get
		{
			return m_ClickParents;
		}
		set
		{
			m_ClickParents = value;
		}
	}

	public HrefClickEvent onHrefClick
	{
		get
		{
			return m_OnHrefClick;
		}
		set
		{
			m_OnHrefClick = value;
		}
	}

	public override void SetVerticesDirty()
	{
		base.SetVerticesDirty();
		UpdateQuadImage();
	}

	private new void Start()
	{
		button = GetComponentInParent<Button>();
		if (button != null)
		{
			CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			}
			canvasGroup.blocksRaycasts = false;
			highlightselectable = canvasGroup.GetComponent<Selectable>();
		}
		else
		{
			highlightselectable = GetComponent<Selectable>();
		}
		Reset_m_HrefInfos();
		base.Start();
	}

	protected void UpdateQuadImage()
	{
		m_OutputText = GetOutputText();
		m_ImagesVertexIndex.Clear();
		foreach (Match item2 in s_Regex.Matches(m_OutputText))
		{
			int item = item2.Index * 4 + 3;
			m_ImagesVertexIndex.Add(item);
			m_ImagesPool.RemoveAll((Image image2) => image2 == null);
			if (m_ImagesPool.Count == 0)
			{
				GetComponentsInChildren(m_ImagesPool);
			}
			if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
			{
				GameObject gameObject = DefaultControls.CreateImage(default(DefaultControls.Resources));
				gameObject.layer = base.gameObject.layer;
				RectTransform rectTransform = gameObject.transform as RectTransform;
				if ((bool)rectTransform)
				{
					rectTransform.SetParent(base.rectTransform);
					rectTransform.localPosition = Vector3.zero;
					rectTransform.localRotation = Quaternion.identity;
					rectTransform.localScale = Vector3.one;
				}
				m_ImagesPool.Add(gameObject.GetComponent<Image>());
			}
			string value = item2.Groups[1].Value;
			Image image = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
			Vector2 vector = Vector2.zero;
			if ((image.sprite == null || image.sprite.name != value) && inspectorIconList != null && inspectorIconList.Length != 0)
			{
				IconName[] array = inspectorIconList;
				for (int num = 0; num < array.Length; num++)
				{
					IconName iconName = array[num];
					if (iconName.name == value)
					{
						image.sprite = iconName.sprite;
						image.rectTransform.sizeDelta = new Vector2((float)base.fontSize * ImageScalingFactor * iconName.scale.x, (float)base.fontSize * ImageScalingFactor * iconName.scale.y);
						vector = iconName.offset;
						break;
					}
				}
			}
			image.enabled = true;
			if (positions.Count == m_ImagesPool.Count)
			{
				image.rectTransform.anchoredPosition = (positions[m_ImagesVertexIndex.Count - 1] += vector);
			}
		}
		for (int num2 = m_ImagesVertexIndex.Count; num2 < m_ImagesPool.Count; num2++)
		{
			if ((bool)m_ImagesPool[num2])
			{
				m_ImagesPool[num2].gameObject.SetActive(value: false);
				m_ImagesPool[num2].gameObject.hideFlags = HideFlags.HideAndDontSave;
				culled_ImagesPool.Add(m_ImagesPool[num2].gameObject);
				m_ImagesPool.Remove(m_ImagesPool[num2]);
			}
		}
		if (culled_ImagesPool.Count > 1)
		{
			clearImages = true;
		}
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		string text = m_Text;
		m_Text = GetOutputText();
		base.OnPopulateMesh(toFill);
		m_Text = text;
		positions.Clear();
		UIVertex vertex = default(UIVertex);
		for (int i = 0; i < m_ImagesVertexIndex.Count; i++)
		{
			int num = m_ImagesVertexIndex[i];
			Vector2 sizeDelta = m_ImagesPool[i].rectTransform.sizeDelta;
			if (num < toFill.currentVertCount)
			{
				toFill.PopulateUIVertex(ref vertex, num);
				positions.Add(new Vector2(vertex.position.x + sizeDelta.x / 2f, vertex.position.y + sizeDelta.y / 2f) + imageOffset);
				toFill.PopulateUIVertex(ref vertex, num - 3);
				Vector3 position = vertex.position;
				int num2 = num;
				int num3 = num - 3;
				while (num2 > num3)
				{
					toFill.PopulateUIVertex(ref vertex, num);
					vertex.position = position;
					toFill.SetUIVertex(vertex, num2);
					num2--;
				}
			}
		}
		if (m_ImagesVertexIndex.Count != 0)
		{
			m_ImagesVertexIndex.Clear();
		}
		foreach (HrefInfo hrefInfo in m_HrefInfos)
		{
			hrefInfo.boxes.Clear();
			if (hrefInfo.startIndex >= toFill.currentVertCount)
			{
				continue;
			}
			toFill.PopulateUIVertex(ref vertex, hrefInfo.startIndex);
			Vector3 position2 = vertex.position;
			Bounds bounds = new Bounds(position2, Vector3.zero);
			int j = hrefInfo.startIndex;
			for (int endIndex = hrefInfo.endIndex; j < endIndex && j < toFill.currentVertCount; j++)
			{
				toFill.PopulateUIVertex(ref vertex, j);
				position2 = vertex.position;
				if (position2.x < bounds.min.x)
				{
					hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
					bounds = new Bounds(position2, Vector3.zero);
				}
				else
				{
					bounds.Encapsulate(position2);
				}
			}
			hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
		}
		UpdateQuadImage();
	}

	protected string GetOutputText()
	{
		s_TextBuilder.Length = 0;
		int num = 0;
		fixedString = text;
		if (inspectorIconList != null && inspectorIconList.Length != 0)
		{
			IconName[] array = inspectorIconList;
			for (int i = 0; i < array.Length; i++)
			{
				IconName iconName = array[i];
				if (iconName.name != null && iconName.name != "")
				{
					fixedString = fixedString.Replace(iconName.name, $"<quad name={iconName.name} size={base.fontSize} width=1 />");
				}
			}
		}
		int num2 = 0;
		foreach (Match item2 in s_HrefRegex.Matches(fixedString))
		{
			s_TextBuilder.Append(fixedString.Substring(num, item2.Index - num));
			s_TextBuilder.Append("<color=" + hyperlinkColor + ">");
			Group obj = item2.Groups[1];
			if (isCreating_m_HrefInfos)
			{
				HrefInfo item = new HrefInfo
				{
					startIndex = s_TextBuilder.Length * 4,
					endIndex = (s_TextBuilder.Length + item2.Groups[2].Length - 1) * 4 + 3,
					name = obj.Value
				};
				m_HrefInfos.Add(item);
			}
			else if (m_HrefInfos.Count > 0)
			{
				m_HrefInfos[num2].startIndex = s_TextBuilder.Length * 4;
				m_HrefInfos[num2].endIndex = (s_TextBuilder.Length + item2.Groups[2].Length - 1) * 4 + 3;
				num2++;
			}
			s_TextBuilder.Append(item2.Groups[2].Value);
			s_TextBuilder.Append("</color>");
			num = item2.Index + item2.Length;
		}
		if (isCreating_m_HrefInfos)
		{
			isCreating_m_HrefInfos = false;
		}
		s_TextBuilder.Append(fixedString.Substring(num, fixedString.Length - num));
		return s_TextBuilder.ToString();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
		foreach (HrefInfo hrefInfo in m_HrefInfos)
		{
			List<Rect> boxes = hrefInfo.boxes;
			for (int i = 0; i < boxes.Count; i++)
			{
				if (boxes[i].Contains(localPoint))
				{
					m_OnHrefClick.Invoke(hrefInfo.name);
					return;
				}
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		foreach (Image item in m_ImagesPool)
		{
			if (highlightselectable != null && highlightselectable.isActiveAndEnabled)
			{
				item.color = highlightselectable.colors.highlightedColor;
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		foreach (Image item in m_ImagesPool)
		{
			if (highlightselectable != null && highlightselectable.isActiveAndEnabled)
			{
				item.color = highlightselectable.colors.normalColor;
			}
			else
			{
				item.color = color;
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		foreach (Image item in m_ImagesPool)
		{
			if (highlightselectable != null && highlightselectable.isActiveAndEnabled)
			{
				item.color = highlightselectable.colors.highlightedColor;
			}
		}
	}

	private void Update()
	{
		lock (thisLock)
		{
			if (clearImages)
			{
				for (int i = 0; i < culled_ImagesPool.Count; i++)
				{
					Object.DestroyImmediate(culled_ImagesPool[i]);
				}
				culled_ImagesPool.Clear();
				clearImages = false;
			}
		}
		if (previousText != text)
		{
			Reset_m_HrefInfos();
		}
	}

	private void Reset_m_HrefInfos()
	{
		previousText = text;
		m_HrefInfos.Clear();
		isCreating_m_HrefInfos = true;
	}
}
