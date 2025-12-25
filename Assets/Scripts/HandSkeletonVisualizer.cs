using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker; // NEW Namespace

public class HandSkeletonVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;
    public float interactionDepth = 0.5f;
    public GameObject jointPrefab;
    
    // Internal State
    private List<GameObject> _jointInstances = new List<GameObject>();
    
    // Indices to show: 
    // Index Finger: 5, 6, 7 (Skipping 8)
    // Thumb: 2, 3, 4
    // Middle: 9, 10, 12
    // Ring: 13, 14, 16
    // Pinky: 17, 18, 20
    private readonly List<int> _targetLandmarkIndices = new List<int> { 5, 6, 7, 2, 3, 4, 9, 10, 12, 13, 14, 16, 17, 18, 20 };

    // CHANGED: Now accepts HandLandmarkerResult from the Runner
    public void OnHandLandmarksOutput(HandLandmarkerResult result)
    {
        // 1. Safety Checks
        // 1. Safety Checks
        if (result.handLandmarks == null || result.handLandmarks.Count == 0) return;
        
        // In Tasks API, handLandmarks is List<NormalizedLandmarks> (plural wrapper)
        // Accessing the first hand
        var landmarks = result.handLandmarks[0].landmarks; // List of NormalizedLandmark

        if (landmarks == null) return;

        // 2. Update Visualization
        UpdateSkeleton(landmarks);
    }

    // Changing internal signature to match the data type
    private void UpdateSkeleton(List<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> landmarks)
    {
        // 1. Ensure instances exist
        if (_jointInstances.Count != _targetLandmarkIndices.Count)
        {
            // Clear old if any mismatch
            foreach (var existing in _jointInstances) if (existing) Destroy(existing);
            _jointInstances.Clear();

            for (int i = 0; i < _targetLandmarkIndices.Count; i++)
            {
                GameObject joint;
                if (jointPrefab != null)
                {
                    joint = Instantiate(jointPrefab, transform);
                    // Use prefab's own scale
                }
                else
                {
                    // Create default sphere
                    joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    joint.transform.SetParent(transform);
                    joint.transform.localScale = Vector3.one * 0.03f; 
                    
                    // Remove/Disable collider
                    var c = joint.GetComponent<Collider>();
                    if (c) Destroy(c); 
                }
                joint.name = $"SkeletonJoint_{_targetLandmarkIndices[i]}";
                _jointInstances.Add(joint);
            }
        }

        // 2. Update positions
        for (int i = 0; i < _targetLandmarkIndices.Count; i++)
        {
            int landmarkId = _targetLandmarkIndices[i];
            if (landmarkId < landmarks.Count)
            {
                var lm = landmarks[landmarkId];
                // Tasks API uses lowercase x, y
                float lx = 1.0f - lm.x; // Flip X (Mirror fix)
                float ly = 1.0f - lm.y; // Flip Y as before
                
                Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(lx, ly, interactionDepth));
                _jointInstances[i].transform.position = worldPos;
                
                if (!_jointInstances[i].activeSelf) _jointInstances[i].SetActive(true);
            }
            else
            {
                if (_jointInstances[i].activeSelf) _jointInstances[i].SetActive(false);
            }
        }
    }
}
