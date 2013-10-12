using UnityEngine;
using System.Collections;

public class MainManager : SingletonGameObject<MainManager>
{
	public Player GetPlayer()
	{
		return m_player;
	}

	protected override void Init()
	{
		base.Init();

		Earth.Instance();
		Sun.Instance();

		m_player = new Player();
		m_player.Init();
		m_player.GetAirPlaneController().SetLatLon(Earth.Instance().GetAirportLatLon(24));
		//Earth.Instance().LoadTerrainByLatLon(Earth.Instance().GetAirportLatLon(24));

		CameraManager.Instance();
	}

	protected override void DeInit()
	{
		base.DeInit();
	}

	private void Update()
	{
		m_player.Tick();
	}
	
	private Player m_player = null;
}
