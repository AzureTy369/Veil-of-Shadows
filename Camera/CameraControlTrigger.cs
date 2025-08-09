using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Cinemachine;


public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObject customInspectorObjects;

    private Collider2D _coll;

    void Start()
    {
        _coll = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance,
                customInspectorObjects.panTime, customInspectorObjects.panDirection, false);

            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Vector2 exitDirection = (other.transform.position - _coll.bounds.center).normalized;

            if(customInspectorObjects.swapCameras && customInspectorObjects.cameraOnLeft != null &&
            customInspectorObjects.cameraOnRight != null)
            {
                CameraManager.instance.SwapCamera(customInspectorObjects.cameraOnLeft,
                customInspectorObjects.cameraOnRight, exitDirection);
            }

            if(customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance,
                customInspectorObjects.panTime, customInspectorObjects.panDirection, true);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObject 
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineCamera cameraOnLeft;
    [HideInInspector] public CinemachineCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;

    void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(cameraControlTrigger.customInspectorObjects.swapCameras)
        {
            cameraControlTrigger.customInspectorObjects.cameraOnLeft = 
            EditorGUILayout.ObjectField("Camera On Left", 
            cameraControlTrigger.customInspectorObjects.cameraOnLeft, 
            typeof(CinemachineCamera), true) as CinemachineCamera;

            cameraControlTrigger.customInspectorObjects.cameraOnRight = 
            EditorGUILayout.ObjectField("Camera On Right", 
            cameraControlTrigger.customInspectorObjects.cameraOnRight, typeof(CinemachineCamera), true) as CinemachineCamera;
        }

        if(cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.
            EnumPopup("Pan Direction", cameraControlTrigger.customInspectorObjects.panDirection);
        
            cameraControlTrigger.customInspectorObjects.panDistance = 
            EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspectorObjects.panDistance);

            cameraControlTrigger.customInspectorObjects.panTime = 
            EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObjects.panTime);
        }

        if(GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif
