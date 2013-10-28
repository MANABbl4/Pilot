using UnityEngine;
using System.Collections;

public class AirPlainController : MonoBehaviour
{
	public void SetLatLon(Vector2 latLon)
	{
		m_curLatLon = latLon;
		gameObject.transform.position = m_curLatLon.SetPositionByLatLon(gameObject.transform, Earth.Instance().GetCenter(), Earth.Instance().GetRadius() + m_height);

		/*gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.rotation *= Quaternion.Euler(m_startRot);*/
		SetLookAt(Earth.Instance().GetCenter());
	}

	// Use this for initialization
	public void Start ()
	{
		m_startRot = transform.rotation.eulerAngles;
		m_screenCenter.x = Screen.width / 2;
		m_screenCenter.y = Screen.height / 2;

		/*gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.rotation *= Quaternion.Euler(m_startRot);*/
		SetLookAt(Earth.Instance().GetCenter());

		m_started = true;
	}
	
	// Update is called once per frame
	public void Tick()
	{
		if (!m_started)
			return;

#if UNITY_EDITOR
		if (Input.GetMouseButtonUp(0))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			Vector2 deltaPos = ((Vector2)mousePos - m_screenCenter).normalized;

			float angle = -Mathf.Sign(deltaPos.x) * Vector2.Angle(Vector2.up, deltaPos);
			gameObject.transform.rotation *= Quaternion.AngleAxis(-angle, Vector3.up);
		}

		Vector3 forward = (-gameObject.transform.forward);

		//Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + forward.normalized * 60, Color.green);
		
		float alpha = m_speed * Time.deltaTime * 360.0f / (2 * Mathf.PI * (Earth.Instance().GetRadius() + m_height));
		float horda = 2 * (Earth.Instance().GetRadius() + m_height) * Mathf.Sin(alpha * Mathf.Deg2Rad / 2);

		//forward = ((Mathf.Tan(alpha * Mathf.Deg2Rad / 2) * normal.normalized) + forward.normalized);
		//Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + forward * 60, Color.yellow);

		gameObject.transform.position += horda * forward.normalized;

		Vector3 normal = (gameObject.transform.position - Earth.Instance().GetCenter()).normalized;
		float angle2 = Vector3.Angle(gameObject.transform.up, normal);

		gameObject.transform.rotation *= Quaternion.Euler(-angle2, 0.0f, 0.0f);

		/*if (Input.GetMouseButton(0))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			if (mousePos.x < Screen.width * 0.2f)
				mousePos.x = Screen.width * 0.2f;
			if (mousePos.x > Screen.width * 0.8f)
				mousePos.x = Screen.width * 0.8f;
			if (mousePos.y < Screen.height * 0.2f)
				mousePos.y = Screen.height * 0.2f;
			if (mousePos.y > Screen.height * 0.8f)
				mousePos.y = Screen.height * 0.8f;

			
			Vector2 deltaPos = (Vector2)mousePos - m_screenCenter;
			Vector3 deltaRot = new Vector3(-deltaPos.y / Screen.height * 120, -deltaPos.x / Screen.width * 240, 0.0f);
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot + deltaRot - cameraRot);

			//if (m_propeller != null)
			//{
			//	m_propeller.transform.rotation = Quaternion.identity;
			//	m_propeller.transform.Rotate(m_startRot + deltaRot);
			//}
		}
		else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			mousePos.x = Screen.width * 0.2f;
			mousePos.y = m_screenCenter.y;
			Vector2 deltaPos = (Vector2)mousePos - m_screenCenter;
			Vector3 deltaRot = new Vector3(-deltaPos.y / Screen.height * 120, -deltaPos.x / Screen.width * 240, 0.0f);
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot + deltaRot - cameraRot);
		}
		else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			mousePos.x = Screen.width * 0.8f;
			mousePos.y = m_screenCenter.y;
			Vector2 deltaPos = (Vector2)mousePos - m_screenCenter;
			Vector3 deltaRot = new Vector3(-deltaPos.y / Screen.height * 120, -deltaPos.x / Screen.width * 240, 0.0f);
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot + deltaRot - cameraRot);
		}
		else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			mousePos.x = m_screenCenter.x;
			mousePos.y = Screen.height * 0.8f;
			Vector2 deltaPos = (Vector2)mousePos - m_screenCenter;
			Vector3 deltaRot = new Vector3(-deltaPos.y / Screen.height * 120, -deltaPos.x / Screen.width * 240, 0.0f);
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot + deltaRot - cameraRot);
		}
		else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			Vector2 mousePos = (Vector2)Input.mousePosition;
			mousePos.x = m_screenCenter.x;
			mousePos.y = Screen.height * 0.2f;
			Vector2 deltaPos = (Vector2)mousePos - m_screenCenter;
			Vector3 deltaRot = new Vector3(-deltaPos.y / Screen.height * 120, -deltaPos.x / Screen.width * 240, 0.0f);
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot + deltaRot - cameraRot);
		}
		else if (transform.rotation.eulerAngles != m_startRot)
		{
			transform.rotation = Quaternion.identity;
			transform.Rotate(m_startRot - cameraRot);
		}*/
#endif
	}

	private void SetLookAt(Vector3 pos)
	{
		Vector3 normal = (gameObject.transform.position - pos).normalized;
		gameObject.transform.rotation *= Quaternion.Euler(m_startRot);
		gameObject.transform.rotation *= Quaternion.FromToRotation(gameObject.transform.up, -normal);
	}

	private Vector2 m_curLatLon;
	private Vector2 m_curRot;
	private Vector3 m_startRot;
	private Vector2 m_screenCenter;
	private float m_speed = 150.0f;
	private float m_height = 100.0f;
	private bool m_started = false;
}
