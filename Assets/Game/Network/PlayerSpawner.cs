using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Network
{
	public class PlayerSpawner : NetworkBehaviour
	{
		[SerializeField]
		private PlayerNetworkEntity _playerPrefab;

		private static List<Transform> _spawnPoints = new List<Transform>();

		public override void OnStartServer()
		{
			LobbyNetworkManager.OnServerReadied += SpawnPlayer;
		}

		[ServerCallback]
		private void OnDestroy()
		{
			LobbyNetworkManager.OnServerReadied -= SpawnPlayer;
		}

		public static void AddSpawnPoint(Transform spawnPoint)
		{
			_spawnPoints.Add(spawnPoint);
		}

		public static void RemoveSpawnPoint(Transform spawnPoint)
		{
			_spawnPoints.Remove(spawnPoint);
		}

		[Server]
		private void SpawnPlayer(NetworkConnection connection)
		{
			Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];

			PlayerNetworkEntity player = Instantiate(_playerPrefab, spawnPoint.position, Quaternion.identity);
			player.SetDisplayName(connection.identity.gameObject.GetComponent<PlayerGameEntity>().DisplayName);
			NetworkServer.Spawn(player.gameObject, connection);
		}

		[Server]
		public void RespawnAndResetAllPlayers()
		{
			Debug.Log("Cock cock");
		}
	}
}