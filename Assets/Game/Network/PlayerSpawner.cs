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

		private List<Transform> _pointsAvailableToSpawn = new List<Transform>();

		private List<PlayerNetworkEntity> _activePlayers = new List<PlayerNetworkEntity>();

		public static void AddSpawnPoint(Transform spawnPoint)
		{
			_spawnPoints.Add(spawnPoint);
		}

		public static void RemoveSpawnPoint(Transform spawnPoint)
		{
			_spawnPoints.Remove(spawnPoint);
		}

		[Server]
		public void ResetPoints()
		{
			_pointsAvailableToSpawn.Clear();
			_pointsAvailableToSpawn.AddRange(_spawnPoints);
		}

		[Server]
		public Vector3 GetPlayerPosition()
		{
			Transform randomPoint = _pointsAvailableToSpawn[Random.Range(0, _pointsAvailableToSpawn.Count)];
			_pointsAvailableToSpawn.Remove(randomPoint);
			return randomPoint.position;
		}
	}
}