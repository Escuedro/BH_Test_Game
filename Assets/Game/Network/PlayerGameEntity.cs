using Mirror;

namespace Game.Network
{
	public class PlayerGameEntity : NetworkBehaviour
	{
		[SyncVar]
		private string _displayName = "Loading...";
		[SyncVar]
		public int Points;

		public string DisplayName => _displayName;

		[Server]
		public void SetDisplayName(string displayName)
		{
			_displayName = displayName;
		}

		private LobbyNetworkManager _room;

		private LobbyNetworkManager Room
		{
			get
			{
				if (_room != null)
				{
					return _room;
				}
				return _room = NetworkManager.singleton as LobbyNetworkManager;
			}
		}

		public override void OnStartClient()
		{
			DontDestroyOnLoad(this);

			Room.GamePlayers.Add(this);
			Room.UpdateReadyToStartState();
		}

		public override void OnStopClient()
		{
			Room.GamePlayers.Remove(this);
		}
	}
}