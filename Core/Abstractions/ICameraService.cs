using UnityEngine;
using Unity.Cinemachine;

public interface ICameraService
{
    bool IsLerpingYDamping { get; }
    bool LerpedFromPlayerFalling { get; set; }
    float FallSpeedYDampingChangeThreshold { get; }

    void LerpYDamping(bool isPlayerFalling);
    void ResetYDamping();

    void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos);

    void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection);
} 