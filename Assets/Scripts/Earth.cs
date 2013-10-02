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
		InitQuads();
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

	private void InitQuads()
	{
		int width = 514;
		int height = 514;

		List<List<ushort>> data = LoadTerrain("Assets/Textures/Earth_bump/neg_z/0_0_0.raw", width, height);

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

				MeshFilter mf = plane.GetComponent<MeshFilter>();
				if (mf != null)
				{
					Mesh m = mf.mesh;
					Vector3[] vertices = m.vertices;

					for (int i = 0; i < height; ++i)
					{
						for (int j = 0; j < width; ++j)
						{
							vertices[i * width + j] += (Vector3.up) * data[i][j] / ushort.MaxValue;
						}
					}

					m.vertices = vertices;
					mf.sharedMesh = m;
					mf.sharedMesh.RecalculateNormals();
					mf.sharedMesh.RecalculateBounds();
				}
				else
				{
					Log("mf = null");
					MeshFilter[] mfs = plane.GetComponentsInChildren<MeshFilter>();
					int vert = 0;
					foreach (MeshFilter mf1 in mfs)
					{
						Mesh m = mf1.mesh;
						Vector3[] vertices = m.vertices;


						for (int i = 0; i < vertices.Length; ++i)
						{
							int j = 0;
							int k = 0;

							j = Mathf.FloorToInt((i + vert) / data.Count);
							k = i + vert - j * data.Count;

							vertices[i] += 10 * (Vector3.up) * data[j][k] / ushort.MaxValue;
						}

						vert += vertices.Length;

						m.vertices = vertices;
						mf1.sharedMesh = m;
						mf1.sharedMesh.RecalculateNormals();
						mf1.sharedMesh.RecalculateBounds();
					}
				}
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

        /*GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        Mesh m = MeshUtils.Plane(Vector3.zero, Vector3.right * 1, Vector3.up * 1, 103, 103);
		Vector3[] vertices = m.vertices;

		for(int i = 0; i < 103; ++i)
		{
			for(int j = 0; j < 103; ++j)
			{
				vertices[i * 103 + j] += (-Vector3.forward) * data[i][j] / 6553;
			}
		}

		m.vertices = vertices;
		mf.sharedMesh = m;
        mf.sharedMesh.RecalculateNormals();
        mf.sharedMesh.RecalculateBounds();
        obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;*/
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
