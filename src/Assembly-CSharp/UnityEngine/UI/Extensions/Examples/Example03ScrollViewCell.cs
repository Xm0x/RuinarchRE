namespace UnityEngine.UI.Extensions.Examples;

public class Example03ScrollViewCell : FancyScrollViewCell<Example03CellDto, Example03ScrollViewContext>
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Text message;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Button button;

	private readonly int scrollTriggerHash = Animator.StringToHash("scroll");

	private Example03ScrollViewContext context;

	private void Start()
	{
		RectTransform obj = base.transform as RectTransform;
		obj.anchorMax = Vector2.one;
		obj.anchorMin = Vector2.zero;
		obj.anchoredPosition3D = Vector3.zero;
		UpdatePosition(0f);
		button.onClick.AddListener(OnPressedCell);
	}

	public override void SetContext(Example03ScrollViewContext context)
	{
		this.context = context;
	}

	public override void UpdateContent(Example03CellDto itemData)
	{
		message.text = itemData.Message;
		if (context != null)
		{
			bool flag = context.SelectedIndex == base.DataIndex;
			image.color = (flag ? new Color32(0, byte.MaxValue, byte.MaxValue, 100) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 77));
		}
	}

	public override void UpdatePosition(float position)
	{
		animator.Play(scrollTriggerHash, -1, position);
		animator.speed = 0f;
	}

	public void OnPressedCell()
	{
		if (context != null)
		{
			context.OnPressedCell(this);
		}
	}
}
