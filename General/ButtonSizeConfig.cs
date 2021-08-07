/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Controls the size of the button without changing the scale of the text.
	/// Requires editor script <see cref="ButtonSizeConfigEditor"/>.
	/// </summary>
	[ExecuteAlways]
	public class ButtonSizeConfig : MonoBehaviour
	{
		#region Serialization

		[Tooltip("Default length unit of the button.")]
		[SerializeField]
		private Vector3 defaultButtonSize = new Vector3(0.032f, 0.032f, 0.016f);

		[Tooltip("Percentage of the width or height for the margins.")]
		[SerializeField, Range(0.0f, 1.0f)]
		private float marginRatio = 0.05f;
		
		[Tooltip("List of Transforms to be scaled. E.g. Backplate, Frontplate")]
		[SerializeField]
		public List<Transform> transformList = new List<Transform>();

		[Tooltip("List of SLICED sprite renderers to be scaled. Adjusts the width and height values of the sprite renderer instead of scale.")]
		[SerializeField]
		public List<SpriteRenderer> slicedSprRList = new List<SpriteRenderer>();

		[Tooltip("Button text Rect Transform reference.")]
		[SerializeField]
		public RectTransform textRT;
		
		[Tooltip("Button text TMP reference.")]
		[SerializeField]
		public TextMeshPro textTMP;
		
		[Tooltip("Button BoxCollider reference.")]
		[SerializeField]
		public BoxCollider boxCollider;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Field for <see cref="ButtonScale"/>. Accessed by <see cref="ButtonSizeConfigEditor"/>.
		/// </summary>
		[HideInInspector]
		public Vector3 _buttonScale = Vector3.one;

		/// <summary>
		/// The scale of the width of the button.
		/// </summary>
		public Vector3 ButtonScale
		{
			get => _buttonScale;
			set
			{
				_buttonScale = value;

				foreach (Transform t in transformList)
					t.localScale = value;

				foreach (SpriteRenderer spr in slicedSprRList)
					spr.size = RectWidthHeight;

				if (textRT)
					textRT.sizeDelta = RectWidthHeight;

				if (textTMP)
					textTMP.margin = TextMargins;

				if (boxCollider)
					boxCollider.size = ColliderScale;
			}
		}

		/// <summary>
		/// Width of the button.
		/// </summary>
		public float Width
		{
			get => ButtonScale.x * defaultButtonSize.x * transform.localScale.x;
		}

		/// <summary>
		/// Height of the button.
		/// </summary>
		public float Height
		{
			get => ButtonScale.y * defaultButtonSize.y * transform.localScale.y;
		}

		/// <summary>
		/// Rect Transform width and height vectors.
		/// </summary>
		public Vector2 RectWidthHeight
		{
			get
			{
				Vector2 newScale = defaultButtonSize;
				newScale.x *= ButtonScale.x;
				newScale.y *= ButtonScale.y;
				return newScale;
			}
		}

		/// <summary>
		/// Text margins vector.
		/// </summary>
		public Vector4 TextMargins
		{
			get
			{
				Vector4 newMargin = new Vector4();

				// Width
				newMargin.x = newMargin.z = (Width / 2) * marginRatio;

				// Height
				newMargin.w = newMargin.y = (Height / 2) * marginRatio;

				return newMargin;
			}
		}

		/// <summary>
		/// Collider scale vector.
		/// </summary>
		public Vector3 ColliderScale
		{
			get
			{
				Vector3 newScale = defaultButtonSize;
				newScale.x *= ButtonScale.x;
				newScale.y *= ButtonScale.y;
				newScale.z *= ButtonScale.z;
				return newScale;
			}
		}

		#endregion
	}
}
