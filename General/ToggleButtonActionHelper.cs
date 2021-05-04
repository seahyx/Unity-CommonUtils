/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// <para>
	/// Toggles an <see cref="interactable"/> toggle button to your desired state. Triggers a button press only if the current state of the toggle does not match the desired state.
	/// </para>
	/// Presently does not work with multi-dimensional buttons.
	/// </summary>
	[RequireComponent(typeof(Interactable))]
	public class ToggleButtonActionHelper : MonoBehaviour
	{
		#region Member Declarations

		/// <summary>
		/// <see cref="Interactable"/> reference.
		/// </summary>
		private Interactable interactable { get; set; }

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void Awake()
		{
			interactable = gameObject.GetComponent<Interactable>();
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Triggers a button press only if the current state of the toggle does not match the desired <paramref name="toggleState"/>.
		/// </summary>
		/// <param name="toggleState">Target toggle state.</param>
		public void SetToggleState(bool toggleState)
		{
			if (interactable.ButtonMode == SelectionModes.Toggle && interactable.IsToggled != toggleState)
				interactable.TriggerOnClick();
		}

		#endregion
	}
}
