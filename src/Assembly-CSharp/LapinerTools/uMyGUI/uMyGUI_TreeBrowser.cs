using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI;

public class uMyGUI_TreeBrowser : MonoBehaviour
{
	public class Node
	{
		public readonly object SendMessageData;

		public readonly Node[] Children;

		public Node(object p_sendMessageData, Node[] p_children)
		{
			SendMessageData = p_sendMessageData;
			Children = p_children;
		}
	}

	public class NodeClickEventArgs : EventArgs
	{
		public readonly Node ClickedNode;

		public NodeClickEventArgs(Node p_clickedNode)
		{
			ClickedNode = p_clickedNode;
		}
	}

	public class NodeInstantiateEventArgs : EventArgs
	{
		public readonly Node Node;

		public readonly GameObject Instance;

		public NodeInstantiateEventArgs(Node p_node, GameObject p_instance)
		{
			Node = p_node;
			Instance = p_instance;
		}
	}

	private class InternalNode
	{
		public readonly Node m_node;

		public GameObject m_instance;

		public int m_indentLevel;

		public RectTransform m_transform;

		public bool m_isFoldout;

		public float m_minY;

		public InternalNode(Node p_node, GameObject p_instance, int p_indentLevel)
		{
			m_node = p_node;
			m_instance = p_instance;
			m_indentLevel = p_indentLevel;
			m_transform = m_instance.GetComponent<RectTransform>();
		}
	}

	[SerializeField]
	private GameObject m_innerNodePrefab;

	[SerializeField]
	private GameObject m_leafNodePrefab;

	[SerializeField]
	private float m_offsetStart;

	[SerializeField]
	private float m_offsetEnd;

	[SerializeField]
	private float m_padding = 4f;

	[SerializeField]
	private float m_indentSize = 20f;

	[SerializeField]
	private float m_forcedEntryHeight;

	[SerializeField]
	private bool m_useExplicitNavigation;

	[SerializeField]
	private float m_navScrollSpeed = 200f;

	[SerializeField]
	private float m_navScrollSmooth = 20f;

	private ScrollRect m_parentScroller;

	public EventHandler<NodeClickEventArgs> OnInnerNodeClick;

	public EventHandler<NodeClickEventArgs> OnLeafNodeClick;

	public EventHandler<NodeClickEventArgs> OnLeafNodePointerDown;

	public EventHandler<NodeInstantiateEventArgs> OnNodeInstantiate;

	private RectTransform m_rectTransform;

	private List<InternalNode> m_nodes = new List<InternalNode>();

	private GameObject m_lastSelectedGO;

	public GameObject InnerNodePrefab
	{
		get
		{
			return m_innerNodePrefab;
		}
		set
		{
			m_innerNodePrefab = value;
		}
	}

