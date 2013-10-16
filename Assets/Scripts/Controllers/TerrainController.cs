#define DEBUG_TERRAIN_CONTROLLER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TerrainController
{
	private class TerrainTextureInfo
	{
		public int m_x = 0;
		public int m_y = 0;
		public string m_side = string.Empty;
		public string m_path = string.Empty;

		public class Comparer : IEqualityComparer<TerrainTextureInfo>
		{
			public bool Equals(TerrainTextureInfo x, TerrainTextureInfo y)
			{
				return (x.m_path.Equals(y.m_path));
			}
			public int GetHashCode(TerrainTextureInfo x)
			{
				return x.m_path.GetHashCode() ^ x.m_path.GetHashCode();
			}
		}
	}

	[System.Diagnostics.Conditional("DEBUG_TERRAIN_CONTROLLER")]
	private static void Log(string msg)
	{
		Debug.Log("TerrainController. " + msg);
	}

	public void Init(Vector3 center, float radius)
	{
		m_center = center;
		m_radius = radius;

		m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_cube.transform.localScale = Vector3.one;
		m_cube.transform.position = m_center;

		PreComputeTerrainTextures();

		List<List<ushort>> data = null;
		Quaternion rot = Quaternion.identity;

		data = LoadTerrain("Assets/Textures/Earth_bump/neg_y/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.down) * Quaternion.FromToRotation(Vector3.back, Vector3.left);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/pos_y/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.up) * Quaternion.FromToRotation(Vector3.back, Vector3.left);
		InitTerrain(InitPlane(), data, rot);

		data = LoadTerrain("Assets/Textures/Earth_bump/pos_z/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.forward) * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/neg_z/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.back) * Quaternion.FromToRotation(Vector3.right, Vector3.back);
		InitTerrain(InitPlane(), data, rot);

		data = LoadTerrain("Assets/Textures/Earth_bump/neg_x/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.left) * Quaternion.FromToRotation(Vector3.right, Vector3.left);
		InitTerrain(InitPlane(), data, rot);
		data = LoadTerrain("Assets/Textures/Earth_bump/pos_x/0_0_0.raw", 128, 128);
		rot = Quaternion.FromToRotation(Vector3.up, Vector3.right) * Quaternion.FromToRotation(Vector3.right, Vector3.right);
		InitTerrain(InitPlane(), data, rot);
	}

	public void DeInit()
	{
	}

	public void Tick()
	{
		if (m_cube != null && MainManager.Instance().GetPlayer().GetAirPlane() != null)
		{
			TerrainTextureInfo info = GetTerrainTexture(MainManager.Instance().GetPlayer().GetAirPlane().transform.position,
				m_center, m_detailsLevel);
			if (info != null && m_currentTerrainTexture != info.m_path)
			{
				m_currentTerrainTexture = info.m_path;
				GetAroundTextures(info, m_detailsLevel);

				Log(m_currentTerrainTexture);
			}
		}
	}

	private void InitTerrain(GameObject plane, List<List<ushort>> terrainData, Quaternion rot)
	{
		float min = 0;

		float h = 63.5f;
		Vector3 c = m_center;
		//float r = m_radius;

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

				/*if ((i == 0 && (j == 0 || j == 127 || j == 15 || j == 31 || j == 47 || j == 63 || j == 79 || j == 95 || j == 111)))
				{
					Log("pos[" + i + "][" + j + "] = " + vertices[k] + " latlon(up) = " + vertices[k].GetLatLon(m_center, Vector3.forward));
				}*/
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
				plane.transform.position = m_center;
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

		//Log("min " + min + " max " + max);

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

	private TerrainTextureInfo GetTerrainTexture(Vector3 pos, Vector3 center, int detailsLevel)
	{
		TerrainTextureInfo info = new TerrainTextureInfo();

		RaycastHit hit;
		Ray ray = new Ray(pos, center - pos);
		if (m_cube.collider.Raycast(ray, out hit, (center - pos).magnitude))
		{
			float x = 0.0f;
			float y = 0.0f;
			if (hit.point.z == m_cube.collider.bounds.extents.z)
			{
				info.m_side = "pos_z/";
				x = hit.point.z;
				y = -hit.point.y;
			}
			if (hit.point.z == -m_cube.collider.bounds.extents.z)
			{
				info.m_side = "neg_z/";
				x = -hit.point.z;
				y = -hit.point.y;
			}

			if (hit.point.x == m_cube.collider.bounds.extents.x)
			{
				info.m_side = "pos_x/";
				x = -hit.point.x;
				y = -hit.point.y;
			}
			if (hit.point.x == -m_cube.collider.bounds.extents.x)
			{
				info.m_side = "neg_x/";
				x = hit.point.x;
				y = -hit.point.y;
			}

			if (hit.point.y == m_cube.collider.bounds.extents.y)
			{
				info.m_side = "pos_y/";
				x = hit.point.x;
				y = -hit.point.z;
			}
			if (hit.point.y == -m_cube.collider.bounds.extents.y)
			{
				info.m_side = "neg_y/";
				x = hit.point.x;
				y = hit.point.z;
			}

			float count = (int)Mathf.Pow(2, detailsLevel);

			info.m_x = (int)(count * (x + 0.5f));
			info.m_y = (int)(count * (y + 0.5f));
			info.m_path = info.m_side + detailsLevel + "_" + info.m_x + "_" + info.m_y + ".raw";

			return info;
		}

		return null;
	}

	private List<TerrainTextureInfo> GetAroundTextures(TerrainTextureInfo info, int detailsLevel)
	{
		if (!m_terrainTextures.ContainsKey(detailsLevel))
		{
			return null;
		}

		if (!m_terrainTextures[detailsLevel].ContainsKey(info))
		{
			return null;
		}

		return m_terrainTextures[detailsLevel][info];
		/*int count = (int)Mathf.Pow(2, m_detailsLevel);
		List<string> textures = new List<string>();

		float edgeSize = 1.0f;
		float halfEdgeSize = edgeSize / 2.0f;
		float cellSize = edgeSize / (float)count;

		Vector3 point = m_center;
		if (info.m_side == "pos_x/")
		{
			point.x = halfEdgeSize;
			point.y = halfEdgeSize - cellSize * (0.5f + (float)info.m_y);
			point.z = -halfEdgeSize + cellSize * (0.5f + (float)info.m_x);
		}
		else if (info.m_side == "neg_x/")
		{
			point.x = -halfEdgeSize;
			point.y = halfEdgeSize - cellSize * (0.5f + (float)info.m_y);
			point.z = halfEdgeSize - cellSize * (0.5f + (float)info.m_x);
		}
		else if (info.m_side == "pos_z/")
		{
			point.z = halfEdgeSize;
			point.y = halfEdgeSize - cellSize * (0.5f + (float)info.m_y);
			point.x = halfEdgeSize - cellSize * (0.5f + (float)info.m_x);
		}
		else if (info.m_side == "neg_z/")
		{
			point.z = -halfEdgeSize;
			point.y = halfEdgeSize - cellSize * (0.5f + (float)info.m_y);
			point.x = -halfEdgeSize + cellSize * (0.5f + (float)info.m_x);
		}
		else if (info.m_side == "pos_y/")
		{
			point.y = halfEdgeSize;
			point.z = halfEdgeSize - cellSize * (0.5f + (float)info.m_y);
			point.x = -halfEdgeSize + cellSize * (0.5f + (float)info.m_x);
		}
		else if (info.m_side == "neg_y/")
		{
			point.y = -halfEdgeSize;
			point.z = -halfEdgeSize + cellSize * (0.5f + (float)info.m_y);
			point.x = -halfEdgeSize + cellSize * (0.5f + (float)info.m_x);
		}
		//объект фэйково переместить в нужные стороны и вычислить текстуры

		return textures.ToArray();*/
	}

	private Dictionary<TerrainTextureInfo, List<TerrainTextureInfo>> PreComputeTerrainTexturesLevel(int detailsLevel)
	{
		Dictionary<TerrainTextureInfo, List<TerrainTextureInfo>> levelTerrainTextures = new Dictionary<TerrainTextureInfo, List<TerrainTextureInfo>>(new TerrainTextureInfo.Comparer());

		//calc all points and call GetTerrainTexture
		Vector3 pos = Vector3.forward;
		pos.x = 0.49f;
		pos.y = 0.49f;
		TerrainTextureInfo info = GetTerrainTexture(pos, m_center, detailsLevel);

		Log(info == null ? "null path" : info.m_path);

		return levelTerrainTextures;
	}

	private void PreComputeTerrainTextures()
	{
		for (int i = m_minDetailsLevel; i < m_maxDetailsLevel; ++i)
		{
			m_terrainTextures.Add(i, PreComputeTerrainTexturesLevel(i));
		}
	}

	private int m_detailsLevel = 1;
	private int m_minDetailsLevel = 0;
	private int m_maxDetailsLevel = 7;
	private Dictionary<string, List<List<ushort>>> m_terrainData = new Dictionary<string, List<List<ushort>>>();
	private Dictionary<int, Dictionary<TerrainTextureInfo, List<TerrainTextureInfo>>> m_terrainTextures = new Dictionary<int, Dictionary<TerrainTextureInfo, List<TerrainTextureInfo>>>();
	private GameObject m_cube = null;
	private string m_currentTerrainTexture = string.Empty;
	private Vector3 m_center = Vector3.zero;
	private float m_radius = 0.0f;
}
