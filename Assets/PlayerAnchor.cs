using UnityEngine;

[DefaultExecutionOrder(500)]
[DisallowMultipleComponent]
public class FollowCameraAnchor : MonoBehaviour
{
    [Tooltip("Leave empty to auto-use Camera.main")]
    public Transform cameraTransform;

    [Header("Follow")]
    public bool copyRotation = false;
    public float smoothing = 20f;
    public float extraYOffset = 0f;

    void Reset() { gameObject.tag = "PlayerAnchor"; }  // new tag for this anchor

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            var cam = Camera.main;
            if (!cam) return;
            cameraTransform = cam.transform;
        }

        var targetPos = cameraTransform.position + Vector3.up * extraYOffset;
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPos, t);

        if (copyRotation) transform.rotation = cameraTransform.rotation;
    }
}
