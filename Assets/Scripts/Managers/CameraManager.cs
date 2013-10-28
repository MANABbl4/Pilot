using UnityEngine;
using System.Collections;

public class CameraManager : SingletonGameObject<CameraManager>
{
	public void Tick()
	{
		UpdateZoom();
		UpdateCameraPos();
	}

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

	private void UpdateCameraPos()
	{
		GameObject curAirPlane = MainManager.Instance().GetPlayer().GetAirPlane();
		if (curAirPlane != null)
		{
			Vector3 airplanePos = curAirPlane.transform.position;
			Camera.main.transform.position = Earth.Instance().GetCenter();
			Camera.main.transform.Translate(airplanePos + airplanePos.normalized * m_heightOffset * m_curZoom, Space.World);

			Camera.main.transform.rotation = curAirPlane.transform.rotation;
			Camera.main.transform.rotation *= Quaternion.AngleAxis(90.0f, Vector3.right);
			Camera.main.transform.rotation *= Quaternion.AngleAxis(180.0f, Vector3.forward);
		}
		else
		{
			Camera.main.transform.position = Earth.Instance().GetCenter();
			Camera.main.transform.Translate(Vector3.right * (2 * Earth.Instance().GetRadius()), Space.World);
			Camera.main.transform.LookAt(Earth.Instance().GetCenter());
		}
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
