using System;
using System.Linq;
using UnityEngine;

namespace BayatGames.SaveGameFree.Types;

[Serializable]
public class MeshSave
{
	public Vector3Save[] vertices;

	public int[] triangles;

	public Vector2Save[] uv;

	public Vector3Save[] normals;

	public Color[] colors;

	public Color32[] colors32;

	public MeshSave(Mesh mesh)
	{
		vertices = mesh.vertices.Cast<Vector3Save>().ToArray();
		triangles = mesh.triangles;
		uv = mesh.uv.Cast<Vector2Save>().ToArray();
		normals = mesh.normals.Cast<Vector3Save>().ToArray();
		colors = mesh.colors.Cast<Color>().ToArray();
		colors32 = mesh.colors32.Cast<Color32>().ToArray();
	}

	public static implicit operator MeshSave(Mesh mesh)
	{
		return new MeshSave(mesh);
	}

	public static implicit operator Mesh(MeshSave mesh)
	{
		return new Mesh
		{
			vertices = mesh.vertices.Cast<Vector3>().ToArray(),
			triangles = mesh.triangles,
			uv = mesh.uv.Cast<Vector2>().ToArray(),
			normals = mesh.normals.Cast<Vector3>().ToArray(),
			colors = mesh.colors.Cast<Color>().ToArray(),
			colors32 = mesh.colors32.Cast<Color32>().ToArray()
		};
	}
}