	public GameObject LeafNodePrefab
	{
		get
		{
			return m_leafNodePrefab;
		}
		set
		{
			m_leafNodePrefab = value;
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

	public float IndentSize
	{
		get
		{
			return m_indentSize;
		}
		set
		{
			m_indentSize = value;
		}
	}

	public float ForcedEntryHeight
	{
		get
		{
			return m_forcedEntryHeight;
		}
		set
		{
			m_forcedEntryHeight = value;
		}
	}

	public bool UseExplicitNavigation
	{
		get
		{
			return m_useExplicitNavigation;
		}
		set
		{
			m_useExplicitNavigation = value;
		}
	}

	public float NavScrollSpeed
	{
		get
		{
			return m_navScrollSpeed;
		}
		set
		{
			m_navScrollSpeed = value;
		}
	}

	public float NavScrollSmooth
	{
		get
		{
			return m_navScrollSmooth;
		}
		set
		{
			m_navScrollSmooth = value;
		}
	}

	public ScrollRect ParentScroller => m_parentScroller;

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

	public void BuildTree(Node[] p_rootNodes)
	{
		BuildTree(p_rootNodes, 0, 0);
	}

	public void BuildTree(Node[] p_rootNodes, int p_insertAt, int p_indentLevel)
	{
		if (m_innerNodePrefab != null && m_leafNodePrefab != null)
		{
			List<InternalNode> list = new List<InternalNode>();
			float num = 0f;
			float p_currY = ((m_nodes.Count >= p_insertAt && p_insertAt > 0) ? m_nodes[p_insertAt - 1].m_minY : (0f - m_offsetStart));
			for (int i = 0; i < p_rootNodes.Length; i++)
			{
				if (p_rootNodes[i] == null)
				{
					continue;
				}
				int num2;
				GameObject gameObject;
				if (p_rootNodes[i].Children != null)
				{
					num2 = ((p_rootNodes[i].Children.Length != 0) ? 1 : 0);
					if (num2 != 0)
					{
						gameObject = UnityEngine.Object.Instantiate(m_innerNodePrefab);
						goto IL_00a4;
					}
				}
				else
				{
					num2 = 0;
				}
				gameObject = UnityEngine.Object.Instantiate(m_leafNodePrefab);
				goto IL_00a4;
				IL_00a4:
				RectTransform component = gameObject.GetComponent<RectTransform>();
				if (m_forcedEntryHeight != 0f)
				{
					component.sizeDelta = new Vector2(component.sizeDelta.x, m_forcedEntryHeight);
				}
				float height = component.rect.height;
				if (p_rootNodes[i].SendMessageData != null)
				{
					if (!gameObject.activeInHierarchy)
					{
						Debug.LogError("uMyGUI_TreeBrowser: BuildTree: node has SendMessageData set, but instance is inactive! SendMessage call will fail! Make your prefab active!");
					}
					gameObject.SendMessage("uMyGUI_TreeBrowser_InitNode", p_rootNodes[i].SendMessageData);
				}
				InternalNode internalNode = new InternalNode(p_rootNodes[i], gameObject, p_indentLevel);
				list.Add(internalNode);
				if (num2 != 0)
				{
					SetupInnerNode(internalNode);
				}
				else
				{
					SetupLeafNode(internalNode);
				}
				p_currY = SetRectTransformPosition(component, p_currY, height, p_indentLevel);
				internalNode.m_minY = component.anchoredPosition.y - height;
				num = internalNode.m_minY;
				if (OnNodeInstantiate != null)
				{
					OnNodeInstantiate(this, new NodeInstantiateEventArgs(p_rootNodes[i], gameObject));
				}
			}
			if (p_insertAt < m_nodes.Count)
			{
				float p_moveDist = ((p_insertAt != 0) ? (num - m_nodes[p_insertAt - 1].m_minY) : num);
				UpdateNodePosition(p_insertAt, p_moveDist);
			}
			if (p_insertAt < m_nodes.Count)
			{
				m_nodes.InsertRange(p_insertAt, list);
			}
			else
			{
				m_nodes.AddRange(list);
			}
			if (m_nodes.Count > 0)
			{
				RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(m_nodes[m_nodes.Count - 1].m_minY - RTransform.rect.yMax - m_offsetEnd));
			}
			if (m_useExplicitNavigation)
			{
				SetExplicitNavigationTargets();
			}
		}
		else
		{
			Debug.LogError("uMyGUI_TreeBrowser: BuildTree: you must provide the InnerNodePrefab and LeafNodePrefab in the inspector or via script!");
		}
	}

	public void Clear()
	{
		for (int i = 0; i < m_nodes.Count; i++)
		{
			UnityEngine.Object.Destroy(m_nodes[i].m_instance);
		}
		m_nodes.Clear();
	}

	private void Start()
	{
		m_parentScroller = GetComponentInParent<ScrollRect>();
	}

	private void LateUpdate()
	{
		if (m_parentScroller == null)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		GameObject currentSelectedGameObject;
		if (current != null && (currentSelectedGameObject = current.currentSelectedGameObject) != null && currentSelectedGameObject.transform.IsChildOf(base.transform))
		{
			if (!(currentSelectedGameObject != m_lastSelectedGO))
			{
				return;
			}
			m_lastSelectedGO = currentSelectedGameObject;
			Transform parent = currentSelectedGameObject.transform;
			while (parent.parent != base.transform && parent.parent != null)
			{
				parent = parent.parent;
			}
			RectTransform component = parent.GetComponent<RectTransform>();
			if (component == null)
			{
				return;
			}
			Vector3[] array = new Vector3[4];
			component.GetWorldCorners(array);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = m_parentScroller.transform.InverseTransformPoint(array[i]);
			}
			Vector3 vector = Vector3.Min(Vector3.Min(array[0], array[1]), Vector3.Min(array[2], array[3]));
			Vector3 vector2 = Vector3.Max(Vector3.Max(array[0], array[1]), Vector3.Max(array[2], array[3]));
			m_parentScroller.GetComponent<RectTransform>().GetLocalCorners(array);
			Vector3 vector3 = Vector3.Min(Vector3.Min(array[0], array[1]), Vector3.Min(array[2], array[3]));
			Vector3 vector4 = Vector3.Max(Vector3.Max(array[0], array[1]), Vector3.Max(array[2], array[3]));
			if (vector.y < vector3.y)
			{
				if (m_parentScroller.verticalNormalizedPosition >= 1f)
				{
					m_parentScroller.verticalNormalizedPosition = 0.999f;
				}
				m_parentScroller.velocity = Vector3.up * Mathf.Max(5f, m_navScrollSpeed * ((m_navScrollSmooth != 0f) ? ((vector3.y - vector.y) / m_navScrollSmooth) : 1f));
				m_lastSelectedGO = null;
			}
			else if (vector2.y > vector4.y)
			{
				if (m_parentScroller.verticalNormalizedPosition <= 0f)
				{
					m_parentScroller.verticalNormalizedPosition = 0.001f;
				}
				m_parentScroller.velocity = Vector3.down * Mathf.Max(5f, m_navScrollSpeed * ((m_navScrollSmooth != 0f) ? ((vector2.y - vector4.y) / m_navScrollSmooth) : 1f));
				m_lastSelectedGO = null;
			}
		}
		else
		{
			m_lastSelectedGO = null;
		}
	}

