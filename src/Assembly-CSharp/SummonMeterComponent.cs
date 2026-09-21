using UtilityScripts;

public class SummonMeterComponent
{
	private bool m_isInitialized;

	public static int MAX_TARGET_POINT = 100000;

	public RuinarchBasicProgress progress { get; private set; }

	public SummonMeterComponent()
	{
		progress = new RuinarchBasicProgress("The Ruinarch's Awakening", BOOKMARK_TYPE.Progress_Bar);
		progress.Initialize(0, MAX_TARGET_POINT);
	}

	public SummonMeterComponent(SaveDataSummonMeterComponent data)
	{
		progress = data.progress;
		progress.Load();
	}

	public void Initialize()
	{
		m_isInitialized = true;
		ListenToSignals();
	}

	private void InitializeOnGameStarted()
	{
		Messenger.RemoveListener(Signals.GAME_STARTED, InitializeOnGameStarted);
	}

	public void LoadReferences(SaveDataSummonMeterComponent data)
	{
	}

	private void ListenToSignals()
	{
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyGain);
		Messenger.AddListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnChaoticEnergyGain);
		Messenger.AddListener(Signals.GAME_STARTED, InitializeOnGameStarted);
	}

	private void OnChaoticEnergyGain(int p_amountGained)
	{
		progress.IncreaseProgress(p_amountGained);
		Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, progress.currentValue, MAX_TARGET_POINT);
	}

	private void OnSpiritEnergyGain(int p_amountGained, int p_currentSpiritEnergy)
	{
		progress.IncreaseProgress(p_amountGained);
		Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, progress.currentValue, MAX_TARGET_POINT);
	}
}
