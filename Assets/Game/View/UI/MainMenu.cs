using Game.Network;
using Game.Network.Messages;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View.UI
{
	public class MainMenu : MonoBehaviour
	{
		[SerializeField]
		private LobbyNetworkManager _lobbyNetworkManager;
		[SerializeField]
		private Button _startAsHostButton;
		[SerializeField]
		private Button _joinLobbyButton;
		[SerializeField]
		private EnterNamePanel _enterNamePanel;
		[SerializeField]
		private EnterIpPanel _enterIpPanel;
		[SerializeField]
		private GameObject _hostOrJoinPanel;

		private void Start()
		{
			_startAsHostButton.onClick.AddListener(OnStartHostButtonClick);
			_joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClick);
			_enterIpPanel.ConnectButtonClick += OnConnect;
			_enterNamePanel.OnNameConfirmClick += OnNameConfirmed;
		}

		private void OnNameConfirmed(string playerName)
		{
			_enterNamePanel.gameObject.SetActive(false);
			_hostOrJoinPanel.SetActive(true);
		}

		private void OnConnect(string ip)
		{
			_lobbyNetworkManager.networkAddress = ip;
			_lobbyNetworkManager.StartClient();

			_enterIpPanel.gameObject.SetActive(false);
			_hostOrJoinPanel.gameObject.SetActive(false);
		}

		private void OnJoinLobbyButtonClick()
		{
			_enterIpPanel.gameObject.SetActive(true);
		}

		private void OnStartHostButtonClick()
		{
			_lobbyNetworkManager.StartHost();

			_hostOrJoinPanel.gameObject.SetActive(false);
		}
	}
}