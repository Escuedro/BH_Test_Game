using System;
using System.Collections.Generic;
using Game.Model;
using Game.Network.Messages;
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
		private PlayerNetworkEntity _gamePlayerPrefab;
		[SerializeField]
		private PlayerSpawner _playerSpawnerPrefab;
		[SerializeField]
		private UpdateTimer _updateTimerPrefab;

		public List<PlayerRoomView> RoomPlayers { get; } = new List<PlayerRoomView>();
		public List<PlayerNetworkEntity> GamePlayers { get; } = new List<PlayerNetworkEntity>();

		private PlayerSpawner _playerSpawner;

		private void IncreasePoints(PlayerNetworkEntity attacker)
		{
			attacker.Points++;
			if (attacker.Points >= _pointsNeededToWin)
			{
				WinGame(attacker);
			}
		}

		private void WinGame(PlayerNetworkEntity winner)
		{
			UpdateTimer updateTimer = Instantiate(_updateTimerPrefab);
			GamePlayers.ForEach(gamePlayer => gamePlayer.CanMove = false);
			updateTimer.Init(() =>
			{
				ResetAllPlayers();
				_playerSpawner.ResetPoints();
				NetworkServer.SendToAll(new GameRestartMessage());
			}, _winStateDuration);
			NetworkServer.SendToAll(new GameWinMessage() {WinnerName = winner.DisplayName});
		}

		private void ResetAllPlayers()
		{
			foreach (PlayerNetworkEntity playerGameEntity in GamePlayers)
			{
				playerGameEntity.Points = 0;
				playerGameEntity.transform.position = _playerSpawner.GetPlayerPosition();
				playerGameEntity.CanMove = true;
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
			GamePlayers.Clear();
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
					PlayerNetworkEntity gamePlayerInstance = Instantiate(_gamePlayerPrefab);
					gamePlayerInstance.SetDisplayName(playerRoomView.DisplayName);
					gamePlayerInstance.gameObject.SetActive(false);
					gamePlayerInstance.CanMove = false;
					gamePlayerInstance.OnRushCollision += player =>
					{
						player.Hit();
						IncreasePoints(gamePlayerInstance);
					};

					NetworkServer.Destroy(connectionToClient.identity.gameObject);

					GamePlayers.Add(gamePlayerInstance);
					NetworkServer.ReplacePlayerForConnection(connectionToClient, gamePlayerInstance.gameObject);
				}
			}

			base.ServerChangeScene(newSceneName);
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			if (IsGameScene)
			{
				_playerSpawner = Instantiate(_playerSpawnerPrefab);

				_playerSpawner.ResetPoints();

				Cursor.lockState = CursorLockMode.Confined;

				foreach (PlayerNetworkEntity playerNetworkEntity in GamePlayers)
				{
					playerNetworkEntity.gameObject.SetActive(true);
					playerNetworkEntity.CanMove = true;
					playerNetworkEntity.transform.position = _playerSpawner.GetPlayerPosition();
				}
			}
		}
	}
}