using UnityEngine;
using System.Collections;

public class Singleton<T> where T : Singleton<T>, new()
{
	private static T m_instance = null;

	public static T Instance()
	{
		if (m_instance == null)
		{
			m_instance = new T();
			m_instance.Init();
		}
		return m_instance;
	}

	public static void DestroyInstance()
	{
		m_instance.DeInit();
		m_instance = null;
	}

	protected virtual void Init()
	{
	}

	protected virtual void DeInit()
	{
	}
}
