namespace UnityEngine.UI.Extensions;

public class FancyScrollViewCell<TData, TContext> : MonoBehaviour where TContext : class
{
	public int DataIndex { get; set; }

	public virtual void SetContext(TContext context)
	{
	}

	public virtual void UpdateContent(TData itemData)
	{
	}

	public virtual void UpdatePosition(float position)
	{
	}

	public virtual void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}
}
public class FancyScrollViewCell<TData> : FancyScrollViewCell<TData, FancyScrollViewNullContext>
{
}
