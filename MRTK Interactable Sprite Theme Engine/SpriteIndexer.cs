/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Indexes a list of <see cref="Sprite"/>s and uses it to update a <see cref="SpriteRenderer"/> via the list index.
	/// </summary>
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteIndexer : MonoBehaviour
	{
		#region Serialization

		[Title("Configuration")]

		[Tooltip("Sprite list.")]
		[SerializeField]
		private List<Sprite> sprites = new List<Sprite>();

		[Tooltip("Current index.")]
		[PropertyRange(0, "maxSpriteIndex")]
		[SerializeField]
		private int currentIndex = 0;

		#endregion

		#region Member Declarations

		/// <summary>
		/// <see cref="SpriteRenderer"/> reference.
		/// </summary>
		private SpriteRenderer spriteRenderer;

		/// <summary>
		/// Maximum index of <see cref="sprites"/>.
		/// </summary>
		private int maxSpriteIndex => sprites?.Count - 1 ?? 0;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void Awake()
		{
			spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		}

		/// <summary>
		/// Update the <see cref="spriteRenderer"/>.
		/// </summary>
		private void OnEnable()
		{
			SetIndex(currentIndex);
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Change the sprite on this GameObject by changing the index for the sprite to be displayed.
		/// If the index is outside of the sprite list's range, it will be ignored.
		/// </summary>
		/// <param name="index">Index of the sprite in the list to change to.</param>
		public void SetIndex(int index)
		{
			if (sprites != null)
			{
				if (index >= 0 && index <= maxSpriteIndex)
				{
					spriteRenderer.sprite = sprites[index];
				}
			}
		}

		public int GetIndex() => currentIndex;

		#endregion
	}
}
