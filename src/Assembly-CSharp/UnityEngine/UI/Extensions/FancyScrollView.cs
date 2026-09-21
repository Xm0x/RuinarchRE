using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

public class FancyScrollView<TData, TContext> : MonoBehaviour where TContext : class
{
	[SerializeField]
	[Range(float.Epsilon, 1f)]
	private float cellInterval;

	[SerializeField]
	[Range(0f, 1f)]
	private float cellOffset;

	[SerializeField]
	private bool loop;

	[SerializeField]
	private GameObject cellBase;

	private float currentPosition;

	private readonly List<FancyScrollViewCell<TData, TContext>> cells = new List<FancyScrollViewCell<TData, TContext>>();

	protected TContext context;

	protected List<TData> cellData = new List<TData>();

	protected void Awake()
	{
		cellBase.SetActive(value: false);
	}

	protected void SetContext(TContext context)
	{
		this.context = context;
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].SetContext(context);
		}
	}

	private FancyScrollViewCell<TData, TContext> CreateCell()
	{
		GameObject obj = Object.Instantiate(cellBase);
		obj.SetActive(value: true);
		FancyScrollViewCell<TData, TContext> component = obj.GetComponent<FancyScrollViewCell<TData, TContext>>();
		RectTransform rectTransform = component.transform as RectTransform;
		Vector3 localScale = component.transform.localScale;
		Vector2 sizeDelta = Vector2.zero;
		Vector2 offsetMin = Vector2.zero;
		Vector2 offsetMax = Vector2.zero;
		if ((bool)rectTransform)
		{
			sizeDelta = rectTransform.sizeDelta;
			offsetMin = rectTransform.offsetMin;
			offsetMax = rectTransform.offsetMax;
		}
		component.transform.SetParent(cellBase.transform.parent);
		component.transform.localScale = localScale;
		if ((bool)rectTransform)
		{
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.offsetMin = offsetMin;
			rectTransform.offsetMax = offsetMax;
		}
		component.SetContext(context);
		component.SetVisible(visible: false);
		return component;
	}

	private void UpdateCellForIndex(FancyScrollViewCell<TData, TContext> cell, int dataIndex)
	{
		if (loop)
		{
			dataIndex = GetLoopIndex(dataIndex, cellData.Count);
		}
		else if (dataIndex < 0 || dataIndex > cellData.Count - 1)
		{
			cell.SetVisible(visible: false);
			return;
		}
		cell.SetVisible(visible: true);
		cell.DataIndex = dataIndex;
		cell.UpdateContent(cellData[dataIndex]);
	}

	private int GetLoopIndex(int index, int length)
	{
		if (index < 0)
		{
			index = length - 1 + (index + 1) % length;
		}
		else if (index > length - 1)
		{
			index %= length;
		}
		return index;
	}

	protected void UpdateContents()
	{
		UpdatePosition(currentPosition);
	}

	protected void UpdatePosition(float position)
	{
		currentPosition = position;
		float num = position - cellOffset / cellInterval;
		float num2 = (Mathf.Ceil(num) - num) * cellInterval;
		int num3 = Mathf.CeilToInt(num);
		int num4 = 0;
		int num5 = 0;
		float num6 = num2;
		while (num6 <= 1f)
		{
			if (num4 >= cells.Count)
			{
				cells.Add(CreateCell());
			}
			num6 += cellInterval;
			num4++;
		}
		num4 = 0;
		for (float num7 = num2; num7 <= 1f; num7 += cellInterval)
		{
			int num8 = num3 + num4;
			num5 = GetLoopIndex(num8, cells.Count);
			if (cells[num5].gameObject.activeSelf)
			{
				cells[num5].UpdatePosition(num7);
			}
			UpdateCellForIndex(cells[num5], num8);
			num4++;
		}
		num5 = GetLoopIndex(num3 + num4, cells.Count);
		while (num4 < cells.Count)
		{
			cells[num5].SetVisible(visible: false);
			num4++;
			num5 = GetLoopIndex(num3 + num4, cells.Count);
		}
	}
}
public class FancyScrollView<TData> : FancyScrollView<TData, FancyScrollViewNullContext>
{
}
