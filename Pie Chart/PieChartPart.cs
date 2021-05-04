/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Rendering;
using TMPro;

namespace CommonUtils.PieChart
{
	/// <summary>
	/// <para>
	/// Generates the meshes for the pie chart.
	/// It is to be instantiated by <see cref="PieChartController"/>.
	/// The mesh generated will start at the 12 o'clock direction, facing the forward z direction.
	/// The pivot point will be at the middle vertex of the mesh, at the bottom of the pie.
	/// It is to be then rotated by the controller into the right spot.
	/// </para>
	/// <para>
	/// The mesh generator works by generating two center pivot points, then generates the rest of the points around a predefined radius from the center.
	/// The two end faces will be filled, even if the mesh is a full cylinder.
	/// The mesh will only generate a vertex point every degree, until the last segment, in which it will generate based on the fraction of the degree.
	/// </para>
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MaterialInstance))]
	public class PieChartPart : MonoBehaviour
	{
		#region Type Definitions

		/// <summary>
		/// A collection of a vertex list and triangle indices list.
		/// </summary>
		private struct MeshVertTrisList
		{
			public List<Vector3> vertices;
			public List<int> triangles;

			public void AddRange(MeshVertTrisList meshVertTrisList)
			{
				vertices.AddRange(meshVertTrisList.vertices);
				triangles.AddRange(meshVertTrisList.triangles);
			}
		}

		#endregion

		#region Serialization

		[Header("Mesh Settings")]

		[Tooltip("Thickness of the pie.")]
		[SerializeField]
		public float Thickness = .1f;

		[Tooltip("Radius of the pie.")]
		[SerializeField]
		public float Radius = .3f;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Title of this pie part.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The value this part represents.
		/// </summary>
		public float Value { get; set; }

		/// <summary>
		/// The total value of the whole pie.
		/// </summary>
		public float Total { get; set; }

		/// <summary>
		/// The colour of the material.
		/// </summary>
		public Color Color
		{
			get => materialInstance.Material.color;
			set
			{
				materialInstance.Material.color = value;
			}
		}

		/// <summary>
		/// The float percent value of this slice.
		/// </summary>
		public float Percent
		{
			get
			{
				return Value / Total;
			}
		}

		/// <summary>
		/// The string percent value of this slice, to two decimal points, with % added.
		/// </summary>
		public string PercentString
		{
			get
			{
				return Percent.ToString("0.00") + "%";
			}
		}

		/// <summary>
		/// <see cref="MeshFilter"/> reference on this object.
		/// </summary>
		private MeshFilter meshFilter { get; set; }

		/// <summary>
		/// <see cref="MaterialInstance"/> reference on this object.
		/// </summary>
		private MaterialInstance materialInstance { get; set; }

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void Awake()
		{
			// Get component references
			meshFilter = gameObject.GetComponent<MeshFilter>();
			materialInstance = gameObject.GetComponent<MaterialInstance>();
		}

		#endregion

		#region Mesh Generator Functions

		/// <summary>
		/// Clears the current mesh and generates a new one.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="total"></param>
		public void UpdateMesh()
		{
			// Create/Clear mesh
			Mesh mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				meshFilter.mesh = new Mesh();
				mesh = meshFilter.sharedMesh;
			}
			else
				mesh.Clear();

			// Angle of the slice in degrees
			float angle = Value / Total * 360.0f;

			// Offset for the tri indices
			int offset = 0;

			// Mesh Generation

			MeshVertTrisList meshVertTrisList;
			meshVertTrisList.vertices = new List<Vector3>();
			meshVertTrisList.triangles = new List<int>();


			// Bottom pie mesh
			meshVertTrisList.AddRange(GenerateSlice(angle, true, offset));
			offset += Mathf.CeilToInt(angle) + 2;

			// Top pie mesh
			meshVertTrisList.AddRange(GenerateSlice(angle, false, offset));
			offset += Mathf.CeilToInt(angle) + 2;

			// Left face
			meshVertTrisList.AddRange(GenerateSideFace(angle, true, offset));
			offset += 4;

			// Right face
			meshVertTrisList.AddRange(GenerateSideFace(angle, false, offset));
			offset += 4;

			// Outside curve face
			meshVertTrisList.AddRange(GenerateOutsideCurve(angle, offset));

			// Set mesh
			mesh.vertices = meshVertTrisList.vertices.ToArray();
			mesh.triangles = meshVertTrisList.triangles.ToArray();

			// Update mesh
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.Optimize();

			gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
		}

		/// <summary>
		/// Generates a flat pie slice.
		/// </summary>
		/// <param name="angle">Angle of the pie slice in degrees.</param>
		/// <param name="isTopFace">Whether this is the top face of the slice.</param>
		/// <param name="offset">Vertices offset for triangle indices.</param>
		private MeshVertTrisList GenerateSlice(float angle, bool isTopFace, int offset = 0)
		{
			MeshVertTrisList mesh;
			mesh.vertices = new List<Vector3>();
			mesh.triangles = new List<int>();

			// Include the first vertex
			int edgeVertices = Mathf.CeilToInt(angle) + 1;

			float height = isTopFace ? Thickness : 0.0f;

			// Bottom pivot vertex
			mesh.vertices.Add(new Vector3(0.0f, height, 0.0f)); // 0 - bottom

			// Generate triangle fan from center until the last face
			for (int i = 1; i <= edgeVertices; i++)
			{
				// Add a vertex every degree step around the center, until the last one which will use the final angle
				if (i < edgeVertices)
					mesh.vertices.Add(
					new Vector3(
						Radius * Mathf.Sin(ToRad(i - 1)), height,
						Radius * Mathf.Cos(ToRad(i - 1))));
				else
					mesh.vertices.Add(
						new Vector3(
							Radius * Mathf.Sin(ToRad(angle)), height,
							Radius * Mathf.Cos(ToRad(angle))));

				// Create tri indices after the first tri is created
				if (i > 1)
				{
					// Add the indices in clockwise direction
					mesh.triangles.Add(offset); // Pivot vertex
					if (isTopFace)
					{
						mesh.triangles.Add(offset + i - 1); // Previous vertex
						mesh.triangles.Add(offset + i); // Current vertex
					}
					else
					{
						mesh.triangles.Add(offset + i); // Current vertex
						mesh.triangles.Add(offset + i - 1); // Previous vertex
					}
				}
			}

			return mesh;
		}

		/// <summary>
		/// Generates the side rectangle faces for each ends of the slices.
		/// </summary>
		/// <param name="angle">Angle of the pie slice in degrees.</param>
		/// <param name="isLeftFace">Whether this is the left face of the slice.</param>
		/// <param name="offset">Vertices offset for triangle indices.</param>
		/// <returns></returns>
		private MeshVertTrisList GenerateSideFace(float angle, bool isLeftFace, int offset = 0)
		{
			MeshVertTrisList mesh;
			mesh.vertices = new List<Vector3>();
			mesh.triangles = new List<int>();

			// Create top and bottom pivot vertices
			mesh.vertices.Add(new Vector3(0.0f, 0.0f, 0.0f)); // 0 - bottom
			mesh.vertices.Add(new Vector3(0.0f, Thickness, 0.0f)); // 1 - top


			// Left face starts at angle of 0
			angle = isLeftFace ? 0.0f : angle;

			// Add bottom outside vertex
			mesh.vertices.Add(
				new Vector3(
					Radius * Mathf.Sin(ToRad(angle)), 0.0f,
					Radius * Mathf.Cos(ToRad(angle)))); // 2 - bottom
			// Add top outside vertex
			mesh.vertices.Add(
				new Vector3(
					Radius * Mathf.Sin(ToRad(angle)), Thickness,
					Radius * Mathf.Cos(ToRad(angle)))); // 3 - top

			// Generate tri indices
			// Rectangle faces are made of 2 tris
			if (isLeftFace)
			{
				// First tri (bottom right, bottom left, top right)
				mesh.triangles.Add(offset);
				mesh.triangles.Add(offset + 2);
				mesh.triangles.Add(offset + 1);

				// Second tri (top right, bottom left, top left)
				mesh.triangles.Add(offset + 1);
				mesh.triangles.Add(offset + 2);
				mesh.triangles.Add(offset + 3);
			}
			else
			{
				// First tri (bottom left, top left, bottom right)
				mesh.triangles.Add(offset);
				mesh.triangles.Add(offset + 1);
				mesh.triangles.Add(offset + 2);

				// Second tri (top left, top right, bottom right)
				mesh.triangles.Add(offset + 1);
				mesh.triangles.Add(offset + 3);
				mesh.triangles.Add(offset + 2);
			}

			return mesh;
		}

		/// <summary>
		/// Generates the outside curved part of the slice.
		/// </summary>
		/// <param name="angle">Angle of the pie slice in degrees.</param>
		/// <param name="offset">Vertices offset for triangle indices.</param>
		private MeshVertTrisList GenerateOutsideCurve(float angle, int offset = 0)
		{
			MeshVertTrisList mesh;
			mesh.vertices = new List<Vector3>();
			mesh.triangles = new List<int>();

			// Include the first vertex
			int edgeVertices = Mathf.CeilToInt(angle) + 1;

			// Generate edge vertices, k represents current index count (since each iteration adds 2 vertices)
			for (int i = 0, k = 1; i < edgeVertices; i++, k += 2)
			{
				// Add top and bottom vertex every degree step around the center, until the last one which will use the final angle
				if (i < edgeVertices - 1)
				{
					// Bottom vertex
					mesh.vertices.Add(
					new Vector3(
						Radius * Mathf.Sin(ToRad(i)), 0.0f,
						Radius * Mathf.Cos(ToRad(i))));
					// Top vertex
					mesh.vertices.Add(
					new Vector3(
						Radius * Mathf.Sin(ToRad(i)), Thickness,
						Radius * Mathf.Cos(ToRad(i))));
				}
				else
				{
					// Bottom vertex
					mesh.vertices.Add(
					new Vector3(
						Radius * Mathf.Sin(ToRad(angle)), 0.0f,
						Radius * Mathf.Cos(ToRad(angle))));
					// Top vertex
					mesh.vertices.Add(
					new Vector3(
						Radius * Mathf.Sin(ToRad(angle)), Thickness,
						Radius * Mathf.Cos(ToRad(angle))));
				}

				// Create a rectangular face for each degree
				if (i > 0)
				{
					// First tri (bottom right, bottom left, top right)
					mesh.triangles.Add(offset + k - 3);
					mesh.triangles.Add(offset + k - 1);
					mesh.triangles.Add(offset + k - 2);

					// Second tri (bottom left, top left, top right)
					mesh.triangles.Add(offset + k - 1);
					mesh.triangles.Add(offset + k);
					mesh.triangles.Add(offset + k - 2);
				}
			}

			return mesh;
		}

		/// <summary>
		/// Converts an angle from degrees to radians.
		/// </summary>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		/// <returns>Angle in radians.</returns>
		public static float ToRad(float angleInDegrees)
		{
			return angleInDegrees * Mathf.PI / 180.0f;
		}

		#endregion

	}
}