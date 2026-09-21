using UnityEngine;

public interface ISelectable
{
	Vector3 worldPosition { get; }

	Vector2 selectableSize { get; }

	bool IsCurrentlySelected();

	void LeftSelectAction();

	void RightSelectAction();

	void MiddleSelectAction();

	bool CanBeSelected();
}
