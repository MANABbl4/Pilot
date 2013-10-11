#define DEBUG_EARTH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Earth : SingletonGameObject<Earth>
{
	private class TerrainTextureInfo
	{
		public int m_x = 0;
		public int m_y = 0;
		public string m_side = string.Empty;
		public string m_path = string.Empty;
	}

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

		m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_cube.transform.localScale = Vector3.one;
		m_cube.transform.position = GetCenter();

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
		float min = 0;

		float h = 63.5f;
		Vector3 c = GetCenter();
		//float r = GetRadius();

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

				if ((i == 0 && (j == 0 || j == 127 || j == 15 || j == 31 || j == 47 || j == 63 || j == 79 || j == 95 || j == 111)))
				{
					Debug.Log("pos[" + i + "][" + j + "] = " + vertices[k] + " latlon(up) = " + vertices[k].GetLatLon(Earth.Instance().GetCenter(), Vector3.forward));
				}
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
				plane.transform.localScale = Vector3.one;

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

	public void LoadTerrainByLatLon(Vector2 latLon)
	{
		//1. Check need loading
		//1yes.1. Calc loading texture
		//1yes.2. Load 5 * 5 = 25 textures 514*514 pixels with center texture in player position
		//1no.1. Check need recalc plane
		//1no.1yes. recalc
		//1no.1no. do nothing


	}

	private TerrainTextureInfo GetTerrainTexture(Vector3 pos)
	{
		TerrainTextureInfo info = new TerrainTextureInfo();

		RaycastHit hit;
		Ray ray = new Ray(pos, GetCenter() - pos);
		if (m_cube.collider.Raycast(ray, out hit, (GetCenter() - pos).magnitude))
		{
			float x = 0.0f;
			float y = 0.0f;
			if (hit.point.x == 0.5f)
			{
				info.m_side = "pos_z/";
				x = hit.point.z;
				y = -hit.point.y;
			}
			if (hit.point.x == -0.5f)
			{
				info.m_side = "neg_z/";
				x = -hit.point.z;
				y = -hit.point.y;
			}

			if (hit.point.z == 0.5f)
			{
				info.m_side = "pos_x/";
				x = -hit.point.x;
				y = -hit.point.y;
			}
			if (hit.point.z == -0.5f)
			{
				info.m_side = "neg_x/";
				x = hit.point.x;
				y = -hit.point.y;
			}

			if (hit.point.y == 0.5f)
			{
				info.m_side = "pos_y/";
				x = -hit.point.x;
				y = hit.point.z;
			}
			if (hit.point.y == -0.5f)
			{
				info.m_side = "neg_y/";
				x = hit.point.z;
				y = -hit.point.x;
			}

			int count = (int)Mathf.Pow(2, m_detailsLevel);

			info.m_x = (int)(count * (x + 0.5f));
			info.m_y = (int)(count * (y + 0.5f));
			info.m_path = info.m_side + m_detailsLevel + "_" + info.m_x + "_" + info.m_y + ".raw";

			return info;
		}

		return null;
	}

	/*private string GetSideLeft(string curSide)
	{
		if (curSide == "pos_x/")
		{
			return "pos_z/";
		}
		else if (curSide == "pos_y/")
		{
			return "neg_x/";
		}
		else if (curSide == "pos_z/")
		{
			return "neg_x/";
		}
		else if (curSide == "neg_x/")
		{
			return "neg_z/";
		}
		else if (curSide == "neg_y/")
		{
			return "neg_x/";
		}
		else if (curSide == "neg_z/")
		{
			return "pos_x/";
		}

		return string.Empty;
	}

	private string GetSideRight(string curSide)
	{
		if (curSide == "pos_x/")
		{
			return "neg_z/";
		}
		else if (curSide == "pos_y/")
		{
			return "pos_x/";
		}
		else if (curSide == "pos_z/")
		{
			return "pos_x/";
		}
		else if (curSide == "neg_x/")
		{
			return "pos_z/";
		}
		else if (curSide == "neg_y/")
		{
			return "pos_x/";
		}
		else if (curSide == "neg_z/")
		{
			return "neg_x/";
		}

		return string.Empty;
	}

	private string GetSideUp(string curSide)
	{
		if (curSide == "pos_x/")
		{
			return "pos_y/";
		}
		else if (curSide == "pos_y/")
		{
			return "neg_z/";
		}
		else if (curSide == "pos_z/")
		{
			return "pos_y/";
		}
		else if (curSide == "neg_x/")
		{
			return "pos_y/";
		}
		else if (curSide == "neg_y/")
		{
			return "pos_z/";
		}
		else if (curSide == "neg_z/")
		{
			return "pos_y/";
		}

		return string.Empty;
	}

	private string GetSideDown(string curSide)
	{
		if (curSide == "pos_x/")
		{
			return "neg_y/";
		}
		else if (curSide == "pos_y/")
		{
			return "pos_z/";
		}
		else if (curSide == "pos_z/")
		{
			return "neg_y/";
		}
		else if (curSide == "neg_x/")
		{
			return "neg_y/";
		}
		else if (curSide == "neg_y/")
		{
			return "neg_z/";
		}
		else if (curSide == "neg_z/")
		{
			return "neg_y/";
		}

		return string.Empty;
	}*/

	private string[] GetAroundTextures(TerrainTextureInfo info)
	{
		int count = (int)Mathf.Pow(2, m_detailsLevel);
		List<string> textures = new List<string>();

		//объект фэйково переместить в нужные стороны и вычислить текстуры

		/*if (info.m_x == 0)
		{
			string sideX = GetSideLeft(info.m_side);
			if (sideX.Length == 0)
			{
				return null;
			}
			textures.Add(sideX + m_detailsLevel + "_" + (count - 1) + "_" + (info.m_y) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x + 1) + "_" + (info.m_y) + ".raw");

			if (info.m_y == 0)
			{
				string sideY = GetSideUp(info.m_side);
				if (sideY.Length == 0)
				{
					return null;
				}

				textures.Add(sideY + m_detailsLevel + "_" + (count - 1) + "_" + (info.m_y + 0) + ".raw");
			}
			else if (info.m_y == count - 1)
			{

			}
			else
			{

			}
		}
		else if (info.m_x == count - 1)
		{
			string side = GetSideRight(info.m_side);
			if (side.Length == 0)
			{
				return null;
			}
		}
		else
		{
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x - 1) + "_" + (info.m_y - 1) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x + 1) + "_" + (info.m_y + 1) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x - 1) + "_" + (info.m_y + 1) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x + 1) + "_" + (info.m_y - 1) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x + 1) + "_" + (info.m_y) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x - 1) + "_" + (info.m_y) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x) + "_" + (info.m_y + 1) + ".raw");
			textures.Add(info.m_side + m_detailsLevel + "_" + (info.m_x) + "_" + (info.m_y - 1) + ".raw");
		}*/
		
		

		return textures.ToArray();
	}

	private void Update()
	{
		if (m_cube != null)
		{
			TerrainTextureInfo info = GetTerrainTexture(MainManager.Instance().GetPlayer().GetAirPlane().transform.position);
			if (info != null && m_currentTerrainTexture != info.m_path)
			{
				m_currentTerrainTexture = info.m_path;
				GetAroundTextures(info);

				Debug.Log(m_currentTerrainTexture);
			}
		}
	}

	[SerializeField]
	private float m_radius = 6370000.0f;
	[SerializeField]
	private List<Vector2> m_airportsLatLon;
	[SerializeField]
	private float m_g = 10.0f;
	[SerializeField]
	private int m_detailsLevel = 1;

	private List<GameObject> m_airports = new List<GameObject>();
	private Dictionary<string, List<List<ushort>>> m_terrainData = new Dictionary<string, List<List<ushort>>>();
	private GameObject m_cube = null;
	private string m_currentTerrainTexture = string.Empty;
}
