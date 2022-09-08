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
		private RoomPlayerEntity _playerRoomPrefab;
		[SerializeField]
		private EnterNamePanel _enterNamePanel;

		private List<RoomPlayerEntity> _joinedPlayers = new List<RoomPlayerEntity>();

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
				bool isRoomMaster = _joinedPlayers.Count == 0;

				RoomPlayerEntity roomPlayerEntity = Instantiate(_playerRoomPrefab);
				roomPlayerEntity.Init(_enterNamePanel.PlayerName, (player) => _joinedPlayers.Add(player),
						(player) => _joinedPlayers.Remove(player));

				roomPlayerEntity.SetIsMaster(isRoomMaster);

				NetworkServer.AddPlayerForConnection(conn, roomPlayerEntity.gameObject);
			}
		}

		public override void OnStopServer()
		{
			base.OnStopServer();
			_joinedPlayers.Clear();
		}

		private bool IsMenuScene => SceneManager.GetActiveScene().name == _menuSceneName;

		public override void OnServerDisconnect(NetworkConnectionToClient conn)
		{
			if (conn.identity != null)
			{
				RoomPlayerEntity player = conn.identity.GetComponent<RoomPlayerEntity>();
				_joinedPlayers.Remove(player);

				UpdateReadyState();
			}
			
			base.OnServerDisconnect(conn);
		}

		private void UpdateReadyState()
		{
			foreach (RoomPlayerEntity roomPlayerEntity in _joinedPlayers)
			{
				roomPlayerEntity.UpdateReadyState(IsReadyToStart());
			}
		}

		private bool IsReadyToStart()
		{
			if (_joinedPlayers.Count < _minPlayers)
			{
				return false;
			}

			foreach (RoomPlayerEntity joinedPlayer in _joinedPlayers)
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