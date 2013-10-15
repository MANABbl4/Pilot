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

		m_loadBtnRect.Init();

		Earth.Instance();
		Sun.Instance();

		m_player = new Player();
		m_player.Init();
		//m_player.GetAirPlaneController().SetLatLon(Earth.Instance().GetAirportLatLon(24));

		CameraManager.Instance();
	}

	protected override void DeInit()
	{
		base.DeInit();
	}

	private void OnGUI()
	{
		int offset = Screen.height / Earth.Instance().GetAirportsLatLon().Count;
		int offsetBtn = 0;
		foreach (Vector2 pos in Earth.Instance().GetAirportsLatLon())
		{
			if (GUI.Button(new Rect(0, offsetBtn, 50, offset), pos.ToString()) && MainManager.Instance().GetPlayer().GetAirPlane() != null)
			{
				m_player.GetAirPlaneController().SetLatLon(pos);
			}

			offsetBtn += offset;
		}

		if (!m_started)
		{
			if (GUI.Button(m_loadBtnRect.GetRect(), m_loadBtnText))
			{
				m_player.SetAirPlane("Prefabs/cessna172");
				m_player.GetAirPlaneController().SetLatLon(Earth.Instance().GetAirportLatLon(24));
				//Earth.Instance().LoadTerrainByLatLon(Earth.Instance().GetAirportLatLon(24));
				m_started = true;
			}
		}
	}

	private void Update()
	{
		m_player.Tick();
	}

	[SerializeField]
	private HUDRect m_loadBtnRect = new HUDRect();
	[SerializeField]
	private string m_loadBtnText;
	
	private Player m_player = null;
	private bool m_started = false;
}
