using Mirror;
using UnityEngine;

namespace Game.Network
{
	public class PlayerNetworkEntity : NetworkBehaviour
	{
		[ServerCallback]
		private void OnCollisionEnter(Collision collision)
		{
			
		}
	}
}
