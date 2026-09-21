using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
	public float UpdateInterval = 1f;

	private Text text;

	private int frames;

	private void Start()
	{
		text = GetComponent<Text>();
		InvokeRepeating("UpdateFPS", UpdateInterval, UpdateInterval);
	}

	private void UpdateFPS()
	{
		if (frames < 30)
		{
			text.color = Color.red;
		}
		if (frames >= 30 && frames <= 50)
		{
			text.color = Color.yellow;
		}
		else
		{
			text.color = Color.green;
		}
		text.text = $"FPS: {frames}";
		frames = 0;
	}

	private void Update()
	{
		frames++;
	}
}
