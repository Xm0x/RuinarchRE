using System;
using System.Diagnostics;
using UnityEngine.Localization;

public class LocalizationManagerEventDispatcher
{
	public interface ILocaleChangeListener
	{
		void OnLocaleChanged(Locale locale);
	}

	private Action<Locale> _onLocaleChanged;

	[Conditional("LOCALIZATION_TESTER")]
	public void Subscribe(ILocaleChangeListener p_listener)
	{
		_onLocaleChanged = (Action<Locale>)Delegate.Combine(_onLocaleChanged, new Action<Locale>(p_listener.OnLocaleChanged));
	}

	[Conditional("LOCALIZATION_TESTER")]
	public void Unsubscribe(ILocaleChangeListener p_listener)
	{
		_onLocaleChanged = (Action<Locale>)Delegate.Remove(_onLocaleChanged, new Action<Locale>(p_listener.OnLocaleChanged));
	}

	[Conditional("LOCALIZATION_TESTER")]
	public void ExecuteLocaleChanged(Locale p_locale)
	{
		_onLocaleChanged?.Invoke(p_locale);
	}

	public void Cleanup()
	{
		_onLocaleChanged = null;
	}
}