	private void OnDestroy()
	{
		OnInnerNodeClick = null;
		OnLeafNodeClick = null;
		OnNodeInstantiate = null;
	}

	private void SetExplicitNavigationTargets()
	{
		if (m_nodes.Count <= 2)
		{
			return;
		}
		RectTransform rectTransform = m_nodes[0].m_transform;
		RectTransform rectTransform2 = m_nodes[1].m_transform;
		Selectable[] array = rectTransform.GetComponentsInChildren<Selectable>();
		Selectable[] componentsInChildren = rectTransform2.GetComponentsInChildren<Selectable>();
		SetAutomaticNavigation(m_nodes[0].m_transform);
		SetAutomaticNavigation(m_nodes[m_nodes.Count - 1].m_transform);
		for (int i = 1; i < m_nodes.Count - 1; i++)
		{
			RectTransform rectTransform3 = rectTransform;
			rectTransform = rectTransform2;
			rectTransform2 = m_nodes[i + 1].m_transform;
			Selectable[] array2 = array;
			array = componentsInChildren;
			componentsInChildren = rectTransform2.GetComponentsInChildren<Selectable>();
			if (rectTransform3 != null && rectTransform != null && rectTransform2 != null && array2.Length == array.Length && componentsInChildren.Length == array.Length)
			{
				for (int j = 0; j < array.Length; j++)
				{
					Navigation navigation = array[j].navigation;
					navigation.mode = Navigation.Mode.Explicit;
					navigation.selectOnUp = array2[j];
					navigation.selectOnDown = componentsInChildren[j];
					array[j].navigation = navigation;
				}
			}
		}
	}

