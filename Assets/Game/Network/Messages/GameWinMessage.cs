using Mirror;

namespace Game.Network.Messages
{
	public struct GameWinMessage : NetworkMessage
	{
		public string WinnerName;
	}
}