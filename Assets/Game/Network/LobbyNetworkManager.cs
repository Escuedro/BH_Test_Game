using System;
using System.Collections.Generic;
using Game.Utils;
using Game.View.UI;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Network
{
	public class LobbyNetworkManager : NetworkManager
	{
		[SerializeField]
		private int _pointsNeededToWin = 3;
		[SerializeField]
		private float _winStateDuration = 5f;
		[SerializeField]
		private int _minPlayers = 2;
		[SerializeField, Scene]
		private string _menuSceneName;
		[SerializeField, Scene]
		private string _gameSceneName;
		[SerializeField]
		private PlayerRoomView _playerRoomPrefab;
		[SerializeField]
		private PlayerGameEntity _gamePlayerPrefab;
		[SerializeField]
		private PlayerSpawner _playerSpawner;
		[SerializeField]
		private UpdateTimer _updateTimerPrefab;

		public List<PlayerRoomView> RoomPlayers { get; } = new List<PlayerRoomView>();
		public List<PlayerGameEntity> GamePlayers { get; } = new List<PlayerGameEntity>();

		public static Action<NetworkConnection> OnServerReadied;

		private float _winStateTimer;

		public override void Start()
		{
			base.Start();
			NetworkServer.RegisterHandler<IncreasePointMessage>(OnPointIncreased);
		}

		private void OnPointIncreased(NetworkConnectionToClient connection, IncreasePointMessage points)
		{
			PlayerGameEntity playerGameEntity = connection.identity.GetComponent<PlayerGameEntity>();
			playerGameEntity.Points++;
			if (playerGameEntity.Points >= _pointsNeededToWin)
			{
				WinGame(connection);
			}
		}

		private void WinGame(NetworkConnectionToClient winnerConnection)
		{
			PlayerGameEntity playerGameEntity = winnerConnection.identity.GetComponent<PlayerGameEntity>();
			UpdateTimer updateTimer = Instantiate(_updateTimerPrefab);
			updateTimer.Init(() =>
			{
				ResetAllPoints();
				_playerSpawner.RespawnAndResetAllPlayers();
			}, _winStateDuration);
			NetworkServer.SendToAll(new GameWinMessage() {WinnerName = playerGameEntity.DisplayName});
		}

		private void ResetAllPoints()
		{
			foreach (PlayerGameEntity playerGameEntity in GamePlayers)
			{
				playerGameEntity.Points = 0;
			}
		}

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

		private bool IsGameScene => SceneManager.GetActiveScene().path == _gameSceneName;

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

		public void StartGame()
		{
			if (IsMenuScene)
			{
				if (!IsReadyToStart())
				{
					return;
				}

				ServerChangeScene(_gameSceneName);
			}
		}

		public override void ServerChangeScene(string newSceneName)
		{
			if (IsMenuScene)
			{
				for (int i = RoomPlayers.Count - 1; i >= 0; i--)
				{
					PlayerRoomView playerRoomView = RoomPlayers[i];
					NetworkConnectionToClient connectionToClient = playerRoomView.connectionToClient;
					PlayerGameEntity gamePlayerInstance = Instantiate(_gamePlayerPrefab);
					gamePlayerInstance.SetDisplayName(playerRoomView.DisplayName);

					NetworkServer.Destroy(connectionToClient.identity.gameObject);

					NetworkServer.ReplacePlayerForConnection(connectionToClient, gamePlayerInstance.gameObject);
				}
			}

			base.ServerChangeScene(newSceneName);
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			if (IsGameScene)
			{
				GameObject playerSpawner = Instantiate(_playerSpawner.gameObject);
				NetworkServer.Spawn(playerSpawner);

				Cursor.lockState = CursorLockMode.Confined;
			}
		}

		public override void OnServerReady(NetworkConnectionToClient conn)
		{
			base.OnServerReady(conn);

			if (IsGameScene)
			{
				OnServerReadied?.Invoke(conn);
			}
		}
	}
}