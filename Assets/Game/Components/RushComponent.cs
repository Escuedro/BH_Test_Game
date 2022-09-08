using Mirror;
using UnityEngine;

namespace Game.Components
{
	public class RushComponent : NetworkBehaviour
	{
		[SerializeField]
		private Transform _transform;
		[SerializeField]
		private float _rushDistance = 2f;
		[SerializeField]
		private Rigidbody _rigidbody;
		
		private bool _inRush;
		public bool InRush => _inRush;

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				CmdAddImpulse();
			}
		}

		[Command]
		private void CmdAddImpulse()
		{
			_inRush = true;
			Vector3 velocity = _transform.forward * (_rushDistance / _rigidbody.mass);
			_rigidbody.AddForce(velocity, ForceMode.Impulse);
		}
	}
}