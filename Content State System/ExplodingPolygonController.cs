/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Controller for the exploding polygon shader.
	/// Will stick to the main camera, but maintain original rotation.
	/// Use in conjunction with the <see cref="ExplodePolygonContentState"/> to animate cool content state transitions.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	public class ExplodingPolygonController : MonoBehaviour
	{
		#region Member Declarations

		/// <summary>
		/// Field for <see cref="meshRenderer"/>
		/// </summary>
		private MeshRenderer _meshRenderer;

		/// <summary>
		/// <see cref="MeshRenderer"/> component reference.
		/// </summary>
		private MeshRenderer meshRenderer
		{
			get
			{
				if (!_meshRenderer)
					_meshRenderer = gameObject.GetComponent<MeshRenderer>();
				return _meshRenderer;
			}
		}

		/// <summary>
		/// Field for <see cref="Material"/>.
		/// </summary>
		private Material _material;

		/// <summary>
		/// Polygon Explode <see cref="Material"/> reference.
		/// </summary>
		public Material Material
		{
			get
			{
				if (!_material)
					_material = meshRenderer.material;
				return _material;
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			meshRenderer.sortingOrder = 99;
			SetAnimationProgress(1.0f);
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Set the _AnimationProgress float value of the material on this object to <paramref name="progress"/>.
		/// </summary>
		/// <param name="progress">Animation progress value. [0..1]</param>
		public void SetAnimationProgress(float progress)
		{
			Material.SetFloat("_AnimationProgress", progress);
		}

		/// <summary>
		/// Set the position of this object to the main camera.
		/// </summary>
		public void SetPositionToCamera()
		{
			transform.position = Camera.main.transform.position;
		}

		#endregion

	}
}
