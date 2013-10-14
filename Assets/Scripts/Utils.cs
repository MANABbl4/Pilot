using UnityEngine;

public static class Utils
{
	public static Vector3 SetPositionByLatLon(this Vector2 self, Transform t, Vector3 center, float radius)
	{
		t.position = center;
		t.rotation = Quaternion.identity;
		t.Translate(radius, 0.0f, 0.0f);
		
		t.RotateAround(center, new Vector3(0.0f, 0.0f, 1.0f), self.y);
		t.RotateAround(center, new Vector3(0.0f, 1.0f, 0.0f), self.x);

		return t.position;
	}

	public static Vector2 GetLatLon(this Vector3 self, Vector3 center, Vector3 refPoint)
	{
		Vector2 latLon = Vector2.zero;

		Vector3 dir = self - center;
		if (dir.x == 0.0f && dir.y == 0.0f)
		{
			latLon.x = 0.0f;
			latLon.y = 90.0f;

			return latLon;
		}

		//y
		Vector3 dirY = dir;
		dirY.y = center.y;
		latLon.y = Vector3.Angle(dir, dirY);
		if (dir.y < 0)
		{
			latLon.y = -latLon.y;
		}

		//x
		latLon.x = Vector3.Angle(refPoint, dirY);
		if (dir.x < 0)
		{
			latLon.x = -latLon.x;
		}

		return latLon;
	}
}