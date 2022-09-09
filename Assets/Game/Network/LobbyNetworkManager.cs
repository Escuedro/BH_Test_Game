using System.Collections.Generic;
using Game.View.UI;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Network
{
	public class LobbyNetworkManager : NetworkManager
	{
		[SerializeField]
		private int _minPlayers = 2;
		[SerializeField, Scene]
		private string _menuSceneName;
		[SerializeField]
		private PlayerRoomView _playerRoomPrefab;

		public List<PlayerRoomView> RoomPlayers = new List<PlayerRoomView>();

		public override void OnServerConnect(NetworkConnectionToClient conn)
		{
			if (numPlayers >= maxConnections)
			{
				conn.Disconnect();
				return;
			}

			if (!IsMenuScene)
			{
				conn.Disconnect();
			}
		}

		public override void OnServerAddPlayer(NetworkConnectionToClient conn)
		{
			if (IsMenuScene)
			{
				bool isRoomMaster = RoomPlayers.Count == 0;

				PlayerRoomView roomPlayerView = Instantiate(_playerRoomPrefab);
				roomPlayerView.IsMaster = isRoomMaster;

				NetworkServer.AddPlayerForConnection(conn, roomPlayerView.gameObject);
			}
		}

		public override void OnStopServer()
		{
			base.OnStopServer();
			RoomPlayers.Clear();
		}

		private bool IsMenuScene => SceneManager.GetActiveScene().path == _menuSceneName;

		public override void OnServerDisconnect(NetworkConnectionToClient conn)
		{
			if (conn.identity != null)
			{
				PlayerRoomView player = conn.identity.GetComponent<PlayerRoomView>();
				RoomPlayers.Remove(player);

				UpdateReadyToStartState();
			}
			
			base.OnServerDisconnect(conn);
		}

		public void UpdateReadyToStartState()
		{
			foreach (PlayerRoomView roomPlayerEntity in RoomPlayers)
			{
				roomPlayerEntity.UpdateReadyToStart(IsReadyToStart());
			}
		}

		private bool IsReadyToStart()
		{
			if (RoomPlayers.Count < _minPlayers)
			{
				return false;
			}

			foreach (PlayerRoomView joinedPlayer in RoomPlayers)
			{
				if (!joinedPlayer.IsReady)
				{
					return false;
				}
			}

			return true;
		}
	}
}