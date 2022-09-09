using System;
using Game.Network;
using Game.Utils;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View.UI
{
	public class PlayerRoomView : NetworkBehaviour
	{
		[SerializeField]
		private GameObject _ui;
		[SerializeField]
		private TextMeshProUGUI[] _displayNames;
		[SerializeField]
		private TextMeshProUGUI[] _readyTexts;
		[SerializeField]
		private Button _readyButton;
		[SerializeField]
		private Button _startGameButton;

		[SyncVar(hook = nameof(OnDisplayNameChanged))]
		public string DisplayName = "Loading...";
		[SyncVar(hook = nameof(OnReadyStatusChanged))]
		public bool IsReady = false;

		private bool _isMaster;

		public bool IsMaster
		{
			set
			{
				_isMaster = value;
				_startGameButton.gameObject.SetActive(value);
			}
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

		public override void OnStartAuthority()
		{
			_ui.SetActive(true);
			SetDisplayName(PlayerPrefs.GetString(PlayerPrefsVariables.PlayerName));
		}

		[Command]
		private void SetDisplayName(string displayName)
		{
			DisplayName = displayName;
		}

		public override void OnStartClient()
		{
			Room.RoomPlayers.Add(this);
			UpdateViews();
			Room.UpdateReadyToStartState();
		}

		public override void OnStopClient()
		{
			Room.RoomPlayers.Remove(this);
			UpdateViews();
		}

		private void Start()
		{
			_readyButton.onClick.AddListener(() => UpdateIsReady(!IsReady));
		}

		private void OnDisplayNameChanged(string oldName, string newName)
		{
			UpdateViews();
		}

		private void UpdateViews()
		{
			if (!hasAuthority)
			{
				foreach (var player in Room.RoomPlayers)
				{
					if (player.hasAuthority)
					{
						player.UpdateViews();
						break;
					}
				}

				return;
			}

			for (int i = 0; i < _displayNames.Length; i++)
			{
				_displayNames[i].text = "Waiting For Player...";
				_readyTexts[i].text = string.Empty;
			}

			for (int i = 0; i < Room.RoomPlayers.Count; i++)
			{
				_displayNames[i].text = Room.RoomPlayers[i].DisplayName;
				_readyTexts[i].text = Room.RoomPlayers[i].IsReady ?
						"<color=green>Ready</color>" :
						"<color=red>Not Ready</color>";
			}
		}

		private void OnReadyStatusChanged(bool oldValue, bool newValue)
		{
			UpdateViews();
		}

		public void UpdateReadyToStart(bool isReadyToStart)
		{
			if (!_isMaster)
			{
				return;
			}
			_startGameButton.interactable = isReadyToStart;
		}

		[Command]
		private void UpdateIsReady(bool isReady)
		{
			IsReady = isReady;

			Room.UpdateReadyToStartState();
		}
	}
}