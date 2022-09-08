using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View.UI
{
	public class EnterNamePanel : MonoBehaviour
	{
		[SerializeField]
		private TMP_InputField _inputField;
		[SerializeField]
		private Button _confirmButton;

		private const string PlayerNameKey = "PlayerName";
		private string _playerName = "";

		public string PlayerName => _playerName;

		public Action<string> OnNameConfirmClick;

		private void Start()
		{
			UpdatePlayerName();
			_inputField.onValueChanged.AddListener(OnNameChanged);
			_confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		}

		private void OnConfirmButtonClicked()
		{
			PlayerPrefs.SetString(PlayerNameKey, _playerName);
			PlayerPrefs.Save();

			OnNameConfirmClick?.Invoke(_playerName);
		}

		private void UpdatePlayerName()
		{
			if (PlayerPrefs.HasKey(PlayerNameKey))
			{
				_playerName = PlayerPrefs.GetString(PlayerNameKey);
			}
			_inputField.text = _playerName;
			OnNameChanged(_playerName);
		}

		private void OnNameChanged(string playerName)
		{
			_playerName = playerName;
			_confirmButton.interactable = !string.IsNullOrEmpty(_playerName);
		}
	}
}