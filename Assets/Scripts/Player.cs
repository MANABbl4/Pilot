#define DEBUG_PLAYER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player
{
	[System.Diagnostics.Conditional("DEBUG_PLAYER")]
	private static void Log(string msg)
	{
		Debug.Log("Player. " + msg);
	}

	// Use this for initialization
	public void Init()
	{
		Object resource = GameObject.Instantiate(Resources.Load("Prefabs/cessna172"));
		if (resource != null)
		{
			m_airplane = resource as GameObject;
			m_airplane.SetActive(true);

			if (m_airplane != null)
			{
				m_airplane.name = resource.name;

				m_controller = m_airplane.GetComponent<AirPlainController>();

				m_airplane.transform.rotation = Quaternion.identity;
				m_airplane.transform.LookAt(Earth.Instance().GetCenter());
				m_airplane.transform.Rotate(new Vector3(90.0f, 270.0f, 0.0f));
				m_airplane.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			}
			else
			{
				Log("prefab = null");
			}
		}
		else
		{
			Log("resource = null");
		}

		if (m_controller == null)
		{
			Log("m_controller not loaded");
		}
	}
	
	// Update is called once per frame
	public void Tick()
	{
		
	}

	public GameObject GetAirPlane()
	{
		return m_airplane;
	}

	public AirPlainController GetAirPlaneController()
	{
		return m_controller;
	}

	private AirPlainController m_controller = null;
	private GameObject m_airplane = null;
}
