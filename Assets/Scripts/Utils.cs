using UnityEngine;

public static class Utils
{
	public enum RotateType
	{
		RotateUnknown = -1,
		Rotate0,
		Rotate90,
		Rotate180,
		Rotate270
	}

	public static void Rotate(this ushort[,] self, RotateType type)
	{
		if (type == RotateType.RotateUnknown || type == RotateType.Rotate0)
		{
			Debug.Log("Rotate. type " + type);
			return;
		}

		int height = self.GetLength(0);
		int width = self.GetLength(1);

		if (height == 0 || height != width)
		{
			Debug.Log("Rotate. height == 0 || height != width");
			return;
		}

		ushort[,] temp = new ushort[height, width];

		if (type == RotateType.Rotate180)
		{
			Debug.Log("Rotate. type " + type);
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					temp[i, j] = self[height - i - 1, width - j - 1];
					/*ushort temp = self[height - i - 1, width - j - 1];
					self[height - i - 1, width - j - 1] = self[i, j];
					self[i, j] = temp;*/
				}
			}
		}
		else if (type == RotateType.Rotate270)
		{
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					temp[i, j] = self[width - j - 1, i];
					/*ushort temp = self[j, i];
					self[j, i] = self[i, j];
					self[i, j] = temp;*/
				}
			}
		}
		else if (type == RotateType.Rotate90)
		{
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					temp[i, j] = self[j, height - 1 - i];
					/*ushort temp = self[width - j - 1, i];
					self[width - j - 1, i] = self[i, j];
					self[i, j] = temp;*/
				}
			}
		}
		else
		{
			Debug.LogError("Rotate. Error type!");
		}

		for (int i = 0; i < height; ++i)
		{
			for (int j = 0; j < width; ++j)
			{
				self[i, j] = temp[i, j];
			}
		}
	}

	/*public static RotateType CalcRotateType(ushort[,] main, ushort[,] other)
	{
		ushort[] mainUpSide = new ushort[main.GetLength(0)];
		ushort[] otherUpSide = new ushort[other.GetLength(0)];
		ushort[] otherDownSide = new ushort[other.GetLength(0)];
		ushort[] otherLeftSide = new ushort[other.GetLength(1)];
		ushort[] otherRightSide = new ushort[other.GetLength(1)];

		for (int i = 0; i < main.GetLength(0); ++i)
		{
			mainUpSide[i] = main[0, i];
		}

		for (int i = 0; i < main.GetLength(0); ++i)
		{
			otherUpSide[i] = main[0, main.GetLength(0) - i - 1];
		}
		
		//Check Rotate0

		return RotateType.RotateUnknown;
	}*/

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