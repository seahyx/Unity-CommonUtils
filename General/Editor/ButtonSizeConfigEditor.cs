/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CommonUtils
{
	/// <summary>
	/// Inspector script for <see cref="ButtonSizeConfig"/>.
	/// </summary>
	[CustomEditor(typeof(ButtonSizeConfig)), CanEditMultipleObjects]
	public class ButtonSizeConfigEditor : Editor
	{
		#region Member Declarations

		private ButtonSizeConfig buttonConfig { get; set; }
		private List<ButtonSizeConfig> buttonConfigs { get; set; } = new List<ButtonSizeConfig>();


		// Property References

		private SerializedProperty buttonScaleProp { get; set; }

		#endregion

		#region Lifecycle Functions

		void OnEnable()
		{
			CastTargets();
			buttonScaleProp = serializedObject.FindProperty("_buttonScale");
		}

		#endregion

		#region Inspector Functions

		/// <summary>
		/// Cast selected target objects.
		/// </summary>
		private void CastTargets()
		{
			buttonConfig = (ButtonSizeConfig)target;
			buttonConfigs = new List<ButtonSizeConfig>();
			foreach (var t in targets)
			{
				ButtonSizeConfig obj = (ButtonSizeConfig)t;
				buttonConfigs.Add(obj);
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal();

			Vector3 buttonScale = EditorGUILayout.Vector3Field(
				"Button Scale", buttonConfig.ButtonScale);

			GUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				buttonScaleProp.vector3Value = buttonScale;
				serializedObject.ApplyModifiedProperties();

				foreach (ButtonSizeConfig bConfig in buttonConfigs)
				{
					// Update transform list
					if (bConfig.transformList.Count != 0)
					{
						SerializedObject transforms = new SerializedObject(bConfig.transformList.ToArray());
						transforms.FindProperty("m_LocalScale").vector3Value = buttonScale;
						transforms.ApplyModifiedProperties();
					}

					// Update TMP rect transform width height
					if (bConfig.textRT)
					{
						SerializedObject textRTSO = new SerializedObject(bConfig.textRT);
						textRTSO.FindProperty("m_SizeDelta").vector2Value = bConfig.RectWidthHeight;
						textRTSO.ApplyModifiedProperties();
					}

					// Update TMP margins
					if (bConfig.textTMP)
					{
						SerializedObject textTMPSO = new SerializedObject(bConfig.textTMP);
						textTMPSO.FindProperty("m_margin").vector4Value = bConfig.TextMargins;
						textTMPSO.ApplyModifiedProperties();
					}

					// Update box collider scale
					if (bConfig.boxCollider)
					{
						SerializedObject boxColliderSO = new SerializedObject(bConfig.boxCollider);
						boxColliderSO.FindProperty("m_Size").vector3Value = bConfig.ColliderScale;
						boxColliderSO.ApplyModifiedProperties();
					}
				}
			}
		}

		#endregion
	}
}