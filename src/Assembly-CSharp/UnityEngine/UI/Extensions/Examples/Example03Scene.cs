using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Extensions.Examples;

public class Example03Scene : MonoBehaviour
{
	[SerializeField]
	private Example03ScrollView scrollView;

	private void Start()
	{
		List<Example03CellDto> data = (from i in Enumerable.Range(0, 20)
			select new Example03CellDto
			{
				Message = $"Cell {i}"
			}).ToList();
		scrollView.UpdateData(data);
	}
}
