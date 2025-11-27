using UnityEngine;

public class BillboardAndOscillation : MonoBehaviour
{
    [Tooltip("카메라를 바라보게 할 대상; 없을경우, 메인 카메라.")]
    public Transform targetCamera;

    [Header("좌우 흔들림 설정")]
    [Tooltip("좌우 흔들림의 폭(월드 좌표계)")]
    public float oscillationRange = 0.5f;
    
    [Tooltip("왕복 운동의 주기(초).")]
    public float oscillationPeriod = 20f; // 20초 주기

    private Vector3 initialPosition;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }

        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        // if (targetCamera != null)
        // {
        //     Vector3 lookDirection = transform.position - targetCamera.position;
        //     lookDirection.y = 0;

        //     transform.rotation = Quaternion.LookRotation(lookDirection);
        // }

        float timeFactor = Time.time / oscillationPeriod;
        float horizontalOffset = Mathf.Sin(timeFactor * 2f * Mathf.PI) * oscillationRange;

        Vector3 newPosition = initialPosition;
        newPosition.x += horizontalOffset;
        
        transform.position = newPosition;
    }
}