using System;
using Inner_Maps;
using Ruinarch;
using UnityEngine;

public class FireBallData : SkillData
{
	private LocationGridTile m_targetTile;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FIRE_BALL;

	public override string name => "Fire Ball";

	public override string description => "This Spell spawns a floating ball of fire that will move around randomly for a few hours, dealing Fire damage to everything in its path.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public FireBallData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		m_targetTile = targetTile;
		Vector3 position = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
		position.z = 0f;
		PointerDirection component = ObjectPoolManager.Instance.InstantiateObjectFromPool("PointerDirection", position, Quaternion.identity).GetComponent<PointerDirection>();
		component.OnClick = (Action<PointerDirection, Vector3>)Delegate.Combine(component.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
		component.OnCancel = (Action<PointerDirection>)Delegate.Combine(component.OnCancel, new Action<PointerDirection>(OnCancel));
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
	}

	private void OnPointerClick(PointerDirection p_pointer, Vector3 p_position)
	{
		p_pointer.OnClick = (Action<PointerDirection, Vector3>)Delegate.Remove(p_pointer.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
		p_pointer.OnCancel = (Action<PointerDirection>)Delegate.Remove(p_pointer.OnCancel, new Action<PointerDirection>(OnCancel));
		FireBall fireBall = new FireBall();
		fireBall.targetDirection = p_position;
		fireBall.SetGridTileLocation(m_targetTile);
		fireBall.OnPlacePOI();
		fireBall.SetIsPlayerSource(p_state: true);
		ObjectPoolManager.Instance.DestroyObject(p_pointer.gameObject);
		base.ActivateAbility(m_targetTile);
	}

	private void OnCancel(PointerDirection p_pointer)
	{
		p_pointer.OnClick = (Action<PointerDirection, Vector3>)Delegate.Remove(p_pointer.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
		p_pointer.OnCancel = (Action<PointerDirection>)Delegate.Remove(p_pointer.OnCancel, new Action<PointerDirection>(OnCancel));
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		ObjectPoolManager.Instance.DestroyObject(p_pointer.gameObject);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return targetTile.structure != null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
