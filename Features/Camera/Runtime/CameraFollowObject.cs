using UnityEngine;
using System.Collections;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;

    private PlayerMovement _player;

    private void Awake()
    {
        if (_playerTransform != null)
        {
            _player = _playerTransform.GetComponent<PlayerMovement>();
        }
    }

    private void OnEnable()
    {
        if (_player == null && _playerTransform != null)
        {
            _player = _playerTransform.GetComponent<PlayerMovement>();
        }
        if (_player != null)
        {
            _player.OnFacingChanged += HandleFacingChanged;
        }
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnFacingChanged -= HandleFacingChanged;
        }
    }

    private void Update()
    {
        if (_playerTransform != null)
        {
            transform.position = _playerTransform.position;
        }
    }

    private void HandleFacingChanged(bool isFacingRight)
    {
        StartFlip(isFacingRight);
    }

    private void StartFlip(bool isFacingRight)
    {
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }
        _turnCoroutine = StartCoroutine(FlipYLerp(isFacingRight));
    }

    private IEnumerator FlipYLerp(bool isFacingRight)
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = isFacingRight ? 0f : 180f;

        float elapsedTime = 0f;
        while (elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotationAmount, elapsedTime / _flipYRotationTime);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0f, endRotationAmount, 0f);
    }
}
