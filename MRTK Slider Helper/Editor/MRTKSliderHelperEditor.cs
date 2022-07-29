/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Inspector script for <see cref="MRTKSliderHelper"/>.
	/// </summary>
	[CustomEditor(typeof(MRTKSliderHelper)), CanEditMultipleObjects]
	public class MRTKSliderHelperEditor : Editor
	{
		#region Member Declarations

		private MRTKSliderHelper sliderHelper { get; set; }
		private List<MRTKSliderHelper> sliderHelpers { get; set; } = new List<MRTKSliderHelper>();

		#endregion

		#region Lifecycle Functions

		void OnEnable()
		{
			CastTargets();
		}

		#endregion

		#region Inspector Functions

		/// <summary>
		/// Cast selected target objects.
		/// </summary>
		private void CastTargets()
		{
			sliderHelper = (MRTKSliderHelper)target;
			sliderHelpers = new List<MRTKSliderHelper>();
			foreach (var t in targets)
			{
				MRTKSliderHelper obj = (MRTKSliderHelper)t;
				sliderHelpers.Add(obj);
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			if (GUILayout.Button("Update Slider"))
			{
				foreach (MRTKSliderHelper sh in sliderHelpers)
				{
					// Get calculations
					float sliderThumbLength = sh.CalculateThumbLength();
					float actualSliderLength = sh.CalculateActualSliderLength();

					// First, update PinchSlider values
					if (sh.PinchSlider)
					{
						SerializedObject pinchSlider = new SerializedObject(sh.PinchSlider);
						pinchSlider.FindProperty("sliderStartDistance").floatValue = actualSliderLength / 2 * (sh.InvertDirection ? 1 : -1);
						pinchSlider.FindProperty("sliderEndDistance").floatValue = actualSliderLength / 2 * (sh.InvertDirection ? -1 : 1);
						pinchSlider.ApplyModifiedProperties();
					}

					// Second, update background sprite size
					if (sh.SliderBackground)
					{
						SerializedObject sliderBackground = new SerializedObject(sh.SliderBackground);
						sliderBackground.FindProperty("m_Size").vector2Value = new Vector2(sh.TotalLength, sh.SliderBackground.size.y);
						sliderBackground.ApplyModifiedProperties();
					}

					// Third, update slider thumb
					if (sh.SliderThumb && sh.FitThumbToMaxScroll)
					{
						SerializedObject sliderThumb = new SerializedObject(sh.SliderThumb);
						sliderThumb.FindProperty("m_Size").vector2Value = new Vector2(sliderThumbLength, sh.SliderThumb.size.y);
						sliderThumb.ApplyModifiedProperties();
					}

					if (sh.SliderThumbCollider)
					{
						SerializedObject thumbCollider = new SerializedObject(sh.SliderThumbCollider);
						thumbCollider.FindProperty("m_Size").vector3Value = new Vector3(
							sliderThumbLength + sh.ColliderLengthwisePadding,
							sh.SliderThumbCollider.size.y,
							sh.SliderThumbCollider.size.z);
						thumbCollider.ApplyModifiedProperties();
					}
				}
			}
		}

		#endregion
	}
}
