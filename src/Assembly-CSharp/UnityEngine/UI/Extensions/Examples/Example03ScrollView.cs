using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.Examples;

public class Example03ScrollView : FancyScrollView<Example03CellDto, Example03ScrollViewContext>
{
	[SerializeField]
	private ScrollPositionController scrollPositionController;

	private new void Awake()
	{
		scrollPositionController.OnUpdatePosition.AddListener(base.UpdatePosition);
		scrollPositionController.OnItemSelected.AddListener(CellSelected);
		SetContext(new Example03ScrollViewContext
		{
			OnPressedCell = OnPressedCell
		});
		base.Awake();
	}

	public void UpdateData(List<Example03CellDto> data)
	{
		cellData = data;
		scrollPositionController.SetDataCount(cellData.Count);
		UpdateContents();
	}

	private void OnPressedCell(Example03ScrollViewCell cell)
	{
		scrollPositionController.ScrollTo(cell.DataIndex, 0.4f);
		context.SelectedIndex = cell.DataIndex;
		UpdateContents();
	}

	private void CellSelected(int cellIndex)
	{
		context.SelectedIndex = cellIndex;
		UpdateContents();
	}
}
