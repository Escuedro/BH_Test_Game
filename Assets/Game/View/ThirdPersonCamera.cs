using System.Collections.Generic;
using UnityEngine;

namespace Game.View
{
	public class ThirdPersonCamera : MonoBehaviour
	{
		[SerializeField]
		private float _distance = 5f;
		[SerializeField]
		private float _minY = 1f;
		[SerializeField]
		private float _maxY = 5f;
		[SerializeField]
		private float _xSpeed = 10f;
		[SerializeField]
		private float _ySpeed = 5f;
		[SerializeField]
		private Transform _transform;

		private float _currentXAngle;
		private float _currentYAngle;

		private Transform _target;

		public Vector3 CameraForward => _transform.forward;
		public Vector3 CameraRight => _transform.right;

		public static List<Transform> ToFaceTransforms = new List<Transform>();

		public void SetTarget(Transform target)
		{
			_target = target;
			_transform.position = target.position + new Vector3(0, 0, -_distance);
			_transform.parent = _target;
		}

		private void Update()
		{
			if (_target == null)
			{
				return;
			}

			float xChangeDelta = Input.GetAxis("Mouse X") * _xSpeed * Time.deltaTime;
			float yChangeDelta = Input.GetAxis("Mouse Y") * _ySpeed * Time.deltaTime;

			Vector3 targetPosition = _target.position;
			_transform.RotateAround(targetPosition, _target.up, xChangeDelta);
			_transform.RotateAround(targetPosition, _transform.right, yChangeDelta);

			Vector3 cameraPosition = _transform.position;
			Vector3 cameraOffset = (cameraPosition - targetPosition).normalized * _distance;

			Vector3 newCameraPosition = targetPosition + cameraOffset;
			newCameraPosition.y = Mathf.Clamp(newCameraPosition.y, _minY, _maxY);

			_transform.position = newCameraPosition;
			_transform.LookAt(_target);

			foreach (Transform faceTransform in ToFaceTransforms)
			{
				faceTransform.LookAt(transform);
			}
		}
	}
}