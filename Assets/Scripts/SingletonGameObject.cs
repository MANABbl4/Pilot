#define DEBUG_SINGLETON_GAME_OBJECT

using UnityEngine;
using System.Collections;

public class SingletonGameObject<T> : MonoBehaviour
	where T : SingletonGameObject<T>, new()
{
	[System.Diagnostics.Conditional("DEBUG_SINGLETON_GAME_OBJECT")]
	private static void Log(string msg)
	{
		Debug.Log("SingletonGameObject. " + msg);
	}

	public static T GetCommponent()
	{
		if (m_instance != null)
		{
			return m_instance;
		}

		Log("m_prefabPath = " + m_prefabPath);

		Object resource = Resources.Load(m_prefabPath);
		if (resource != null)
		{
			GameObject prefab = resource as GameObject;
			if (prefab != null)
			{
				prefab.name = resource.name;
				T component = prefab.GetComponent<T>();
				return component;
			}

			Log("prefab = null");
			return null;
		}

		Log("resource = null");
		return null;
	}

	public static T Instance()
	{
		if (m_instance == null)
		{
			Log("m_prefabPath = " + m_prefabPath);

			// Load resource
			Object resource = Resources.Load(m_prefabPath);
			if (resource == null)
			{
				Log("resource = null");
				return null;
			}
			// Instantiate
			if (resource as GameObject == null)
			{
				Log("GameObject = null");
				return null;
			}
			GameObject prefab = GameObject.Instantiate(resource, Vector3.zero, Quaternion.identity) as GameObject;
			// Get component
			T component = prefab.GetComponent<T>();
			if (component == null)
			{
				Log("component = null");
				return null;
			}
			// Move to parent
			if (m_parentName != null)
			{
				string name;
				if (m_dontDestoyOnLoad)
				{
					name = m_parentName + "_DontDestroyOnLoad";
				}
				else
				{
					name = m_parentName + "_DestroyOnLoad";
				}

				GameObject parent = GameObject.Find("/" + name);
				if (!parent) parent = new GameObject(name);
				if (m_dontDestoyOnLoad)
				{
					DontDestroyOnLoad(parent);
				}
				prefab.transform.parent = parent.transform;
			}
			// Apply don't destroy on load if need
			if (m_dontDestoyOnLoad)
			{
				DontDestroyOnLoad(prefab);
			}
			// Init singleton
			m_instance = component;
			m_instance.Init();
			m_inited = true;
		}
		return m_instance;
	}

	public static T TryInstance()
	{
		return m_instance;
	}

	public static void FreeInstance()
	{
		if (m_instance)
		{
			m_instance.DeInit();
			GameObject go = m_instance.gameObject;
			m_instance = null;
			Destroy(go);
			go = null;
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void FreeInstanceImmediate()
	{
		if (m_instance)
		{
			m_instance.DeInit();
			GameObject go = m_instance.gameObject;
			m_instance = null;
			DestroyImmediate(go);
			go = null;
		}
	}

	protected virtual void Init()
	{
	}

	protected virtual void DeInit()
	{
	}

	private void Start()
	{
		if (m_instance != this as T)
		{
			Destroy(gameObject);
		}
	}

	private void OnDestroy()
	{
		if (m_instance == this as T)
		{
			FreeInstance();
		}
	}

	private void OnApplicationQuit()
	{
		if (m_instance == this as T)
		{
			//FreeInstance();
		}
	}

	protected static T m_instance = null;
	protected static bool m_dontDestoyOnLoad = true;
	protected static string m_prefabPath = "Prefabs/" + typeof(T).Name;
	protected static string m_parentName = "Singleton";
	private static bool m_inited = false;
}
