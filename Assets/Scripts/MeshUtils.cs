using UnityEngine;

public static class MeshUtils
{
	public static Mesh Triangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
	{
		var normal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0)).normalized;
		var mesh = new Mesh
		{
			vertices = new[] { vertex0, vertex1, vertex2 },
			normals = new[] { normal, normal, normal },
			uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) },
			triangles = new[] { 0, 1, 2 }
		};
		return mesh;
	}

	public static Mesh Quad(Vector3 origin, Vector3 width, Vector3 length)
	{
		var normal = Vector3.Cross(length, width).normalized;
		var mesh = new Mesh
		{
			vertices = new[] { origin, origin + length, origin + length + width, origin + width },
			normals = new[] { normal, normal, normal, normal },
			uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) },
			triangles = new[] { 0, 1, 2, 0, 2, 3 }
		};
		return mesh;
	}

	public static Mesh Plane(Vector3 origin, Vector3 width, Vector3 length, int widthCount, int lengthCount)
	{
		var combine = new CombineInstance[widthCount * lengthCount];

		var i = 0;
		for (var x = 0; x < widthCount; x++)
		{
			for (var y = 0; y < lengthCount; y++)
			{
				combine[i].mesh = Quad(origin + width * x + length * y, width, length);
				i++;
			}
		}

		var mesh = new Mesh();
		mesh.CombineMeshes(combine, true, false);
		return mesh;
	}

	public static Mesh Cube(Vector3 width, Vector3 length, Vector3 height)
	{
		var corner0 = -width / 2 - length / 2 - height / 2;
		var corner1 = width / 2 + length / 2 + height / 2;

		var combine = new CombineInstance[6];
		combine[0].mesh = Quad(corner0, length, width);
		combine[1].mesh = Quad(corner0, width, height);
		combine[2].mesh = Quad(corner0, height, length);
		combine[3].mesh = Quad(corner1, -width, -length);
		combine[4].mesh = Quad(corner1, -height, -width);
		combine[5].mesh = Quad(corner1, -length, -height);

		var mesh = new Mesh();
		mesh.CombineMeshes(combine, true, false);
		return mesh;
	}
}