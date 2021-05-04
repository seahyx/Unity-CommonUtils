/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

namespace CommonUtils.PieChart
{
	/// <summary>
	/// A segment of the pie.
	/// </summary>
	[System.Serializable]
	public class PieSegment
	{
		public string Name;
		public float Value;
		public Color Color = Color.white;
		public float Thickness = 0.04f;
		public float Radius = 0.2f;
	}

	/// <summary>
	/// Draws a 3D pie chart with labels.
	/// This class will spawn <see cref="PieChartPart"/>s which will create the meshes for the different slices of the pie.
	/// </summary>
	public class PieChartController : MonoBehaviour
	{
		#region Serializable

		[Header("Pie Chart Settings")]

		[Tooltip("Pie Chart Part prefab.")]
		[Required]
		[SerializeField]
		private PieChartPart partPrefab;

		[Tooltip("Pie Chart labels.")]
		[Required]
		[SerializeField]
		private TextMeshPro labelPrefab;

		[Tooltip("Pie Chart segments.")]
		[SerializeField]
		private List<PieSegment> segments;

		[Tooltip("Whether the colours for the segments will be randomly generated, ignoring pre-defined values.")]
		[SerializeField]
		private bool isColourRandom = true;

		[Tooltip("Seed for generating random segment colours.")]
		[SerializeField]
		private int colourSeed = 0;

		[Header("Label Settings")]

		[Tooltip("Pie Chart label offset from the midpoint surface of the pie chart part.")]
		[SerializeField]
		private Vector3 labelOffset;

		[Tooltip("Pie Chart distance from the center of the pie to the circumference of the pie segment.")]
		[Range(0.0f, 1.0f)]
		[SerializeField]
		private float labelRadiusRatio = 0.5f;

		[Header("Animation Settings")]

		[Tooltip("Whether there is animation.")]
		[SerializeField]
		private bool isAnimated = true;

		[Tooltip("Animation duration in seconds.")]
		[SerializeField]
		private float animDuration = 0.4f;

		[Tooltip("Animation curve.")]
		[SerializeField]
		private AnimationCurve animCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		[Tooltip("If it is controlled by code, the controller will not update from preset values in the inspector.")]
		[SerializeField]
		private bool isControlledByCode = false;

		#endregion

		#region Member Definitions

		/// <summary>
		/// List of generated <see cref="PieChartPart"/> object references.
		/// </summary>
		private List<PieChartPart> pieParts { get; set; } = new List<PieChartPart>();

		/// <summary>
		/// List of generated label references on this object.
		/// </summary>
		private List<TextMeshPro> labels { get; set; } = new List<TextMeshPro>();

		/// <summary>
		/// Total value of all pie segments.
		/// </summary>
		private float totalValue
		{
			get
			{
				float value = 0;
				foreach (PieSegment segment in segments)
				{
					value += segment.Value;
				}
				return value;
			}
		}

		/// <summary>
		/// Reference to animation enumerator.
		/// </summary>
		private IEnumerator animEnumerator;

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Reset and start animating the pie chart.
		/// </summary>
		private void OnEnable()
		{
			ResetPie();
			SpawnPieParts(segments);
		}

		/// <summary>
		/// Reset pie chart and stops all animation.
		/// </summary>
		private void OnDisable()
		{
			ResetPie();
		}

		#endregion

		#region Pie Chart Functions

		/// <summary>
		/// Updates the pie chart data.
		/// </summary>
		public void UpdateData(List<PieSegment> segmentsData)
		{
			// Reset
			ResetPie();

			// Update segment data
			segments = segmentsData;

			// Create pie chart parts
			SpawnPieParts(segments);
		}

