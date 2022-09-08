using System;
using Mirror;

namespace Game.Network
{
	public class RoomPlayerEntity : NetworkBehaviour
	{
		[SyncVar(hook = nameof(OnNameChanged))]
		public string DispayName = "Loading...";
		[SyncVar(hook = nameof(OnReadyChanged))]
		public bool IsReady;
		
		private Action<RoomPlayerEntity> _onCreate;
		private Action<RoomPlayerEntity> _onDestroy;

		public bool IsMaster { get; private set; }

		public void Init(string displayName, Action<RoomPlayerEntity> onCreate, Action<RoomPlayerEntity> onDestroy)
		{
			DispayName = displayName;
			_onCreate = onCreate;
			_onDestroy = onDestroy;
		}

		public override void OnStartAuthority()
		{
			base.OnStartAuthority();
			
		}

		public override void OnStopClient()
		{
			_onDestroy?.Invoke(this);
		}

		public override void OnStartClient()
		{
			_onCreate?.Invoke(this);
		}

		[Server]
		public void SetIsMaster(bool isRoomMaster)
		{
			IsMaster = isRoomMaster;
		}

		private void OnNameChanged(string oldName, string newName)
		{
		}

		private void OnReadyChanged(bool oldIsReady, bool newIsReady)
		{
		}

		public void UpdateReadyState(bool isReadyToStart)
		{
			IsReady = isReadyToStart;
		}
	}
}