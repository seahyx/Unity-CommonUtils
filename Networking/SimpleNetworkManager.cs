/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using CommonUtils.Core.Networking;

namespace CommonUtils.Networking
{
	/// <summary>
	/// Performs basic Photon networking management and logging.
	/// </summary>
	public class SimpleNetworkManager : NetworkMonoBehaviour
	{
		#region Room Property Callbacks

		public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			Debug.Log($"[{name}] Room properties updated:\n" + propertiesThatChanged);
		}

		public override void OnJoinedRoom()
		{
			Debug.Log($"[{name}] Updating from existing room properties:\n" + PhotonNetwork.CurrentRoom.CustomProperties);

			base.OnJoinedRoom();
		}

		#endregion
	}
}