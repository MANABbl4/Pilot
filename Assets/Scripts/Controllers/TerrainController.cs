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

		public override string ToString()
		{
			return "m_side = " + m_side + "; m_hitPos = " + m_hitPos + "; m_path = " + m_path;
		}
	}

	[System.Diagnostics.Conditional("DEBUG_TERRAIN_CONTROLLER")]
	private static void Log(string msg)
	{
		Debug.Log("TerrainController. " + msg);
	}

	public void Init(Vector3 center, float radius)
	{
		m_terrainData = new ushort[3 * m_terrainTextureSize, 3 * m_terrainTextureSize];
		m_center = center;
		m_radius = radius;

		m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_cube.transform.localScale = Vector3.one;
		m_cube.transform.position = m_center;

		m_curPlane = InitPlane(Color.yellow);
		m_curPlane.SetActive(false);
		MeshFilter mf = m_curPlane.GetComponent<MeshFilter>();
		if (mf != null)
		{
			Mesh m = mf.mesh;
			m_curPlaneInitialVertices = m.vertices;
		}

		m_neighborPlaneX = InitPlane(Color.yellow);
		m_neighborPlaneX.SetActive(false);
		m_neighborPlaneY = InitPlane(Color.yellow);
		m_neighborPlaneY.SetActive(false);

		/*m_curPlanes = new GameObject[m_debugPlanesCount, m_debugPlanesCount];
		for (int i = 0; i < m_debugPlanesCount; ++i)
		{
			for (int j = 0; j < m_debugPlanesCount; ++j)
			{
				m_curPlanes[i, j] = InitPlane(Color.gray);
				m_curPlanes[i, j].SetActive(true);
			}
		}*/

		PreComputeTerrainTextures();
		PreComputeRotations();

		InitEarth();
	}

	public void DeInit()
	{
	}

	public void Tick()
	{
		if (m_cube != null && MainManager.Instance().GetPlayer().GetAirPlane() != null)
		{
			if (m_curPlane.activeSelf)
			{
				LoadTerrainByPosition(MainManager.Instance().GetPlayer().GetAirPlane().transform.position);
			}
		}
	}

	public void LoadTerrainByPosition(Vector3 pos)
	{
		//SetVisibleEarth(false);
		m_curPlane.SetActive(true);
		m_neighborPlaneX.SetActive(true);
		m_neighborPlaneY.SetActive(true);

		Vector3 hitPos = Vector3.zero;
		TerrainTextureInfo info = GetTerrainTexture(pos, m_center, m_detailsLevel, out hitPos);
		if (info == null)
		{
			return;
		}

		// TODO: calc m_detailsLevel
		// HACK
		bool detailsChanged = false;
		if (Input.GetKeyUp(KeyCode.Plus) && m_detailsLevel < m_maxDetailsLevel)
		{
			++m_detailsLevel;
			detailsChanged = true;
			Log("KeyCode.Plus");
		}

		if (Input.GetKeyUp(KeyCode.Minus) && m_detailsLevel > m_minDetailsLevel)
		{
			--m_detailsLevel;
			detailsChanged = true;
			Log("KeyCode.Minus");
		}

		int offsetX = m_terrainTextureSize;
		int offsetY = m_terrainTextureSize;
		if ((m_currentTerrainTexture != info.m_path || detailsChanged) && !m_updating)
		{
			m_updating = true;
			MainManager.Instance().StartCoroutine(UpdateTerrainData(info));
		}

		offsetX = (int)(m_terrainTextureSize * (1.0f + info.m_hitPos.x)) - (m_planeSize / 2 - 1);
		offsetY = (int)(m_terrainTextureSize * (1.0f + info.m_hitPos.y)) - (m_planeSize / 2 - 1);

		if (offsetX < m_terrainTextureSize)
		{
			ResetPlane(m_neighborPlaneX);
			m_neighborPlaneX = InitTerrain(m_neighborPlaneX, m_terrainData, GetRotation(info.m_side), m_terrainTextureSize - m_planeSize + 1, offsetY, hitPos, m_detailsLevel);
			offsetX = m_terrainTextureSize;
		}
		else if (offsetX > m_terrainTextureSize + m_terrainTextureSize - m_planeSize + 1)
		{
			ResetPlane(m_neighborPlaneX);
			m_neighborPlaneX = InitTerrain(m_neighborPlaneX, m_terrainData, GetRotation(info.m_side), m_terrainTextureSize + m_terrainTextureSize, offsetY, hitPos, m_detailsLevel);
			offsetX = m_terrainTextureSize + m_terrainTextureSize - m_planeSize + 1;
		}
		ResetPlane(m_curPlane);
		m_curPlane = InitTerrain(m_curPlane, m_terrainData, GetRotation(info.m_side), offsetX, offsetY, hitPos, m_detailsLevel);

		/*float count = (int)Mathf.Pow(2, m_detailsLevel);
		float len = 128.0f / (514.0f * count);
		for (int i = 0; i < m_debugPlanesCount; ++i)
		{
			for (int j = 0; j < m_debugPlanesCount; ++j)
			{
				Vector2 hp = hitPos;
				int ofX = offsetX + (j - 2) * 127;
				int ofY = offsetY + (i - 2) * 127;
				hp.x += (j - 2) * len;
				hp.y += (i - 2) * len;
				ResetPlane(m_curPlanes[i, j]);
				ResetPlane(m_curPlanes[i, j]);
				m_curPlanes[i, j] = InitTerrain(m_curPlanes[i, j], m_terrainData, GetRotation(info.m_side), ofX, ofY, hp, m_detailsLevel);
			}
		}*/
	}

	private IEnumerator UpdateTerrainData(TerrainTextureInfo info)
	{
		// CenterCenter
		int offsetX = m_terrainTextureSize;
		int offsetY = m_terrainTextureSize;
		FillTerrainData(m_terrainData, m_terrainTextureSize, info,
				m_rotations[info.m_side][info.m_side], offsetX, offsetY);

		Dictionary<string, TerrainTextureInfo> around = GetAroundTextures(info, m_detailsLevel);

		string log = "CenterCenter " + info.m_side + "   rot " + m_rotations[info.m_side][info.m_side]
			+ "\r\nRightCenter " + around["RightCenter"].m_side + "   rot " + m_rotations[info.m_side][around["RightCenter"].m_side]
			+ "\r\nLeftCenter " + around["LeftCenter"].m_side + "   rot " + m_rotations[info.m_side][around["LeftCenter"].m_side]
			+ "\r\nCenterUp " + around["CenterUp"].m_side + "   rot " + m_rotations[info.m_side][around["CenterUp"].m_side]
			+ "\r\nCenterDown " + around["CenterDown"].m_side + "   rot " + m_rotations[info.m_side][around["CenterDown"].m_side];
		// CenterUp
		offsetX = m_terrainTextureSize;
		offsetY = 0;
		FillTerrainData(m_terrainData, m_terrainTextureSize, around["CenterUp"],
			m_rotations[info.m_side][around["CenterUp"].m_side], offsetX, offsetY);

		// CenterDown
		offsetX = m_terrainTextureSize;
		offsetY = m_terrainTextureSize + m_terrainTextureSize;
		FillTerrainData(m_terrainData, m_terrainTextureSize, around["CenterDown"],
			m_rotations[info.m_side][around["CenterDown"].m_side], offsetX, offsetY);

		// LeftCenter
		offsetX = 0;
		offsetY = m_terrainTextureSize;
		FillTerrainData(m_terrainData, m_terrainTextureSize, around["LeftCenter"],
			m_rotations[info.m_side][around["LeftCenter"].m_side], offsetX, offsetY);

		// RightCenter
		offsetX = m_terrainTextureSize + m_terrainTextureSize;
		offsetY = m_terrainTextureSize;
		FillTerrainData(m_terrainData, m_terrainTextureSize, around["RightCenter"],
			m_rotations[info.m_side][around["RightCenter"].m_side], offsetX, offsetY);

		// LeftUp
		offsetX = 0;
		offsetY = 0;
		if (around.ContainsKey("LeftUp"))
		{
			log += "\r\nLeftUp " + around["LeftUp"].m_side + "   rot " + m_rotations[info.m_side][around["LeftUp"].m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, around["LeftUp"],
				m_rotations[info.m_side][around["LeftUp"].m_side], offsetX, offsetY);
		}
		else
		{
			TerrainTextureInfo inf = GetAroundTextures(around["LeftCenter"], m_detailsLevel)["CenterUp"];
			log += "\r\nLeftUp " + inf.m_side + "   rot " + m_rotations[around["LeftCenter"].m_side][inf.m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, inf,
				m_rotations[around["LeftCenter"].m_side][inf.m_side], offsetX, offsetY);
		}

		// RightUp
		offsetX = m_terrainTextureSize + m_terrainTextureSize;
		offsetY = 0;
		if (around.ContainsKey("RightUp"))
		{
			log += "\r\nRightUp " + around["RightUp"].m_side + "   rot " + m_rotations[info.m_side][around["RightUp"].m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, around["RightUp"],
				m_rotations[info.m_side][around["RightUp"].m_side], offsetX, offsetY);
		}
		else
		{
			TerrainTextureInfo inf = GetAroundTextures(around["RightCenter"], m_detailsLevel)["CenterUp"];
			log += "\r\nRightUp " + inf.m_side + inf.m_side + "   rot " + m_rotations[around["RightCenter"].m_side][inf.m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, inf,
				m_rotations[around["RightCenter"].m_side][inf.m_side], offsetX, offsetY);
		}

		// LeftDown
		offsetX = 0;
		offsetY = m_terrainTextureSize + m_terrainTextureSize;
		if (around.ContainsKey("LeftDown"))
		{
			log += "\r\nLeftDown " + around["LeftDown"].m_side + "   rot " + m_rotations[info.m_side][around["LeftDown"].m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, around["LeftDown"],
				m_rotations[info.m_side][around["LeftDown"].m_side], offsetX, offsetY);
		}
		else
		{
			TerrainTextureInfo inf = GetAroundTextures(around["LeftCenter"], m_detailsLevel)["CenterDown"];
			log += "\r\nLeftDown " + inf.m_side + "   rot " + m_rotations[around["LeftCenter"].m_side][inf.m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, inf,
				m_rotations[around["LeftCenter"].m_side][inf.m_side], offsetX, offsetY);
		}

		// RightDown
		offsetX = m_terrainTextureSize + m_terrainTextureSize;
		offsetY = m_terrainTextureSize + m_terrainTextureSize;
		if (around.ContainsKey("RightDown"))
		{
			log += "\r\nRightDown " + around["RightDown"].m_side + "   rot " + m_rotations[info.m_side][around["RightDown"].m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, around["RightDown"],
				m_rotations[info.m_side][around["RightDown"].m_side], offsetX, offsetY);
		}
		else
		{
			TerrainTextureInfo inf = GetAroundTextures(around["RightCenter"], m_detailsLevel)["CenterDown"];
			log += "\r\nRightDown " + inf.m_side + "   rot " + m_rotations[around["RightCenter"].m_side][inf.m_side];
			FillTerrainData(m_terrainData, m_terrainTextureSize, inf,
				m_rotations[around["RightCenter"].m_side][inf.m_side], offsetX, offsetY);
		}

		Debug.Log("done updating " + log);

		m_currentTerrainTexture = info.m_path;

		m_updating = false;
		yield break;
	}

	private void FillTerrainData(ushort[,] terrainData, int terrainTextureSize, TerrainTextureInfo info, Utils.RotateType rotation, int offsetX, int offsetY)
	{
		if (info != null)
		{
			Log("FillTerrainData. info " + info.m_side + " rotation " + rotation + " offset " + offsetX + " " + offsetY);
			ushort[,] data = LoadTerrainData(info.m_path, terrainTextureSize, terrainTextureSize);
			data.Rotate(rotation);
			for (int i = 0; i < terrainTextureSize; ++i)
			{
				for (int j = 0; j < terrainTextureSize; ++j)
				{
					terrainData[i + offsetY, j + offsetX] = data[i, j];
				}
			}
		}
		else
		{
			Log("FillTerrainData. info is null");
			for (int i = 0; i < terrainTextureSize; ++i)
			{
				for (int j = 0; j < terrainTextureSize; ++j)
				{
					terrainData[i + offsetY, j + offsetX] = 0;
				}
			}
		}
	}

	private void ResetPlane(GameObject plane)
	{
		MeshFilter mf = plane.GetComponent<MeshFilter>();
		if (mf != null)
		{
			Mesh m = mf.mesh;
			m.vertices = m_curPlaneInitialVertices;
			mf.sharedMesh = m;
			mf.sharedMesh.RecalculateNormals();
			mf.sharedMesh.RecalculateBounds();
		}

		plane.transform.rotation = Quaternion.identity;
		plane.transform.localScale = Vector3.one;
	}

	private void InitEarth()
	{
		ushort[,] data = null;

		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_y/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("neg_y/"), 0, 0, Vector2.zero, 0));
		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_y/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("pos_y/"), 0, 0, Vector2.zero, 0));

		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_z/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("pos_z/"), 0, 0, Vector2.zero, 0));
		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_z/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("neg_z/"), 0, 0, Vector2.zero, 0));

		data = LoadTerrainData("Assets/Textures/Earth_bump/neg_x/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("neg_x/"), 0, 0, Vector2.zero, 0));
		data = LoadTerrainData("Assets/Textures/Earth_bump/pos_x/0.raw", m_planeSize, m_planeSize);
		m_earth.Add(InitTerrain(InitPlane(Color.white), data, GetRotation("pos_x/"), 0, 0, Vector2.zero, 0));
	}

	private void SetVisibleEarth(bool val)
	{
		foreach (GameObject plane in m_earth)
		{
			plane.SetActive(val);
		}
	}

	private GameObject InitTerrain(GameObject plane, ushort[,] terrainData, Quaternion rot, int fromX, int fromY, Vector2 center, int detailsLevel)
	{
		float min = 0;
		float count = Mathf.Pow(2, detailsLevel);
		float scale = 1.0f;
		if (terrainData.GetLength(0) > 128)
		{
			scale = 128.0f / 514.0f;
		}
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

				vertices[k].x = vertices[k].x * scale / count + center.y * h * 2;
				vertices[k].z = vertices[k].z * scale / count + center.x * h * 2;
				//offset by center

				vertices[k].y += h;
				Vector3 d = (vertices[k] - c).normalized;
				vertices[k] = d * (h + (float)(terrainData[i + fromY, j + fromX] - min) / 10000.0f);
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

	private GameObject InitPlane(Color color)
	{
		Object resource = GameObject.Instantiate(Resources.Load("Prefabs/plane"));
		if (resource != null)
		{
			GameObject plane = resource as GameObject;
			plane.SetActive(true);

			if (plane != null)
			{
				plane.name = resource.name;

				MeshRenderer gameObjectRenderer = plane.GetComponent<MeshRenderer>();
				gameObjectRenderer.material.color = color;

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

	private ushort[,] LoadTerrainData(string fileName, int width, int height)
	{
		ushort[,] data = new ushort[height, width];

		ushort max = ushort.MinValue;
		ushort min = ushort.MaxValue;

		using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
		{
			for (int i = 0; i < height; ++i)
			{
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

					data[i, j] = d;
				}
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

	private TerrainTextureInfo GetTerrainTexture(Vector3 pos, Vector3 center, int detailsLevel, out Vector3 hitPos)
	{
		TerrainTextureInfo info = new TerrainTextureInfo();
		hitPos = Vector2.zero;
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
			info.m_hitPos.x = count * (hitPos.x + 0.5f - (((float)info.m_x) / count));
			info.m_hitPos.y = count * (hitPos.y + 0.5f - (((float)info.m_y) / count));
			info.m_path = "Assets/Textures/Earth_bump/" + info.m_side + detailsLevel + "_" + info.m_x + "_" + info.m_y + ".raw";

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
						neighbors.Add("RightCenter", new Vector3(pos.x, pos.y, pos.z + Mathf.Sign(pos.x) * offset));
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
						neighbors.Add("RightCenter", new Vector3(pos.x + Mathf.Sign(pos.y) * offset, pos.y, pos.z));
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

						neighbors.Add("LeftUp", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y + offset, pos.z));
						neighbors.Add("LeftCenter", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y, pos.z));
						neighbors.Add("LeftDown", new Vector3(pos.x + Mathf.Sign(pos.z) * offset, pos.y - offset, pos.z));
						neighbors.Add("CenterUp", new Vector3(pos.x, pos.y + offset, pos.z));
						neighbors.Add("CenterDown", new Vector3(pos.x, pos.y - offset, pos.z));
						neighbors.Add("RightUp", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y + offset, pos.z));
						neighbors.Add("RightCenter", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y, pos.z));
						neighbors.Add("RightDown", new Vector3(pos.x - Mathf.Sign(pos.z) * offset, pos.y - offset, pos.z));
					}

					Vector3 hitPos = Vector3.zero;
					TerrainTextureInfo info = GetTerrainTexture(pos, m_center, detailsLevel, out hitPos);

					Dictionary<string, TerrainTextureInfo> terrainNeighbors = new Dictionary<string, TerrainTextureInfo>();
					foreach (KeyValuePair<string, Vector3> neighbor in neighbors)
					{
						TerrainTextureInfo terrainNeighbor = GetTerrainTexture(neighbor.Value, m_center, detailsLevel, out hitPos);
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

	private void PreComputeRotations()
	{
		Dictionary<string, Utils.RotateType> rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_y/", Utils.RotateType.Rotate90);
		rotations.Add("pos_z/", Utils.RotateType.Rotate0);
		rotations.Add("neg_z/", Utils.RotateType.Rotate0);
		rotations.Add("neg_y/", Utils.RotateType.Rotate270);
		rotations.Add("neg_x/", Utils.RotateType.Rotate0);
		m_rotations.Add("neg_x/", rotations);

		rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_y/", Utils.RotateType.Rotate270);
		rotations.Add("pos_z/", Utils.RotateType.Rotate0);
		rotations.Add("neg_z/", Utils.RotateType.Rotate0);
		rotations.Add("neg_y/", Utils.RotateType.Rotate90);
		rotations.Add("pos_x/", Utils.RotateType.Rotate0);
		m_rotations.Add("pos_x/", rotations);

		rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_y/", Utils.RotateType.Rotate0);
		rotations.Add("pos_x/", Utils.RotateType.Rotate0);
		rotations.Add("neg_x/", Utils.RotateType.Rotate0);
		rotations.Add("neg_y/", Utils.RotateType.Rotate0);
		rotations.Add("neg_z/", Utils.RotateType.Rotate0);
		m_rotations.Add("neg_z/", rotations);

		rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_y/", Utils.RotateType.Rotate180);
		rotations.Add("pos_x/", Utils.RotateType.Rotate0);
		rotations.Add("neg_x/", Utils.RotateType.Rotate0);
		rotations.Add("neg_y/", Utils.RotateType.Rotate180);
		rotations.Add("pos_z/", Utils.RotateType.Rotate0);
		m_rotations.Add("pos_z/", rotations);

		rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_z/", Utils.RotateType.Rotate180);
		rotations.Add("pos_x/", Utils.RotateType.Rotate90);
		rotations.Add("neg_x/", Utils.RotateType.Rotate270);
		rotations.Add("neg_z/", Utils.RotateType.Rotate0);
		rotations.Add("neg_y/", Utils.RotateType.Rotate0);
		m_rotations.Add("neg_y/", rotations);

		rotations = new Dictionary<string, Utils.RotateType>();
		rotations.Add("pos_z/", Utils.RotateType.Rotate180);
		rotations.Add("pos_x/", Utils.RotateType.Rotate90);
		rotations.Add("neg_x/", Utils.RotateType.Rotate270);
		rotations.Add("neg_z/", Utils.RotateType.Rotate0);
		rotations.Add("pos_y/", Utils.RotateType.Rotate0);
		m_rotations.Add("pos_y/", rotations);
	}

	private int m_detailsLevel = 0;
	private int m_minDetailsLevel = 0;
	private int m_maxDetailsLevel = 3;
	private ushort[,] m_terrainData = null;
	private Dictionary<int, Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>>> m_terrainTextures = new Dictionary<int, Dictionary<TerrainTextureInfo, Dictionary<string, TerrainTextureInfo>>>();
	private List<GameObject> m_earth = new List<GameObject>();
	private Dictionary<string, Dictionary<string, Utils.RotateType>> m_rotations = new Dictionary<string, Dictionary<string, Utils.RotateType>>();
	private GameObject m_curPlane = null;
	private GameObject m_neighborPlaneX = null;
	private GameObject m_neighborPlaneY = null;
	//private GameObject[,] m_curPlanes = null;
	private GameObject m_cube = null;
	private string m_currentTerrainTexture = string.Empty;
	private Vector3 m_center = Vector3.zero;
	private float m_radius = 0.0f;
	private int m_terrainTextureSize = 514;
	private int m_planeSize = 128;
	private Vector3[] m_curPlaneInitialVertices = null;
	private bool m_updating = false;
	//private int m_debugPlanesCount = 5;
}
