using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Extensions.Examples;

public class Example02Scene : MonoBehaviour
{
	[SerializeField]
	private Example02ScrollView scrollView;

	private void Start()
	{
		List<Example02CellDto> data = (from i in Enumerable.Range(0, 20)
			select new Example02CellDto
			{
				Message = $"Cell {i}"
			}).ToList();
		scrollView.UpdateData(data);
	}
}
