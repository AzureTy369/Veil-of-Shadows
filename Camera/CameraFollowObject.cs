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

    private bool _isFacingRight;

    void Awake()
    {
        _player = _playerTransform.GetComponent<PlayerMovement>();
        _isFacingRight = _player.IsFacingRight;
    }

    void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void CallTurn(){
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp(){
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmout = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while(elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;

            // lerp the y rotation
            yRotation = Mathf.Lerp(startRotation, endRotationAmout, elapsedTime / _flipYRotationTime);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }  
    }

    private float DetermineEndRotation(){
        _isFacingRight = !_isFacingRight;

        if(_isFacingRight)
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }
}
