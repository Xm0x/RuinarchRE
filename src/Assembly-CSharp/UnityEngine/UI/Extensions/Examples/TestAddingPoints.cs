using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.Examples;

public class TestAddingPoints : MonoBehaviour
{
	public UILineRenderer LineRenderer;

	public Text XValue;

	public Text YValue;

	public void AddNewPoint()
	{
		Vector2 item = new Vector2
		{
			x = float.Parse(XValue.text),
			y = float.Parse(YValue.text)
		};
		List<Vector2> list = new List<Vector2>(LineRenderer.Points);
		list.Add(item);
		LineRenderer.Points = list.ToArray();
	}

	public void ClearPoints()
	{
		LineRenderer.Points = new Vector2[0];
	}
}
