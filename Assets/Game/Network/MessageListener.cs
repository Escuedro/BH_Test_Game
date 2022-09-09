using Mirror;
using TMPro;
using UnityEngine;

namespace Game.Network
{
	public class MessageListener : MonoBehaviour
	{
		[SerializeField]
		private GameObject _winnerCanvas;
		[SerializeField]
		private TextMeshProUGUI _winnerNameText;

		private void Start()
		{
			NetworkClient.RegisterHandler<GameWinMessage>(OnGameWin);
		}

		private void OnGameWin(GameWinMessage gameWinMessage)
		{
			_winnerNameText.text = gameWinMessage.WinnerName;
			_winnerCanvas.gameObject.SetActive(true);
		}
	}
}