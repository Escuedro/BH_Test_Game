using UnityEngine;

namespace Game.Network
{
	public class PlayerSpawnPoint : MonoBehaviour
	{
		private void Awake()
		{
			PlayerSpawner.AddSpawnPoint(transform);
		}

		private void OnDestroy()
		{
			PlayerSpawner.RemoveSpawnPoint(transform);
		}
	}
}