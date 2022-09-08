using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View.UI
{
	public class EnterIpPanel : MonoBehaviour
	{
		[SerializeField]
		private TMP_InputField _inputField;
		[SerializeField]
		private Button _connectButton;

		private string _lastJoinedAddress = "localhost";

		private const string LastJoinedAddressPrefsName = "LastJoinedIp";

		public Action<string> ConnectButtonClick;

		private void Start()
		{
			UpdateLastJoinedAddress();
			_inputField.onValueChanged.AddListener(OnIpAddressChanged);
			_connectButton.onClick.AddListener(OnConnectButtonClick);
		}

		private void OnConnectButtonClick()
		{
			PlayerPrefs.SetString(LastJoinedAddressPrefsName, _lastJoinedAddress);
			PlayerPrefs.Save();

			ConnectButtonClick?.Invoke(_lastJoinedAddress);
		}

		private void OnIpAddressChanged(string address)
		{
			_lastJoinedAddress = address;
		}

		private void UpdateLastJoinedAddress()
		{
			if (PlayerPrefs.HasKey(LastJoinedAddressPrefsName))
			{
				_lastJoinedAddress = PlayerPrefs.GetString(LastJoinedAddressPrefsName);
			}
			_inputField.text = _lastJoinedAddress;
		}
	}
}