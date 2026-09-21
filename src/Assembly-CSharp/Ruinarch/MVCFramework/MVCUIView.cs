using UnityEngine;

namespace Ruinarch.MVCFramework;

public abstract class MVCUIView : MonoBehaviour
{
	protected MVCUIModel _baseAssetModel;

	public bool hasBeenInitialized;

	protected virtual void Init(Canvas canvas, MVCUIModel assets)
	{
		_baseAssetModel = assets;
		_baseAssetModel.transform.SetParent(canvas.transform, worldPositionStays: false);
		hasBeenInitialized = true;
	}

	public virtual void Destroy()
	{
		Object.Destroy(base.gameObject);
		if (_baseAssetModel != null)
		{
			Object.Destroy(_baseAssetModel.gameObject);
			_baseAssetModel = null;
		}
	}

	public virtual void HideUI()
	{
		_baseAssetModel.parentDisplay.gameObject.SetActive(value: false);
	}

	public virtual void ShowUI()
	{
		_baseAssetModel.parentDisplay.gameObject.SetActive(value: true);
	}

	public void MakeLastSibling()
	{
		_baseAssetModel.transform.SetAsLastSibling();
	}
}
