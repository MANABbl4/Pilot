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

		List<List<ushort>> data = null;
		Quaternion rot = Quaternion.identity;

		data = LoadTerrain("Assets/Textures/Earth_bump/neg_y/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.left * 180.0f) * Quaternion.Euler(Vector3.up * 180.0f);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/pos_y/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.up * 0.0f);
		InitTerrain(InitPlane(), data, rot);

		data = LoadTerrain("Assets/Textures/Earth_bump/neg_z/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.left * 180.0f) * Quaternion.Euler(Vector3.forward * 90.0f);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/pos_z/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.back * 90.0f);
		InitTerrain(InitPlane(), data, rot);

		data = LoadTerrain("Assets/Textures/Earth_bump/neg_x/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.forward * -90.0f) * Quaternion.Euler(Vector3.left * 90.0f);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/pos_x/0_0_0.raw", 128, 128);
		rot = Quaternion.Euler(Vector3.forward * -90.0f) * Quaternion.Euler(Vector3.right * 90.0f);
		InitTerrain(InitPlane(), data, rot);
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

	private void InitTerrain(GameObject plane, List<List<ushort>> terrainData, Quaternion rot)
	{
		ushort min = 0;

		float h = 63.5f;
		Vector3 c = GetCenter();

		MeshFilter mf = plane.GetComponent<MeshFilter>();
		if (mf != null)
		{
			Mesh m = mf.mesh;
			Vector3[] vertices = m.vertices;

			for (int k = 0; k < vertices.Length; ++k)
			{
				int i = (int)((vertices[k].x + h));
				int j = (int)((vertices[k].z + h));

				vertices[k].y += h;
				Vector3 d = (vertices[k] - c).normalized;
				vertices[k] = d * (h + (terrainData[i][j] - min) / 10000);
			}

			m.vertices = vertices;
			mf.sharedMesh = m;
			mf.sharedMesh.RecalculateNormals();
			mf.sharedMesh.RecalculateBounds();
		}

		plane.transform.rotation *= rot;
		plane.transform.localScale = Vector3.one * m_radius / h;
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
				plane.transform.position = GetCenter();
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

		ushort max = ushort.MinValue;
		ushort min = ushort.MaxValue;

		using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
		{
			for (int i = 0; i < height; ++i)
			{
				List<ushort> row = new List<ushort>();

				for (int j = 0; j < width; ++j)
				{
					ushort d = reader.ReadUInt16();
					if (d > max)
					{
						max = d;
					}

					if (d < min)
					{
						min = d;
					}

					row.Add(d);
				}

				data.Add(row);
			}
		}

		Debug.Log("min " + min + " max " + max);

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
