using Mirror;

namespace Game.Network.Messages
{
	public struct PlayerChangeNameMessage : NetworkMessage
	{
		public string PlayerName;
	}
}