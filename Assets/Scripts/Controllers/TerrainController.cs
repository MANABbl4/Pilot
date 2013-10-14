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
	}

	[System.Diagnostics.Conditional("DEBUG_TERRAIN_CONTROLLER")]
	private static void Log(string msg)
	{
		Debug.Log("TerrainController. " + msg);
	}

	public void Init()
	{
		m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_cube.transform.localScale = Vector3.one;
		m_cube.transform.position = Earth.Instance().GetCenter();

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
			TerrainTextureInfo info = GetTerrainTexture(MainManager.Instance().GetPlayer().GetAirPlane().transform.position);
			if (info != null && m_currentTerrainTexture != info.m_path)
			{
				m_currentTerrainTexture = info.m_path;
				GetAroundTextures(info);

				Debug.Log(m_currentTerrainTexture);
			}
		}
	}

	private void InitTerrain(GameObject plane, List<List<ushort>> terrainData, Quaternion rot)
	{
		float min = 0;

		float h = 63.5f;
		Vector3 c = Earth.Instance().GetCenter();
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
		plane.transform.localScale = Vector3.one * Earth.Instance().GetRadius() / h;
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
				plane.transform.position = Earth.Instance().GetCenter();
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
		Ray ray = new Ray(pos, Earth.Instance().GetCenter() - pos);
		if (m_cube.collider.Raycast(ray, out hit, (Earth.Instance().GetCenter() - pos).magnitude))
		{
			float x = 0.0f;
			float y = 0.0f;
			if (hit.point.z == 0.5f)
			{
				info.m_side = "pos_z/";
				x = hit.point.z;
				y = -hit.point.y;
			}
			if (hit.point.z == -0.5f)
			{
				info.m_side = "neg_z/";
				x = -hit.point.z;
				y = -hit.point.y;
			}

			if (hit.point.x == 0.5f)
			{
				info.m_side = "pos_x/";
				x = -hit.point.x;
				y = -hit.point.y;
			}
			if (hit.point.x == -0.5f)
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

	private int m_detailsLevel = 1;
	private Dictionary<string, List<List<ushort>>> m_terrainData = new Dictionary<string, List<List<ushort>>>();
	private GameObject m_cube = null;
	private string m_currentTerrainTexture = string.Empty;
}
