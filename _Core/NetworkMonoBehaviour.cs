/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CommonUtils.Core.Networking
{
	/// <summary>
	/// <para
	/// >Multiplayer-enabled MonoBehaviour with property synchronization using Photon's room properties.
	/// </para>
	/// 
	/// <para>
	/// Implements <see cref="IInRoomCallbacks"/> and <see cref="IMatchmakingCallbacks"/> to listen to room property updates, as well as synchronize property state when joining a room.
	/// </para>
	/// 
	/// <para>
	/// When overriding any functions, call their base functions first. E.g. <c>base.Awake()</c>
	/// </para>
	/// 
	/// <para>
	/// Call <see cref="SendPropertyUpdate(string, object)"/> to send property updates to the network.
	/// <br></br>
	/// Override <see cref="OnReceivePropertyUpdate(string, object)"/> to handle property updates!
	/// </para>
	/// </summary>
	[RequireComponent(typeof(PhotonView))]
	public class NetworkMonoBehaviour : MonoBehaviour, IInRoomCallbacks, IMatchmakingCallbacks
	{
		#region Member Declarations

		/// <summary>
		/// <see cref="PhotonView"/> reference.
		/// </summary>
		protected PhotonView photonView { get; set; }

		/// <summary>
		/// Character that divides a room property string key into the <see cref="NetworkName">network name</see> and property name.
		/// </summary>
		protected static char SplitChar => '|';

		/// <summary>
		/// The unique network identifier of this <see cref="NetworkMonoBehaviour"/>.
		/// </summary>
		public string NetworkName => name + photonView.ViewID;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Networking setup.
		/// </summary>
		protected virtual void Awake()
		{
			// Get PhotonView reference
			photonView = gameObject.GetComponent<PhotonView>();

			// Register network callbacks
			PhotonNetwork.AddCallbackTarget(this);
		}

		/// <summary>
		/// Cleanup.
		/// </summary>
		protected virtual void OnDestroy()
		{
			// Unregister network callbacks
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		#endregion

		#region Network Functions

		/// <summary>
		/// Called when the <see cref="OnRoomPropertiesUpdate(Hashtable)">room properties update</see> has this object's <see cref="NetworkName">network name</see> as the key.
		/// </summary>
		/// <param name="value">Property value.</param>
		protected virtual void OnReceivePropertyUpdate(string propertyName, object value) { }

		/// <summary>
		/// Send a property update to the network.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		/// <param name="value">Propert value.</param>
		protected virtual void SendPropertyUpdate(string propertyName, object value)
		{
			// Check if we're in a room first
			if (PhotonNetwork.InRoom)
			{
				Hashtable newPropertyTable = new Hashtable();

				// Each property key is the NetworkName + SplitChar + property name
				newPropertyTable.Add(NetworkName + SplitChar + propertyName, value);

				// Send update
				PhotonNetwork.CurrentRoom.SetCustomProperties(newPropertyTable);
			}
		}

		#endregion

		#region IInRoomCallbacks

		public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			// Check through all update properties
			foreach (DictionaryEntry property in propertiesThatChanged)
			{
				// If this GameObject has received updates
				string key = (string)property.Key;

				// Split it between the network name of this object, and the property name
				string[] splitKey = key.Split(SplitChar);

				// Check if this property is for this object
				if (splitKey[0] == NetworkName)
				{
					// Receive update
					OnReceivePropertyUpdate(splitKey[1], property.Value);
				}
			}
		}

		#region Unused Methods

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
			// Do nothing
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
			// Do nothing
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
			// Do nothing
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
			// Do nothing
		}

		#endregion

		#endregion

		#region IMatchmakingCallbacks

		/// <summary>
		/// Updates client with the current state of the room properties when joining a room.
		/// </summary>
		public virtual void OnJoinedRoom()
		{
			OnRoomPropertiesUpdate(PhotonNetwork.CurrentRoom.CustomProperties);
		}

		#region Unused Methods

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			// Do nothing
		}

		public void OnCreatedRoom()
		{
			// Do nothing
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			// Do nothing
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			// Do nothing
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			// Do nothing
		}

		public void OnLeftRoom()
		{
			// Do nothing
		}

		#endregion

		#endregion
	}
}