using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;
    public static CameraManager instance => _instance;

    [SerializeField] private CinemachineCamera[] _allVirtualCameras; 

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;
    
    private CinemachineCamera _currentCamera;
    private CinemachinePositionComposer _positionComposer;

    private Vector3 _startingTargetOffset;
    private Vector2 _startingDamping;
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        for(int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if(_allVirtualCameras[i].enabled)
            {
                _currentCamera = _allVirtualCameras[i];
                _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
            }
        }
        _startingTargetOffset = _positionComposer.TargetOffset;
        _startingDamping = _positionComposer.Damping;
    }

    #region Lerp the Y Damping

    public void LerpYDamping(bool isPlayerFalling)
    {
        // Dừng coroutine cũ nếu đang chạy
        if (_lerpYPanCoroutine != null)
        {
            StopCoroutine(_lerpYPanCoroutine);
            _lerpYPanCoroutine = null;
        }
        if (isPlayerFalling)
        {
            LerpedFromPlayerFalling = true;
        }
        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    public void ResetYDamping()
    {
        if (_lerpYPanCoroutine != null)
        {
            StopCoroutine(_lerpYPanCoroutine);
            _lerpYPanCoroutine = null;
        }
        if (_positionComposer != null)
            _positionComposer.Damping = _startingDamping;
        IsLerpingYDamping = false;
        LerpedFromPlayerFalling = false;
    }

    IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;
        
        Vector2 startDamping = _positionComposer.Damping;
        Vector2 endDamping = startDamping;
        endDamping.y = isPlayerFalling ? _fallPanAmount : _startingDamping.y;

        float elapsedTime = 0f;
        while(elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;
            Vector2 lerpDamping = Vector2.Lerp(startDamping, endDamping, elapsedTime / _fallYPanTime);
            _positionComposer.Damping = lerpDamping;
            yield return null;
        }
        IsLerpingYDamping = false;
    }

    #endregion
    #region Pan Camera


    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection,
    bool panToStartingPos)
    {
        if (_panCameraCoroutine != null)
        {
            StopCoroutine(_panCameraCoroutine);
            _panCameraCoroutine = null;
        }
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }
    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection,
    bool panToStartingPos)
    {
        Vector3 startOffset = _positionComposer.TargetOffset;
        Vector3 endOffset = _startingTargetOffset;

        if(!panToStartingPos)
        {
            switch(panDirection)
            {
                case PanDirection.Up:
                    endOffset.y += panDistance;
                    break;
                case PanDirection.Down:
                    endOffset.y -= panDistance;
                    break;
                case PanDirection.Left:
                    endOffset.x -= panDistance;
                    break;
                case PanDirection.Right:
                    endOffset.x += panDistance;
                    break;
                default:
                    break;
            }
        }

        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;
            Vector3 lerpOffset = Vector3.Lerp(startOffset, endOffset, elapsedTime / panTime);
            _positionComposer.TargetOffset = lerpOffset;
            yield return null;
        }
    }

    #endregion
    #region Swap Cameras

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight,
    Vector2 triggerExitDirection)
    {
        if(_currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            cameraFromRight.enabled = true;

            cameraFromLeft.enabled = false;

            _currentCamera = cameraFromRight;

            _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
        }
        else if(_currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            cameraFromLeft.enabled = true;

            cameraFromRight.enabled = false;

            _currentCamera = cameraFromLeft;

            _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
        }
            
    }
    #endregion
}
