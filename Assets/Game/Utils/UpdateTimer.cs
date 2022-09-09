using System;
using UnityEngine;

namespace Game.Utils
{
	public class UpdateTimer : MonoBehaviour
	{
		private bool _isStarted;
		private float _timer;
		private Action _action;

		public void Init(Action onTimerEnds, float duration)
		{
			_timer = duration;
			_action = onTimerEnds;
			_isStarted = true;
		}

		private void Update()
		{
			if (_isStarted)
			{
				if (_timer <= 0f)
				{
					_action?.Invoke();
					Destroy(gameObject);
				}
				_timer -= Time.deltaTime;
			}
		}
	}
}