		/// <summary>
		/// Create pie chart parts.
		/// </summary>
		/// <param name="segments">Pie chart segments.</param>
		private void SpawnPieParts(List<PieSegment> segments)
		{
			Random.InitState(colourSeed);

			foreach (PieSegment segment in segments)
			{
				// Generate random colours for each segment if random colours is enabled
				if (isColourRandom)
					segment.Color = Color.HSVToRGB(Random.value, 0.6f, .8f);

				// Spawn pie chart parts
				PieChartPart part = Instantiate(partPrefab, transform);

				part.gameObject.name = segment.Name;
				part.Name = segment.Name;
				part.Value = segment.Value;
				part.Total = totalValue;
				part.Color = segment.Color;
				part.Thickness = segment.Thickness;
				part.Radius = segment.Radius;

				// Hide empty segments
				if (segment.Value == 0)
					part.gameObject.GetComponent<Renderer>().enabled = false;

				Debug.Assert(part.Thickness != 0.0f, $"[{name}] Segment thickness for {part.Name} is 0!");
				Debug.Assert(part.Radius != 0.0f, $"[{name}] Segment radius for {part.Name} is 0!");

				// Add to instantiated object list
				pieParts.Add(part);


				// Spawn label objects
				TextMeshPro label = Instantiate(labelPrefab, transform);
				label.text = segment.Value.ToString("P0");

				// Add to instantiated object list
				labels.Add(label);
			}

			// Animate pie chart if enabled
			if (isAnimated)
			{
				animEnumerator = AnimatePieChart();
				StartCoroutine(animEnumerator);
			}
			else
				UpdatePie();
		}

		/// <summary>
		/// Resets the pie chart. And stops any ongoing animations.
		/// </summary>
		private void ResetPie()
		{
			// Stop any ongoing animations
			StopAllCoroutines();

			foreach (PieChartPart part in pieParts)
				Destroy(part.gameObject);

			foreach (TextMeshPro label in labels)
				Destroy(label.gameObject);

			pieParts.Clear();
			labels.Clear();
		}

		/// <summary>
		/// Animation coroutine.
		/// </summary>
		/// <returns></returns>
		private IEnumerator AnimatePieChart()
		{
			// Current time elapsed for animation
			float timeElapsed = 0.0f;

			while (timeElapsed < animDuration)
			{
				float ratio = animCurve.Evaluate(timeElapsed / animDuration);

				UpdatePie(ratio);

				timeElapsed += Time.deltaTime;

				yield return null;
			}

			UpdatePie();
		}

		/// <summary>
		/// Updates pie chart values and rotations.
		/// </summary>
		/// <param name="t">The ratio to the maximum angle in which the chart will be expanded to. [0..1]</param>
		private void UpdatePie(float t = 1.0f)
		{
			float angle = 0.0f, prevAngle = 0.0f;

			// Update each segment
			for (int i = 0; i < pieParts.Count; i++)
			{
				// Interpolate pie chart radius
				pieParts[i].Value = t * segments[i].Value;
				pieParts[i].UpdateMesh();
				pieParts[i].transform.localRotation = Quaternion.Euler(0.0f, angle, 0.0f);

				// Next part of the pie will follow at the end of the previous pie part
				angle += pieParts[i].Value / totalValue * 360.0f;


				// Hide label if its value is 0
				if (segments[i].Value == 0)
				{
					labels[i].gameObject.SetActive(false);
				}
				else
				{
					// Show object if it has non-zero value
					labels[i].gameObject.SetActive(true);

					// Position labels
					float midPointAngle = (angle + prevAngle) / 2;
					labels[i].transform.localPosition = new Vector3(
						Mathf.Sin(PieChartPart.ToRad(midPointAngle)) * pieParts[i].Radius * labelRadiusRatio,
						pieParts[i].Thickness,
						Mathf.Cos(PieChartPart.ToRad(midPointAngle)) * pieParts[i].Radius * labelRadiusRatio)
						+ labelOffset;

					// Update label text
					labels[i].text = pieParts[i].Value.ToString("P0");
				}

				// Update prevAngle
				prevAngle = angle;
			}
		}

		#endregion
	}
}