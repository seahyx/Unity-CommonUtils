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

namespace CommonUtils.Networking
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
		/// Field for <see cref="PhotonView"/>.
		/// </summary>
		private PhotonView _photonView;
		/// <summary>
		/// <see cref="Photon.Pun.PhotonView"/> reference.
		/// </summary>
		public PhotonView PhotonView
		{
			get
			{
				// Check if null
				if (!_photonView)
				{
					_photonView = PhotonView.Get(this);
				}
				return _photonView;
			}
			protected set
			{
				_photonView = value;
			}
		}

		/// <summary>
		/// Character that divides a room property string key into the <see cref="NetworkName">network name</see> and property name.
		/// </summary>
		protected static char SplitChar => '|';

		/// <summary>
		/// The unique network identifier of this <see cref="NetworkMonoBehaviour"/>.
		/// </summary>
		public string NetworkName => name + PhotonView.ViewID;

		/// <summary>
		/// Whether to delete room properties for this object in <see cref="OnDestroy"/>.
		/// </summary>
		public bool DeletePropertiesOnDestroy { get; set; } = true;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Networking setup.
		/// </summary>
		protected virtual void Awake()
		{
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

			// Check if we're in a room first
			if (PhotonNetwork.InRoom && DeletePropertiesOnDestroy)
			{
				// List of properties to delete
				List<string> properties = new List<string>();

				// Clear room properties of this object's properties (if any)
				foreach (DictionaryEntry property in PhotonNetwork.CurrentRoom.CustomProperties)
				{
					// Get the room prop key
					string key = (string)property.Key;

					// Split it between the network name of this object, and the property name
					string[] splitKey = key.Split(SplitChar);

					// Check if this property is for this object
					if (splitKey[0] == NetworkName)
					{
						// Add to list of properties to delete
						properties.Add(splitKey[1]);
					}
				}

				// Send null update to these propeties to delete them
				SendPropertiesUpdate(properties, null);
			}
		}

		#endregion

		#region Network Functions

		/// <summary>
		/// Override this function to handle updates when the <see cref="OnRoomPropertiesUpdate(Hashtable)">room properties update</see> has this object's <see cref="NetworkName">network name</see> as the key.
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
				newPropertyTable.Add(GetPropertyKey(propertyName), value);

				// Send update
				PhotonNetwork.CurrentRoom.SetCustomProperties(newPropertyTable);
			}
		}

		/// <summary>
		/// Send multiple property updates to the network.
		/// The array size of <paramref name="propertyNames"/> should match the array size of <paramref name="values"/>.
		/// <para>
		/// If there are less <paramref name="values"/> than there are <paramref name="propertyNames"/>, the remaining properties that do not have a corresponding value will be have <see langword="null"/> as the value instead.
		/// </para>
		/// Extra values will be ignored. Passing <see langword="null"/> to <paramref name="values"/> will set all the property values to null.
		/// </summary>
		/// <param name="propertyNames">Array of property names.</param>
		/// <param name="values">Array of values corresponding to the <paramref name="propertyNames"/>.</param>
		protected virtual void SendPropertiesUpdate(List<string> propertyNames, List<object> values)
		{
			if (PhotonNetwork.InRoom)
			{
				if (propertyNames == null || propertyNames.Count == 0)
				{
					Debug.LogWarning($"[{name}] propertyNames is empty or null, no room properties will be updated.");
					return;
				}

				Hashtable newPropertyTable = new Hashtable();

				for (int i = 0; i < propertyNames.Count; i++)
				{
					// If values is null or i is out of range of the values array, then value will be null
					object value = values == null ? null : i < values.Count ? values[i] : null;

					// Null values will remove the room property entry
					newPropertyTable.Add(GetPropertyKey(propertyNames[i]), value);
				}

				// Send update
				PhotonNetwork.CurrentRoom.SetCustomProperties(newPropertyTable);
			}
		}

		/// <summary>
		/// Parse the room property hashtable.
		/// </summary>
		/// <param name="propertiesThatChanged">Property hashtable.</param>
		/// <returns>Whether there was any property updated.</returns>
		protected virtual bool ParsePropertyTable(Hashtable propertiesThatChanged)
		{
			bool hasUpdate = false;

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

					// Toggle flag
					hasUpdate = true;
				}
			}

			return hasUpdate;
		}

		/// <summary>
		/// Convert a property name to a property key.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		protected string GetPropertyKey(string propertyName) => NetworkName + SplitChar + propertyName;

		#endregion

		#region IInRoomCallbacks

		public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			ParsePropertyTable(propertiesThatChanged);
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
			ParsePropertyTable(PhotonNetwork.CurrentRoom.CustomProperties);
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