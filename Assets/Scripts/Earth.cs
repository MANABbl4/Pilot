#define DEBUG_EARTH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Earth : SingletonGameObject<Earth>
{
	public float GetG()
	{
		return m_g;
	}

	public float GetRadius()
	{
		return m_radius;
	}

	public Vector3 GetCenter()
	{
		return gameObject.transform.position;
	}

	public List<GameObject> GetAirports()
	{
		return m_airports;
	}

	public List<Vector2> GetAirportsLatLon()
	{
		return m_airportsLatLon;
	}

	public Vector2 GetAirportLatLon(int index)
	{
		return m_airportsLatLon[index];
	}

	public TerrainController GetTerrainController()
	{
		return m_terrainController;
	}

	public void Tick()
	{
		if (m_terrainController != null)
		{
			m_terrainController.Tick();
		}
	}

	protected override void Init()
	{
		base.Init();

		gameObject.transform.localScale = new Vector3(m_radius * 2, m_radius * 2, m_radius * 2);
		gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		gameObject.transform.rotation = Quaternion.identity;

		InitAirports();

		m_terrainController.Init(GetCenter(), GetRadius());
	}

	protected override void DeInit()
	{
		base.DeInit();

		m_terrainController.DeInit();
	}

	private void CreateAirport(Vector2 pos)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		obj.transform.position = pos.SetPositionByLatLon(obj.transform, GetCenter(), m_radius);

		m_airports.Add(obj);
	}

	private void InitAirports()
	{
		foreach (Vector2 pos in m_airportsLatLon)
		{
			CreateAirport(pos);
		}
	}

	[SerializeField]
	private float m_radius = 6370000.0f;
	[SerializeField]
	private List<Vector2> m_airportsLatLon;
	[SerializeField]
	private float m_g = 10.0f;

	private List<GameObject> m_airports = new List<GameObject>();
	private TerrainController m_terrainController = new TerrainController();
}
