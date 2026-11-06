using UnityEngine;

namespace Fangbound.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Collider2D roomBounds;
        [SerializeField, Min(0f)] private Vector2 deadZoneSize = new Vector2(1.2f, 0.8f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;

        private Camera attachedCamera;
        private Vector3 smoothVelocity;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 desiredPosition = currentPosition;
            desiredPosition.x = GetDeadZoneTarget(currentPosition.x, target.position.x, deadZoneSize.x);
            desiredPosition.y = GetDeadZoneTarget(currentPosition.y, target.position.y, deadZoneSize.y);
            desiredPosition = ClampToRoom(desiredPosition);

            Vector3 smoothedPosition = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref smoothVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            Vector3 clampedPosition = ClampToRoom(smoothedPosition);
            if (!Mathf.Approximately(clampedPosition.x, smoothedPosition.x))
            {
                smoothVelocity.x = 0f;
            }

            if (!Mathf.Approximately(clampedPosition.y, smoothedPosition.y))
            {
                smoothVelocity.y = 0f;
            }

            transform.position = clampedPosition;
        }

        private static float GetDeadZoneTarget(float cameraPosition, float targetPosition, float deadZoneSize)
        {
            float halfDeadZone = deadZoneSize * 0.5f;
            float difference = targetPosition - cameraPosition;

            if (difference > halfDeadZone)
            {
                return targetPosition - halfDeadZone;
            }

            if (difference < -halfDeadZone)
            {
                return targetPosition + halfDeadZone;
            }

            return cameraPosition;
        }

        private Vector3 ClampToRoom(Vector3 position)
        {
            if (roomBounds == null || attachedCamera == null || !attachedCamera.orthographic)
            {
                return position;
            }

            Bounds bounds = roomBounds.bounds;
            float halfHeight = attachedCamera.orthographicSize;
            float halfWidth = halfHeight * attachedCamera.aspect;

            position.x = ClampAxis(position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth, bounds.center.x);
            position.y = ClampAxis(position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight, bounds.center.y);
            return position;
        }

        private static float ClampAxis(float value, float minimum, float maximum, float center)
        {
            return minimum > maximum ? center : Mathf.Clamp(value, minimum, maximum);
        }
    }
}