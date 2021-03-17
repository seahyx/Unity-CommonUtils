/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using ExitGames.Client.Photon;
using Photon.Pun;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Manages the state of the content and flow.
	/// Multiplayer-enabled using Photon.
	/// </summary>
	public class ContentStateManager : MonoBehaviour
	{
		#region Serialization

		[Header("Configuration")]

		[Tooltip("List of content states.")]
		[SerializeField]
		private List<BaseContentState> contentStates;

		[Tooltip("Index of the default content state in the list.")]
		[PropertyRange(0, "maxStateIndex")]
		[SerializeField]
		private int defaultContentStateIndex = 0;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Content state stack. Tracks the navigation between states.
		/// </summary>
		private Stack<BaseContentState> stack { get; set; } = new Stack<BaseContentState>();

		/// <summary>
		/// The maximum index of contentStates, or, if contentStates is null, 0
		/// </summary>
		private int maxStateIndex => contentStates?.Count - 1 ?? 0;

		/// <summary>
		/// Field for currentStateIndex.
		/// </summary>
		private int _currentStateIndex = 0;

		/// <summary>
		/// Currently active content state index. This value is changed the moment before the content state is stating its out transition. Used for network synchronization.
		/// </summary>
		private int currentStateIndex
		{
			get => _currentStateIndex;
			set
			{
				_currentStateIndex = value;
				UpdateNetworkStateIndex(value);
			}
		}

		/// <summary>
		/// String key for the room property of the currently active content state.
		/// </summary>
		private const string CURRENT_CONTENT_STATE_INDEX_RPKEY = "ContentStateManager_CurrentContentState";

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Error checking.
		/// </summary>
		private void Awake()
		{
			Debug.Assert(contentStates?.Count > 0, "Content state list cannot be empty!");

			foreach (BaseContentState contentState in contentStates)
				contentState.ContentStateManager = this;
		}

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void OnEnable()
		{
			// Reset the stack every time content is loaded.
			stack = new Stack<BaseContentState>();

			// If there are no network updates, reset to default index
			if (!UpdateFromNetworkStateIndex())
				Reset(defaultContentStateIndex);
		}

		#endregion

		#region Unity Events

		/// <summary>
		/// Unity Event for sequential SetState()
		/// </summary>
		/// <param name="index">Index of the the ContentState in the ContentState list.</param>
		public void SetStateSequential(int index)
		{
			SetState(index, true);
		}

		#endregion

		#region State Management Functions

		/// <summary>
		/// Goes back to the home state. Does not reset the current stack.
		/// </summary>
		public void GoHome()
		{
			SetState(defaultContentStateIndex);
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
			currentStateIndex = contentStates.IndexOf(stack.Peek());

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
		/// <param name="index">Index of the the ContentState in the ContentState list.</param>
		/// <param name="sequential">Whether the transitions are executed sequentially or in parallel.</param>
		/// <param name="forced">Whether to force the transition to happen, if the current state is still in a transition.</param>
		public void SetState(int index, bool sequential = true, bool forced = false)
		{
			// Catch errors
			if (index < 0 || index >= contentStates.Count)
			{
				Debug.LogError($"[{name}] Invalid index for setting state: {index}, ContentState list size: {contentStates.Count}");
				return;
			}

			// Check if there is a transition still in progress
			if (stack.Peek().IsTransitioning)
			{
				if (forced)
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
			currentStateIndex = index;

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
		public void Reset(int index)
		{
			foreach (BaseContentState state in contentStates)
				state.gameObject.SetActive(false);

			stack.Clear();
			stack.Push(contentStates[index]);
			stack.Peek().gameObject.SetActive(true);
			stack.Peek().TransitionIn();

			currentStateIndex = index;
		}

		#endregion

		#region Networking Functions

		/// <summary>
		/// Updates current state index on the network if connected.
		/// </summary>
		/// <param name="index">Current content state index.</param>
		private void UpdateNetworkStateIndex(int index)
		{
			// Only the host shall update when in networked mode
			if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
			{
				// Update network
				Hashtable hashtable = new Hashtable();
				hashtable.Add(CURRENT_CONTENT_STATE_INDEX_RPKEY, index);
				PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
			}
		}

		/// <summary>
		/// Updates current state index from the network if connected.
		/// </summary>
		/// <returns>Whether the current state index was updated from the network.</returns>
		private bool UpdateFromNetworkStateIndex()
		{
			bool hasNetworkUpdate = false;

			// Only the host shall update when in networked mode
			if (PhotonNetwork.InRoom)
			{
				// Iterate through the changes in the room properties
				foreach (System.Collections.DictionaryEntry property in PhotonNetwork.CurrentRoom.CustomProperties)
				{
					// Update content state

					// Check if whether the property key is the content state index key 
					if ((string)property.Key == CURRENT_CONTENT_STATE_INDEX_RPKEY)
					{
						if (currentStateIndex != (int)property.Value)
						{
							currentStateIndex = (int)property.Value;
							Reset(currentStateIndex);
							hasNetworkUpdate = true;
						}
					}
				}
			}

			return hasNetworkUpdate;
		}

		#endregion
	}
}
