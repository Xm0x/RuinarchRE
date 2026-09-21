public class SaveDataPlayerDevastationComponent : SaveData<PlayerDevastationComponent>
{
	public bool hasMeteorDevastation;

	public int currentTickMeteorDevastation;

	public int durationInTicksMeteorDevastation;

	public override void Save(PlayerDevastationComponent data)
	{
		base.Save(data);
		hasMeteorDevastation = data.hasMeteorDevastation;
		currentTickMeteorDevastation = data.currentTickMeteorDevastation;
		durationInTicksMeteorDevastation = data.durationInTicksMeteorDevastation;
	}

	public override PlayerDevastationComponent Load()
	{
		return new PlayerDevastationComponent(this);
	}
}
