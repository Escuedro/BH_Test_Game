using System;
using Game.View;
using Mirror;
using TMPro;
using UnityEngine;

namespace Game.Network
{
	public class PlayerNetworkEntity : NetworkBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _nameText;
		[SerializeField]
		private TextMeshProUGUI _pointsText;
		[SerializeField]
		private Rigidbody _rigidbody;
		[SerializeField]
		private float _speed = 5f;
		[SerializeField]
		private ThirdPersonCamera _cameraPrefab;
		[SerializeField]
		private float _rushDistance = 2f;
		[SerializeField]
		private float _rushForce = 1f;
		[SerializeField]
		private AnimationCurve _rushCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField]
		private float _maxRushDuration = 1f;
		[SerializeField]
		private float _immuneStateDuration = 2f;
		[SerializeField]
		private MeshRenderer _meshRenderer;
		[SerializeField]
		private Material _normalMaterial;
		[SerializeField]
		private Material _damagedMaterial;

		[SyncVar(hook = nameof(OnDisplayNameChanged))]
		private string _displayName;

		[SyncVar]
		private bool _canMove = true;

		[SyncVar(hook = nameof(OnImmuneChanged))]
		private bool IsImmune;
		[SyncVar]
		public bool InRush;
		[SyncVar(hook = nameof(OnPointsChanged))]
		private int Points;

		private ThirdPersonCamera _camera;
		private Vector3 _startRushPosition;
		private Vector3 _positionToRush;

		private float _rushTimer;
		private float _immuneTimer;

		private void Start()
		{
			ThirdPersonCamera.ToFaceTransforms.Add(_nameText.transform);
			ThirdPersonCamera.ToFaceTransforms.Add(_pointsText.transform);
		}

		private void OnDestroy()
		{
			ThirdPersonCamera.ToFaceTransforms.Remove(_nameText.transform);
			ThirdPersonCamera.ToFaceTransforms.Remove(_pointsText.transform);
		}

		public override void OnStartAuthority()
		{
			_camera = Instantiate(_cameraPrefab);
			_camera.SetTarget(transform);
		}

		[ServerCallback]
		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.TryGetComponent(out PlayerNetworkEntity player))
			{
				if (player.InRush && !IsImmune)
				{
					Hit();
					player.IncreasePoints();
				}
			}
		}

		[Server]
		private void IncreasePoints()
		{
			Points++;
			NetworkClient.Send(new IncreasePointMessage());
		}

		private void Update()
		{
			if (hasAuthority)
			{
				Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

				if (inputDirection.sqrMagnitude > 0f)
				{
					CmdMove(inputDirection, _camera.CameraForward, _camera.CameraRight);
				}
				if (Input.GetMouseButtonDown(0) && !InRush)
				{
					CmdRush(_camera.CameraForward);
				}
				if (InRush)
				{
					CmdRushMovement(_camera.CameraForward);
				}
				if (IsImmune)
				{
					CmdHandleImmune();
				}
			}
		}

		[Command]
		private void CmdHandleImmune()
		{
			_immuneTimer -= Time.deltaTime;
			if (_immuneTimer <= 0f)
			{
				IsImmune = false;
			}
		}

		[Command]
		private void CmdRushMovement(Vector3 forward)
		{
			if (_rushTimer >= _maxRushDuration || Vector3.Distance(_startRushPosition, transform.position) >= _rushDistance)
			{
				InRush = false;
				_canMove = true;
				StopMovement();
				_rigidbody.useGravity = true;
				return;
			}
			forward.y = 0f;
			RpcRushMovement();
			_rushTimer += Time.deltaTime;
		}

		[ClientRpc]
		private void StopMovement()
		{
			_rigidbody.velocity = Vector3.zero;
		}

		[ClientRpc]
		private void RpcRushMovement()
		{
			_rigidbody.velocity = (_positionToRush - _startRushPosition) / _maxRushDuration;
		}

		[Command]
		private void CmdRush(Vector3 forwardVector)
		{
			if (!InRush && _canMove)
			{
				_rigidbody.useGravity = false;
				InRush = true;
				_canMove = false;
				forwardVector.y = 0;
				_startRushPosition = transform.position;
				_positionToRush = _startRushPosition + forwardVector * _rushDistance;
				_rushTimer = 0f;
				RpcRush(forwardVector.normalized);
			}
		}

		[ClientRpc]
		private void RpcRush(Vector3 direction)
		{
			_rigidbody.AddForce(direction * _rushForce);
		}

		[Command]
		private void CmdMove(Vector2 inputDirection, Vector3 forwardVector, Vector3 rightVector)
		{
			if (_canMove)
			{
				Vector3 forwardRelativeMovement = inputDirection.y * forwardVector;
				Vector3 rightRelativeMovement = inputDirection.x * rightVector;
				Vector3 direction = forwardRelativeMovement + rightRelativeMovement;
				direction.y = 0;
				RpcMove(direction * _speed);
			}
		}

		[ClientRpc]
		private void RpcMove(Vector3 velocity)
		{
			_rigidbody.velocity = velocity;
		}

		private void OnDisplayNameChanged(string oldValue, string newValue)
		{
			_nameText.text = newValue;
		}

		public void SetDisplayName(string displayName)
		{
			_displayName = displayName;
		}

		[Server]
		private void Hit()
		{
			_immuneTimer = _immuneStateDuration;
			IsImmune = true;
		}

		private void OnImmuneChanged(bool oldValue, bool newValue)
		{
			_meshRenderer.material = newValue ? _damagedMaterial : _normalMaterial;
		}

		private void OnPointsChanged(int oldValue, int newValue)
		{
			_pointsText.text = newValue.ToString();
		}
	}
}