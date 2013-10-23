﻿#define DEBUG_TERRAIN_CONTROLLER

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
		public Vector2 m_hitPos = Vector2.zero;

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

		m_curPlane = InitPlane();
		m_curPlane.SetActive(false);

		PreComputeTerrainTextures();

		InitEarth();
	}

	public void DeInit()
	{
	}

	public void Tick()
	{
		if (m_cube != null && MainManager.Instance().GetPlayer().GetAirPlane() != null)
		{
			/*TerrainTextureInfo info = GetTerrainTexture(MainManager.Instance().GetPlayer().GetAirPlane().transform.position,
				m_center, m_detailsLevel);
			if (info != null && m_currentTerrainTexture != info.m_path)
			{
				m_currentTerrainTexture = info.m_path;
				Dictionary<string, TerrainTextureInfo> neghbors = GetAroundTextures(info, m_detailsLevel);

				string log = m_currentTerrainTexture + "\r\nNeighbors:";

				if (neghbors != null)
				{
					foreach (KeyValuePair<string, TerrainTextureInfo> inf in neghbors)
					{
						log += "\r\n" + inf.Key + " " + inf.Value.m_path;
					}
				}
				else
				{
					log += "\r\nNull neighbors";
				}

				Log(log);
			}*/

			if (m_curPlane.activeSelf)
			{
				LoadTerrainByPosition(MainManager.Instance().GetPlayer().GetAirPlane().transform.position);
			}
		}
	}

	public void LoadTerrainByPosition(Vector3 pos)
	{
		//1. Check need loading
		//1yes.1. Calc loading texture
		//1yes.2. Load 3 * 3 = 9 textures m_terrainTextureSize*m_terrainTextureSize pixels with center texture in player position
		//1no.1. Check need recalc plane
		//1no.1yes. recalc
		//1no.1no. do nothing

		m_curPlane.SetActive(true);
		TerrainTextureInfo info = GetTerrainTexture(pos, m_center, m_detailsLevel);
		if (m_currentTerrainTexture != info.m_path)
		{
			Dictionary<string, TerrainTextureInfo> around = GetAroundTextures(info, m_detailsLevel);

			m_terrainData.Clear();
			m_terrainData.Add(info.m_path, LoadTerrainData(info.m_path, m_terrainTextureSize, m_terrainTextureSize));
			foreach (KeyValuePair<string, TerrainTextureInfo> ar in around)
			{
				m_terrainData.Add(ar.Value.m_path, LoadTerrainData(ar.Value.m_path, m_terrainTextureSize, m_terrainTextureSize));
			}
		}

		//calc current height map
		int hitX = (int)(info.m_hitPos.x * (float)m_terrainTextureSize);
		int hitY = (int)(info.m_hitPos.y * (float)m_terrainTextureSize);

		if (hitX < m_planeSize / 2)
		{
			if (hitY < m_planeSize / 2)
			{
				//LeftUp
			}
			else
			{
				//Left
			}
		}
		else if (hitX + m_planeSize / 2 >= m_terrainTextureSize)
		{
			if (hitY + m_planeSize / 2 >= m_terrainTextureSize)
			{
				//RightUp
			}
			else
			{
				//Right
			}
		}
	}

	private void InitEarth()
	{
		List<List<ushort>> data = null;

		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_y/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("neg_y/")));
		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_y/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("pos_y/")));

		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_z/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("pos_z/")));
		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_z/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("neg_z/")));

		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_x/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("neg_x/")));
		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_x/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(), data, GetRotation("pos_x/")));
	}

	private GameObject InitTerrain(GameObject plane, List<List<ushort>> terrainData, Quaternion rot)
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

		return plane;
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

	private List<List<ushort>> LoadTerrainData(string fileName, int width, int height)
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

	private Quaternion GetRotation(string side)
	{
		Quaternion rot = Quaternion.identity;

		if (side == "neg_y/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.down) * Quaternion.FromToRotation(Vector3.back, Vector3.left);
		}
		else if (side == "pos_y/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.up) * Quaternion.FromToRotation(Vector3.back, Vector3.left);
		}
		else if (side == "pos_z/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.forward) * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
		}
		else if (side == "neg_z/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.back) * Quaternion.FromToRotation(Vector3.right, Vector3.back);
		}
		else if (side == "pos_x/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.right) * Quaternion.FromToRotation(Vector3.right, Vector3.right);
		}
		else if (side == "neg_x/")
		{
			rot = Quaternion.FromToRotation(Vector3.up, Vector3.left) * Quaternion.FromToRotation(Vector3.right, Vector3.left);
		}

		return rot;
	}

	private TerrainTextureInfo GetTerrainTexture(Vector3 pos, Vector3 center, int detailsLevel)
	{
		TerrainTextureInfo info = new TerrainTextureInfo();

		RaycastHit hit;
		Ray ray = new Ray(pos, center - pos);
		if (m_cube.collider.Raycast(ray, out hit, (center - pos).magnitude))
		{
			if ((Mathf.Abs(hit.point.x) == Mathf.Abs(hit.point.y) && Mathf.Abs(hit.point.x) == m_cube.collider.bounds.extents.x) ||
				(Mathf.Abs(hit.point.x) == Mathf.Abs(hit.point.z) && Mathf.Abs(hit.point.z) == m_cube.collider.bounds.extents.z) ||
				(Mathf.Abs(hit.point.y) == Mathf.Abs(hit.point.z) && Mathf.Abs(hit.point.y) == m_cube.collider.bounds.extents.y))
			{
				return null;
			}

			Vector2 hitPos = Vector2.zero;
			if (hit.point.z == m_cube.collider.bounds.extents.z)
			{
				info.m_side = "pos_z/";
				hitPos.x = -hit.point.x;
				hitPos.y = -hit.point.y;
			}
			else if (hit.point.z == -m_cube.collider.bounds.extents.z)
			{
				info.m_side = "neg_z/";
				hitPos.x = hit.point.x;
				hitPos.y = -hit.point.y;
			}
			else if (hit.point.x == m_cube.collider.bounds.extents.x)
			{
				info.m_side = "pos_x/";
				hitPos.x = hit.point.z;
				hitPos.y = -hit.point.y;
			}
			else if (hit.point.x == -m_cube.collider.bounds.extents.x)
			{
				info.m_side = "neg_x/";
				hitPos.x = -hit.point.z;
				hitPos.y = -hit.point.y;
			}
			else if (hit.point.y == m_cube.collider.bounds.extents.y)
			{
				info.m_side = "pos_y/";
				hitPos.x = hit.point.x;
				hitPos.y = -hit.point.z;
			}
			else if (hit.point.y == -m_cube.collider.bounds.extents.y)
			{
				info.m_side = "neg_y/";
				hitPos.x = hit.point.x;
				hitPos.y = hit.point.z;
			}
			else
			{
				return null;
			}

			float count = (int)Mathf.Pow(2, detailsLevel);

			info.m_x = (int)(count * (hitPos.x + 0.5f));
			info.m_y = (int)(count * (hitPos.y + 0.5f));
			info.m_hitPos.x = hitPos.x + 0.5f - (((float)info.m_x) / count);
			info.m_hitPos.y = hitPos.y + 0.5f - (((float)info.m_y) / count);
			info.m_path = info.m_side + detailsLevel + "_" + info.m_x + "_" + info.m_y + ".raw";

			return info;
		}

		return null;
	}

	private Dictionary<string, TerrainTextureInfo> GetAroundTextures(TerrainTextureInfo info, int detailsLevel)
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
	}

	private Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>> PreComputeTerrainTexturesLevel(int detailsLevel)
	{
		Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>> levelTerrainTextures = new Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>>(new TerrainTextureInfo.Comparer());

		Vector3[] sides = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
		int count = (int)Mathf.Pow(2, detailsLevel);
		float offset = 1.0f / (float)count;

		foreach (Vector3 side in sides)
		{
			for (int i = 0; i < count; ++i)
			{
				for (int j = 0; j < count; ++j)
				{
					Vector3 pos = side * 0.5001f;
					Dictionary<string, Vector3> neighbors = new Dictionary<string, Vector3>();

					if (pos.x != 0.0f)
					{
						pos.z = (((float)i + 0.5f) * offset) - 0.5f;
						pos.y = (((float)j + 0.5f) * offset) - 0.5f;

						neighbors.Add("RightUp", new Vector3(pos.x, pos.y + offset, pos.z + Mathf.Sign(pos.x) * offset));
						neighbors.Add("Right", new Vector3(pos.x, pos.y, pos.z + Mathf.Sign(pos.x) * offset));
						neighbors.Add("RightDown", new Vector3(pos.x, pos.y - offset, pos.z + Mathf.Sign(pos.x) * offset));
						neighbors.Add("CenterUp", new Vector3(pos.x, pos.y + offset, pos.z));
						neighbors.Add("CenterDown", new Vector3(pos.x, pos.y - offset, pos.z));
						neighbors.Add("LeftUp", new Vector3(pos.x, pos.y + offset, pos.z - Mathf.Sign(pos.x) * offset));
						neighbors.Add("LeftCenter", new Vector3(pos.x, pos.y, pos.z - Mathf.Sign(pos.x) * offset));
						neighbors.Add("LeftDown", new Vector3(pos.x, pos.y - offset, pos.z - Mathf.Sign(pos.x) * offset));
					}
					else if (pos.y != 0.0f)
					{
						pos.x = (((float)i + 0.5f) * offset) - 0.5f;
						pos.z = (((float)j + 0.5f) * offset) - 0.5f;

						neighbors.Add("RightUp", new Vector3(pos.x + Mathf.Sign(pos.y) * offset, pos.y, pos.z + offset));
						neighbors.Add("Right", new Vector3(pos.x + Mathf.Sign(pos.y) * offset, pos.y, pos.z));
						neighbors.Add("RightDown", new Vector3(pos.x + Mathf.Sign(pos.y) * offset, pos.y, pos.z - offset));
						neighbors.Add("CenterUp", new Vector3(pos.x, pos.y, pos.z + offset));
						neighbors.Add("CenterDown", new Vector3(pos.x, pos.y, pos.z - offset));
						neighbors.Add("LeftUp", new Vector3(pos.x - Mathf.Sign(pos.y) * offset, pos.y, pos.z + offset));
						neighbors.Add("LeftCenter", new Vector3(pos.x - Mathf.Sign(pos.y) * offset, pos.y, pos.z));
						neighbors.Add("LeftDown", new Vector3(pos.x - Mathf.Sign(pos.y) * offset, pos.y, pos.z - offset));
					}
					else
					{
						pos.x = (((float)i + 0.5f) * offset) - 0.5f;
						pos.y = (((float)j + 0.5f) * offset) - 0.5f;

						neighbors.Add("RightUp", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y + offset, pos.z));
						neighbors.Add("Right", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y, pos.z));
						neighbors.Add("RightDown", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y - offset, pos.z));
						neighbors.Add("CenterUp", new Vector3(pos.x, pos.y + offset, pos.z));
						neighbors.Add("CenterDown", new Vector3(pos.x, pos.y - offset, pos.z));
						neighbors.Add("LeftUp", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y + offset, pos.z));
						neighbors.Add("LeftCenter", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y, pos.z));
						neighbors.Add("LeftDown", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y - offset, pos.z));
					}

					TerrainTextureInfo info = GetTerrainTexture(pos, m_center, detailsLevel);

					Dictionary<string, TerrainTextureInfo> terrainNeighbors = new Dictionary<string, TerrainTextureInfo>();
					foreach (KeyValuePair<string, Vector3> neighbor in neighbors)
					{
						TerrainTextureInfo terrainNeighbor = GetTerrainTexture(neighbor.Value, m_center, detailsLevel);
						if (terrainNeighbor != null)
						{
							bool contains = false;
							foreach (KeyValuePair<string, TerrainTextureInfo> tn in terrainNeighbors)
							{
								if (tn.Value.m_path == terrainNeighbor.m_path)
								{
									contains = true;
									break;
								}
							}
							if (!contains)
							{
								terrainNeighbors.Add(neighbor.Key, terrainNeighbor);
							}
						}
					}

					if (info != null)
					{
						levelTerrainTextures.Add(info, terrainNeighbors);
					}
				}
			}
		}

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
	private int m_maxDetailsLevel = 6;
	private Dictionary<string, List<List<ushort>>> m_terrainData = new Dictionary<string, List<List<ushort>>>();
	private Dictionary<int, Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>>> m_terrainTextures = new Dictionary<int, Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>>>();
	private List<GameObject> m_earth = new List<GameObject>();
	private GameObject m_curPlane = null;
	private GameObject m_cube = null;
	private string m_currentTerrainTexture = string.Empty;
	private Vector3 m_center = Vector3.zero;
	private float m_radius = 0.0f;
	private int m_terrainTextureSize = 514;
	private int m_planeSize = 128;
}
