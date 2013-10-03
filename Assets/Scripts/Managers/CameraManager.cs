using UnityEngine;
using System.Collections;

public class CameraManager : SingletonGameObject<CameraManager>
{
	protected override void Init()
	{
		base.Init();

		m_curZoom = m_startZoom;

		UpdateCameraPos();
	}

	protected override void DeInit()
	{
		base.DeInit();
	}
	
	// Update is called once per frame
	private void Update()
	{
		UpdateZoom();
		UpdateCameraPos();
	}

	private void UpdateCameraPos()
	{
		Vector3 airplanePos = MainManager.Instance().GetPlayer().GetAirPlane().transform.position;
		Camera.main.transform.position = Earth.Instance().GetCenter();
		Camera.main.transform.Translate(airplanePos + airplanePos.normalized * m_heightOffset * m_curZoom, Space.World);

		Camera.main.transform.rotation = MainManager.Instance().GetPlayer().GetAirPlane().transform.rotation;
		Camera.main.transform.rotation *= Quaternion.AngleAxis(90.0f, Vector3.right);
		Camera.main.transform.rotation *= Quaternion.AngleAxis(180.0f, Vector3.forward);
	}

	private void UpdateZoom()
	{
		m_curZoom += m_zoomStepScale * Input.GetAxis("Mouse ScrollWheel");
		if (m_curZoom < m_minZoom)
		{
			m_curZoom = m_minZoom;
		}
	}

	[SerializeField]
	private float m_heightOffset = 20.0f;
	[SerializeField]
	private float m_minZoom = 1.0f;
	[SerializeField]
	private float m_startZoom = 5.0f;
	[SerializeField]
	private float m_zoomStepScale = 5.0f;

	private float m_curZoom = 1.0f;
}