	private void SetAutomaticNavigation(RectTransform p_nodeTransform)
	{
		if (p_nodeTransform != null)
		{
			Selectable[] componentsInChildren = p_nodeTransform.GetComponentsInChildren<Selectable>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Navigation navigation = componentsInChildren[i].navigation;
				navigation.mode = Navigation.Mode.Automatic;
				componentsInChildren[i].navigation = navigation;
			}
		}
	}

	private float SetRectTransformPosition(RectTransform p_transform, float p_currY, float p_size, int p_indentLevel)
	{
		p_transform.SetParent(RTransform, worldPositionStays: false);
		Vector2 anchoredPosition = p_transform.anchoredPosition;
		anchoredPosition.x += (float)p_indentLevel * m_indentSize;
		anchoredPosition.y += p_currY;
		p_currY -= m_padding + p_size;
		p_transform.anchoredPosition = anchoredPosition;
		return p_currY;
	}

	private void UpdateNodePosition(int p_startIndex, float p_moveDist)
	{
		for (int i = p_startIndex; i < m_nodes.Count; i++)
		{
			Vector2 anchoredPosition = m_nodes[i].m_transform.anchoredPosition;
			anchoredPosition.y += p_moveDist;
			m_nodes[i].m_transform.anchoredPosition = anchoredPosition;
			m_nodes[i].m_minY = anchoredPosition.y - m_nodes[i].m_transform.rect.height;
		}
	}

	private void SetupInnerNode(InternalNode p_node)
	{
		if (p_node.m_instance.GetComponent<Button>() != null)
		{
			p_node.m_instance.GetComponent<Button>().onClick.AddListener(delegate
			{
				ToggleInnerNodeFoldout(p_node);
			});
		}
		else if (p_node.m_instance.GetComponent<Toggle>() != null)
		{
			p_node.m_instance.GetComponent<Toggle>().onValueChanged.AddListener(delegate
			{
				ToggleInnerNodeFoldout(p_node);
			});
		}
		else
		{
			Debug.LogError("uMyGUI_TreeBrowser: BuildTree: the inner node prefabs must have either a Button or a Toggle script attached to the root. Otherwise they cannot fold out!");
		}
	}

	private void SetupLeafNode(InternalNode p_node)
	{
		if (p_node.m_instance.GetComponent<Button>() != null)
		{
			p_node.m_instance.GetComponent<Button>().onClick.AddListener(delegate
			{
				SafeCallOnLeafNodeClick(p_node);
			});
		}
		else if (p_node.m_instance.GetComponent<Toggle>() != null)
		{
			p_node.m_instance.GetComponent<Toggle>().onValueChanged.AddListener(delegate
			{
				SafeCallOnLeafNodeClick(p_node);
			});
		}
		EventTrigger eventTrigger = p_node.m_instance.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = p_node.m_instance.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(delegate
		{
			SafeCallOnLeafNodePointerDown(p_node);
		});
		entry.callback = triggerEvent;
		if (eventTrigger.triggers == null)
		{
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		eventTrigger.triggers.Add(entry);
	}

	private void ToggleInnerNodeFoldout(InternalNode p_node)
	{
		int num = m_nodes.IndexOf(p_node);
		p_node.m_isFoldout = !p_node.m_isFoldout;
		if (p_node.m_isFoldout)
		{
			BuildTree(p_node.m_node.Children, num + 1, p_node.m_indentLevel + 1);
		}
		else
		{
			float num2 = 0f;
			for (int i = 0; i < p_node.m_node.Children.Length; i++)
			{
				int index = num + p_node.m_node.Children.Length - i;
				InternalNode internalNode = m_nodes[index];
				num2 += internalNode.m_transform.rect.height;
				if (i + 1 < p_node.m_node.Children.Length)
				{
					num2 += m_padding;
				}
				if (internalNode.m_isFoldout)
				{
					ToggleInnerNodeFoldout(internalNode);
				}
				m_nodes.RemoveAt(index);
				UnityEngine.Object.Destroy(internalNode.m_instance);
			}
			UpdateNodePosition(num + 1, num2);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RTransform.sizeDelta.y - num2);
		}
		if (OnInnerNodeClick != null)
		{
			OnInnerNodeClick(this, new NodeClickEventArgs(p_node.m_node));
		}
	}

	private void SafeCallOnLeafNodePointerDown(InternalNode p_node)
	{
		if (OnLeafNodePointerDown != null)
		{
			OnLeafNodePointerDown(this, new NodeClickEventArgs(p_node.m_node));
		}
	}

	private void SafeCallOnLeafNodeClick(InternalNode p_node)
	{
		if (OnLeafNodeClick != null)
		{
			OnLeafNodeClick(this, new NodeClickEventArgs(p_node.m_node));
		}
	}
}
