using UnityEngine;
using System.Collections.Generic;
using Mediapipe;

public class HandColliderProxy : MonoBehaviour
{
    [Header("Settings")]
    public Transform physicsFinger;
    public Camera mainCamera;
    public float interactionDepth = 0.5f;

    // Internal State
    private Rigidbody _fingerRigidbody;

    void Start()
    {
        if (physicsFinger)
        {
            _fingerRigidbody = physicsFinger.GetComponent<Rigidbody>();
            
            // ENSURE COLLIDER IS ALWAYS ON
            Collider col = physicsFinger.GetComponent<Collider>();
            if (col) col.enabled = true; 
        }
    }

    public void OnHandLandmarksOutput(List<NormalizedLandmarkList> result)
    {
        // 1. Safety Checks
        if (result == null || result.Count == 0) return;
        NormalizedLandmarkList handContainer = result[0];
        if (handContainer.Landmark == null) return;

        // 2. Get Index Finger Tip (ID 8)
        int indexTipId = 8;
        if (handContainer.Landmark.Count <= indexTipId) return;

        // 3. Extract Coordinates
        // Note:flip Y (1.0 - y) because Unity UI starts at bottom-left, MediaPipe at top-left
        float x = handContainer.Landmark[indexTipId].X;
        float y = 1.0f - handContainer.Landmark[indexTipId].Y;

        // 4. Move the Physics Finger
        // ViewportToWorldPoint converts (0-1) screen coordinates to 3D world space
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(x, y, interactionDepth));

        if (physicsFinger != null)
        {
            // Use Rigidbody move if available for smoother physics collision
            if (_fingerRigidbody)
                _fingerRigidbody.MovePosition(worldPos);
            else
                physicsFinger.position = worldPos;
        }
    }
}