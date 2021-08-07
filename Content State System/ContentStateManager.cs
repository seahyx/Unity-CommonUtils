/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using CommonUtils.Networking;
using Photon.Pun;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Simple content state management system.
	/// Multiplayer-enabled using <see cref="NetworkMonoBehaviour"/>.
	/// </summary>
	public class ContentStateManager : NetworkMonoBehaviour
	{
		#region Serialization

		[Header("Configuration")]

		[Tooltip("List of content states.")]
		[SerializeField]
		private List<BaseContentState> contentStates = new List<BaseContentState>();

		[Tooltip("Index of the default content state in the list.")]
		[PropertyRange(0, "maxStateIndex")]
		[SerializeField]
		private int defaultContentStateIndex = 0;

		[Tooltip("Disable initialization calls in OnEnable. Initialize must be called at once for ContentStateManager to work properly.")]
		[SerializeField]
		private bool manualInitialization = true;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Content state stack. Tracks the navigation between states.
		/// </summary>
		private Stack<BaseContentState> stack { get; set; } = new Stack<BaseContentState>();

		/// <summary>
		/// The maximum index of <see cref="contentStates"/>, or, if <see cref="contentStates"/> is null, 0
		/// </summary>
		private int maxStateIndex => contentStates?.Count - 1 ?? 0;

		/// <summary>
		/// Currently active content state index.
		/// This value is changed the moment before the content state is stating its out transition.
		/// Also used for network synchronization.
		/// </summary>
		public int CurrentStateIndex { get; private set; }

		#region Constants

		/// <summary>
		/// Network property keys.
		/// </summary>
		private struct NetworkProperty
		{
			public const string CONTENT_STATE_INDEX = "ContentStateIndex";
		}

		#endregion

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Error checking.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			Debug.Assert(contentStates?.Count > 0, "Content state list cannot be empty!");

			foreach (BaseContentState contentState in contentStates)
				contentState.ContentStateManager = this;
		}

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void OnEnable()
		{
			if (!manualInitialization)
				Initialize();
		}

		#endregion

		#region Public Events

		public void Initialize()
		{
			if (PhotonNetwork.InRoom)
			{
				// Check update from the current state of the room properties
				if (!ParsePropertyTable(PhotonNetwork.CurrentRoom.CustomProperties))
				{
					// There are no network room property values, so we need to initialize
					// But we don't want to send updates, thus isNetwork is true
					Reset(defaultContentStateIndex, isNetwork: true);
				}
			}
			else
			{
				// Initialize content stack
				Reset(defaultContentStateIndex);
			}
		}

		/// <summary>
		/// Invokes sequential <see cref="SetState(int, bool, bool)"/> with <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Target index of the the <see cref="BaseContentState"/> in <see cref="contentStates"/>.</param>
		public void SetStateSequential(int index)
		{
			SetState(index, sequential: true);
		}

		#endregion

		#region State Management Functions

		/// <summary>
		/// Goes back to the home state. Does not reset the current stack.
		/// </summary>
		public void GoHome(bool reset = true)
		{
			if (reset)
				Reset(defaultContentStateIndex);
			else
				SetStateSequential(defaultContentStateIndex);
		}

		/// <summary>
		/// Goes back to the previous state. If there is no previous state, it will not do anything.
		/// </summary>
		public void GoBackSequential()
		{
			// Catch errors
			if (stack.Count <= 1)
			{
				Debug.Log($"[{name}] No previous state to go back to, call will be ignored.");
				return;
			}
			// Check if there is a transition still in progress
			else if (stack.Peek().IsTransitioning)
			{
				Debug.LogWarning($"[{name}] Current state is still in transition, new transition will not be executed.");
				return;
			}

			// Update current content state
			CurrentStateIndex = contentStates.IndexOf(stack.Peek());

			// Begin transitions
			stack.Peek().TransitionOut(() =>
			{
				// Remove current content state
				stack.Pop();
				
				// Transition in the new state
				stack.Peek().TransitionIn();
			});
		}

		/// <summary>
		/// Start the transition to another state.
		/// Will not execute if the current state is still in transition, unless forced is true.
		/// </summary>
		/// <param name="index">Index of the the <see cref="BaseContentState"/> in <see cref="contentStates"/>.</param>
		/// <param name="sequential">Whether the transitions are executed sequentially or in parallel.</param>
		/// <param name="forced">Whether to force the transition to happen, if the current state is still in a transition.</param>
		public void SetState(int index, bool sequential = true, bool forced = false, bool isNetwork = false)
		{
			// Catch errors
			if (index < 0 || index >= contentStates.Count)
			{
				Debug.LogError($"[{name}] Invalid index for setting state: {index}, ContentState list size: {contentStates.Count}");
				return;
			}

			// Reset if stack is empty
			if (stack?.Count == 0)
			{
				Debug.Log($"[{name}] Content state stack is empty, initializing it with index {index}.");
				Reset(index);
			}

			// Skip if index is the same
			if (CurrentStateIndex == index)
			{
				Debug.Log($"[{name}] SetState() was called with an index the same as the current index, thus will be ignored.");
				return;
			}

			// Check if there is a transition still in progress
			if (stack.Peek().IsTransitioning)
			{
				if (forced || isNetwork)
				{
					Debug.LogWarning($"[{name}] Current state is still in transition, forcing transition to another state.");
				}
				else
				{
					Debug.LogWarning($"[{name}] Current state is still in transition, new transition will not be executed.");
					return;
				}
			}

			// Update current content state
			CurrentStateIndex = index;

			// Update network
			if (!isNetwork)
				SendContentStateNetworkUpdate(CurrentStateIndex);

			if (sequential)
			{
				// These transitions will be executed sequentially

				// Transition out the current state
				stack.Peek().TransitionOut(() =>
				{
					// Add new state to stack
					stack.Push(contentStates[index]);

					// Transition in the new state
					stack.Peek().TransitionIn();
				});
			}
			else
			{
				// These transitions will be executed in parallel

				// Transition out the current state
				stack.Peek().TransitionOut();

				// Add new state to stack
				stack.Push(contentStates[index]);

				// Transition in the new state
				stack.Peek().TransitionIn();
			}
		}

		/// <summary>
		/// Disables all other states, reset the stack, and return to selected index.
		/// </summary>
		public void Reset(int index, bool isNetwork = false)
		{
			foreach (BaseContentState state in contentStates)
				state.gameObject.SetActive(false);

			stack.Clear();
			stack.Push(contentStates[index]);
			stack.Peek().gameObject.SetActive(true);
			stack.Peek().TransitionIn();

			CurrentStateIndex = index;

			// Update network
			if (!isNetwork)
				SendContentStateNetworkUpdate(CurrentStateIndex);
		}

		#endregion

		#region Networking

		/// <summary>
		/// Updates <see cref="CurrentStateIndex"/> on the network if connected.
		/// </summary>
		/// <param name="index">Current content state index.</param>
		private void SendContentStateNetworkUpdate(int index)
		{
			SendPropertyUpdate(NetworkProperty.CONTENT_STATE_INDEX, index);
		}

		protected override void OnReceivePropertyUpdate(string propertyName, object value)
		{
			switch (propertyName)
			{
				case NetworkProperty.CONTENT_STATE_INDEX:
					// Get index
					int index = (int)value;

					// Update content state
					SetState(index, isNetwork: true);
					break;
			}
		}

		#endregion
	}
}
