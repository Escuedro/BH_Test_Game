using Mirror;

namespace Game.Network
{
	public struct GameWinMessage : NetworkMessage
	{
		public string WinnerName;
	}
}