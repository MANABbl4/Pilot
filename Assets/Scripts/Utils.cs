using UnityEngine;

public static class Utils
{
	public static Vector3 SetPositionByLatLon(this Vector2 self, Transform t, Vector3 center, float radius)
	{
		t.position = center;
		t.Translate(radius, 0.0f, 0.0f);
		
		t.RotateAround(center, new Vector3(0.0f, 0.0f, 1.0f), self.y);
		t.RotateAround(center, new Vector3(0.0f, 1.0f, 0.0f), self.x);

		return t.position;
	}
}