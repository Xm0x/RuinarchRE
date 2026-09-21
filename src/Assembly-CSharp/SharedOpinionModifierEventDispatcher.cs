using System;

public class SharedOpinionModifierEventDispatcher
{
	public interface IExpiryListener
	{
		void OnSharedOpinionModifierExpired(SharedOpinionModifier p_modifier);
	}

	private Action<SharedOpinionModifier> _modifierExpired;

	public void SubscribeToExpiryEvent(IExpiryListener p_listener)
	{
		_modifierExpired = (Action<SharedOpinionModifier>)Delegate.Combine(_modifierExpired, new Action<SharedOpinionModifier>(p_listener.OnSharedOpinionModifierExpired));
	}

	public void UnsubscribeToExpiryEvent(IExpiryListener p_listener)
	{
		_modifierExpired = (Action<SharedOpinionModifier>)Delegate.Remove(_modifierExpired, new Action<SharedOpinionModifier>(p_listener.OnSharedOpinionModifierExpired));
	}

	public void ExecuteModifierExpired(SharedOpinionModifier p_modifier)
	{
		_modifierExpired?.Invoke(p_modifier);
		DatabaseManager.Instance.sharedOpinionDatabase.RemoveSharedOpinion(p_modifier);
	}
}
