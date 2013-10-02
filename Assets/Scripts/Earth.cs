#define DEBUG_EARTH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Earth : SingletonGameObject<Earth>
{
	[System.Diagnostics.Conditional("DEBUG_EARTH")]
	private static void Log(string msg)
	{
		Debug.Log("Earth. " + msg);
	}

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

	public Vector2 GetAirportLatLon(int index)
	{
		return m_airportsLatLon[index];
	}

	protected override void Init()
	{
		base.Init();

		gameObject.transform.localScale = new Vector3(m_radius * 2, m_radius * 2, m_radius * 2);
		gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		gameObject.transform.rotation = Quaternion.identity;

		InitAirports();
		InitPlane();

		List<List<ushort>> data = LoadTerrain("Assets/Textures/Earth_bump/neg_z/0_0_0.raw", 128, 128);
		InitTerrain(InitPlane(), data);
	}

	protected override void DeInit()
	{
		base.DeInit();
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

	private void InitTerrain(GameObject plane, List<List<ushort>> terrainData)
	{
		int width = 128;
		int height = 128;

		MeshFilter mf = plane.GetComponent<MeshFilter>();
		if (mf != null)
		{
			Mesh m = mf.mesh;
			Vector3[] vertices = m.vertices;

			//calc index by pos!

			/*for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					vertices[i * width + j] += (Vector3.up) * terrainData[i][j] / ushort.MaxValue;
				}
			}*/

			m.vertices = vertices;
			mf.sharedMesh = m;
			mf.sharedMesh.RecalculateNormals();
			mf.sharedMesh.RecalculateBounds();
		}
	}

	private GameObject InitPlane()
	{
		Object resource = GameObject.Instantiate(Resources.Load("Prefabs/plane"));
		if (resource != null)
		{
			GameObject plane = resource as GameObject;
			plane.SetActive(true);

			if (plane != null)
			{
				plane.name = resource.name;

				plane.transform.rotation = Quaternion.identity;
				plane.transform.position = Vector3.zero;
				plane.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

				return plane;
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

		return null;
	}

	private List<List<ushort>> LoadTerrain(string fileName, int width, int height)
	{
		List<List<ushort>> data = new List<List<ushort>>();

		using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
		{
			for (int i = 0; i < height; ++i)
			{
				List<ushort> row = new List<ushort>();

				for (int j = 0; j < width; ++j)
				{
					row.Add(reader.ReadUInt16());
				}

				data.Add(row);
			}
		}

		return data;
	}

	private void Update()
	{
	}

	[SerializeField]
	private float m_radius = 6370000.0f;
	[SerializeField]
	private List<Vector2> m_airportsLatLon;
	[SerializeField]
	private float m_g = 10.0f;

	private List<GameObject> m_airports = new List<GameObject>();
}
