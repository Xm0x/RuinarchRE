using System.Collections.Generic;
using UnityEngine;

public class FactionEmblemInitializer : MonoBehaviour
{
	[SerializeField]
	private List<Sprite> factionEmblemPool;

	[SerializeField]
	private Sprite wildMonsterFactionEmblem;

	[SerializeField]
	private Sprite vagrantFactionEmblem;

	[SerializeField]
	private Sprite disguisedFactionEmblem;

	[SerializeField]
	private Sprite undeadFactionEmblem;

	[SerializeField]
	private Sprite playerFactionEmblem;

	[SerializeField]
	private Sprite cultFactionEmblem;

	[SerializeField]
	private Sprite ratmenFactionEmblem;

	[SerializeField]
	private Sprite banditFactionEmblem;

	[SerializeField]
	private Sprite retaliatorFactionEmblem;

	private void Awake()
	{
		FactionEmblemRandomizer.Initialize(factionEmblemPool);
		FactionEmblemRandomizer.wildMonsterFactionEmblem = wildMonsterFactionEmblem;
		FactionEmblemRandomizer.vagrantFactionEmblem = vagrantFactionEmblem;
		FactionEmblemRandomizer.disguisedFactionEmblem = disguisedFactionEmblem;
		FactionEmblemRandomizer.undeadFactionEmblem = undeadFactionEmblem;
		FactionEmblemRandomizer.playerFactionEmblem = playerFactionEmblem;
		FactionEmblemRandomizer.cultFactionEmblem = cultFactionEmblem;
		FactionEmblemRandomizer.ratmenFactionEmblem = ratmenFactionEmblem;
		FactionEmblemRandomizer.banditFactionEmblem = banditFactionEmblem;
		FactionEmblemRandomizer.retaliatorFactionEmblem = retaliatorFactionEmblem;
	}
}
