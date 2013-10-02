#define DEBUG_EARTH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        mf.sharedMesh = MeshUtils.Plane(Vector3.zero, Vector3.right * 1, Vector3.up * 1, 103, 103);
        mf.sharedMesh.RecalculateNormals();
        mf.sharedMesh.RecalculateBounds();
        obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
	}

	private void Update()
	{
		/*foreach (GameObject obj in m_airports)
		{
			Vector3 normal = obj.transform.position - Earth.Instance().GetCenter();
			Vector3 forward = Vector3.up;
			Vector3 left = Vector3.right;

			Vector3.OrthoNormalize(ref normal, ref forward, ref left);
			Debug.DrawLine(obj.transform.position, obj.transform.position + Camera.mainCamera.transform.rotation * (Vector3.right * 20), Color.blue);
			//Debug.DrawLine(obj.transform.position, obj.transform.position + left.normalized * 20, Color.red);
		}*/
	}

	[SerializeField]
	private float m_radius = 6370000.0f;
	[SerializeField]
	private List<Vector2> m_airportsLatLon;
	[SerializeField]
	private float m_g = 10.0f;

	private List<GameObject> m_airports = new List<GameObject>();
}
