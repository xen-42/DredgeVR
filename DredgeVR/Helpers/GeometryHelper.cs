﻿using UnityEngine;

namespace DredgeVR.Helpers;

public static class GeometryHelper
{
	/// <summary>
	/// Returns a copy of the mesh but with double sided faces
	/// Originally got it from Marshmallow, written by _nebula unless they also copied it from somewhere
	/// </summary>
	/// <param name="mesh"></param>
	/// <returns></returns>
	public static Mesh MakeMeshDoubleFaced(Mesh mesh)
	{
		var vertices = mesh.vertices;
		var uv = mesh.uv;
		var normals = mesh.normals;
		var szV = vertices.Length;
		var newVerts = new Vector3[szV * 2];
		var newUv = new Vector2[szV * 2];
		var newNorms = new Vector3[szV * 2];
		for (var j = 0; j < szV; j++)
		{
			// duplicate vertices and uvs:
			newVerts[j] = newVerts[j + szV] = vertices[j];
			newUv[j] = newUv[j + szV] = uv[j];
			// copy the original normals...
			newNorms[j] = normals[j];
			// and revert the new ones
			newNorms[j + szV] = -normals[j];
		}
		var triangles = mesh.triangles;
		var szT = triangles.Length;
		var newTris = new int[szT * 2]; // double the triangles
		for (var i = 0; i < szT; i += 3)
		{
			// copy the original triangle
			newTris[i] = triangles[i];
			newTris[i + 1] = triangles[i + 1];
			newTris[i + 2] = triangles[i + 2];
			// save the new reversed triangle
			var j = i + szT;
			newTris[j] = triangles[i] + szV;
			newTris[j + 2] = triangles[i + 1] + szV;
			newTris[j + 1] = triangles[i + 2] + szV;
		}

		var newMesh = new Mesh();
		newMesh.name = $"{mesh.name}_DoubleSided";
		newMesh.vertices = newVerts;
		newMesh.uv = newUv;
		newMesh.normals = newNorms;
		newMesh.triangles = newTris; // assign triangles last!
		return newMesh;
	}
}
