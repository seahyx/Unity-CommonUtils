/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CommonUtils.ContentState
{

	/// <summary>
	/// Base class for a state of the content.
	/// </summary>
	public class BaseContentState : MonoBehaviour
	{
		#region Type Definitions

		/// <summary>
		/// Delegate for transition callback events.
		/// </summary>
		public delegate void OnTransitionCompleted();

		#endregion

		#region Serialization

		[Header("Base Content State Settings")]

		[Tooltip("Gets invoked when the content state has started its in transition.")]
		[SerializeField]
		public UnityEvent OnTransitionInStarted = new UnityEvent();

		[Tooltip("Gets invoked when the content state has finished its in transition.")]
		[SerializeField]
		public UnityEvent OnTransitionInCompleted = new UnityEvent();

		[Tooltip("Gets invoked when the content state has started its out transition.")]
		[SerializeField]
		public UnityEvent OnTransitionOutStarted = new UnityEvent();

		[Tooltip("Gets invoked when the content state has finished its out transition.")]
		[SerializeField]
		public UnityEvent OnTransitionOutCompleted = new UnityEvent();

		#endregion

		#region Member Declarations

		/// <summary>
		/// The content state manager this content state is being handled by.
		/// Reference is assigned by the content state manager.
		/// </summary>
		public ContentStateManager ContentStateManager { get; set; }

		/// <summary>
		/// Whether this state is still in the middle of a transition.
		/// </summary>
		public bool IsTransitioning { get; private set; } = false;

		/// <summary>
		/// The reference for the transition enumerator, if any.
		/// </summary>
		private IEnumerator transitionEnumerator { get; set; }

		#endregion

		#region Content State Functions

		/// <summary>
		/// This function will begin the out transition for this state.
		/// It will not stop any running transitions, but it will log a warning if another transition is active.
		/// </summary>
		/// <param name="onTransitionCompleted">Callback to be executed when transition is complete.</param>
		public void TransitionOut(OnTransitionCompleted onTransitionCompleted = null)
		{
			if (IsTransitioning)
				Debug.LogWarning($"[{name}] This state is attempting to transition out while a transition is already running. It may cause issues.");

			transitionEnumerator = AnimateTransitionOut(onTransitionCompleted);
			IsTransitioning = true;

			Debug.Log($"[{name}] Starting out transition of {name}.");

			OnTransitionOutStarted?.Invoke();

			StartCoroutine(transitionEnumerator);
		}

		/// <summary>
		/// This function will begin the in transition for this state.
		/// It will not stop any running transitions, but it will log a warning if another transition is active.
		/// </summary>
		/// <param name="onTransitionCompleted">Callback to be executed when transition is complete.</param>
		public void TransitionIn(OnTransitionCompleted onTransitionCompleted = null)
		{
			if (IsTransitioning)
				Debug.LogWarning($"[{name}] This state is attempting to transition in while a transition is already running. It may cause issues.");

			// Show itself
			gameObject.SetActive(true);

			transitionEnumerator = AnimateTransitionIn(onTransitionCompleted);
			IsTransitioning = true;

			Debug.Log($"[{name}] Starting in transition of {name}.");

			OnTransitionInStarted?.Invoke();

			StartCoroutine(transitionEnumerator);
		}

		/// <summary>
		/// This function is called whenever the app is switching from this state to another.
		/// Base function must be called at the end of the override.
		/// <br></br><br></br>
		/// <example>At the end of the override function, call this:
		/// <code>
		/// yield return StartCoroutine(base.AnimateTransitionOut(onTransitionCompleted));
		/// </code>
		/// </example>
		/// </summary>
		/// <param name="onTransitionCompleted">Callback to be executed when transition is complete.</param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateTransitionOut(OnTransitionCompleted onTransitionCompleted)
		{
			// Set transition flag
			IsTransitioning = false;

			Debug.Log($"[{name}] Out transition of {name} completed.");

			yield return null;

			// Execute callbacks
			onTransitionCompleted?.Invoke();
			OnTransitionOutCompleted?.Invoke();

			// Hide itself
			gameObject.SetActive(false);
		}

		/// <summary>
		/// This function is called whenever the app is switching from another state to this.
		/// Base function must be called at the end of the override.
		/// <br></br><br></br>
		/// <example>At the end of the override function, call this:
		/// <code>
		/// yield return StartCoroutine(base.AnimateTransitionIn(onTransitionCompleted));
		/// </code>
		/// </example>
		/// </summary>
		/// <param name="onTransitionCompleted">Callback to be executed when transition is complete.</param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateTransitionIn(OnTransitionCompleted onTransitionCompleted)
		{
			// Set transition flag
			IsTransitioning = false;

			Debug.Log($"[{name}] In transition of {name} completed.");

			yield return null;

			// Execute callbacks
			onTransitionCompleted?.Invoke();
			OnTransitionInCompleted?.Invoke();
		}

		#endregion
	}
}
