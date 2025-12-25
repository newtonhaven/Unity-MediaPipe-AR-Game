using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandLandmarkerResultAnnotationController : MonoBehaviour
    {
        [Header("Visualization Settings")]
        // FIX: We explicitly use UnityEngine.Color to avoid the CS0117 error
        public UnityEngine.Color skeletonColor = UnityEngine.Color.red;
        
        [Range(0.001f, 0.02f)]
        public float lineWidth = 0.005f;

        [SerializeField] private Camera _mainCamera;

        // We use 5 LineRenderers (one for each finger)
        private LineRenderer[] _fingerLines;
        
        // Connections: 0=Wrist, 1-4=Thumb, 5-8=Index, etc.
        private readonly int[][] _fingerConnections = new int[][]
        {
            new int[] { 0, 1, 2, 3, 4 },       // Thumb
            new int[] { 0, 5, 6, 7, 8 },       // Index
            new int[] { 0, 9, 10, 11, 12 },    // Middle
            new int[] { 0, 13, 14, 15, 16 },   // Ring
            new int[] { 0, 17, 18, 19, 20 }    // Pinky
        };

        private void Awake()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            InitializeLineRenderers();
        }

        private void InitializeLineRenderers()
        {
            _fingerLines = new LineRenderer[5];
            for (int i = 0; i < 5; i++)
            {
                // Create a generic Unity LineRenderer for each finger
                GameObject go = new GameObject($"FingerLine_{i}");
                go.transform.SetParent(transform, false);
                
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                
                // Use a default Unity material so it is visible
                lr.material = new Material(Shader.Find("Sprites/Default"));
                
                lr.startColor = skeletonColor;
                lr.endColor = skeletonColor;
                
                lr.positionCount = 5; 
                lr.enabled = false;   
                
                _fingerLines[i] = lr;
            }
        }

        public void DrawLater(HandLandmarkerResult result)
        {
            // FIX: Check 'handedness' list to avoid CS0019 (Struct null check error)
            if (result.handedness == null || result.handedness.Count == 0)
            {
                ClearLines();
                return;
            }

            // We visualize the first hand [0]
            var landmarks = result.handLandmarks[0].landmarks;

            for (int i = 0; i < 5; i++)
            {
                var lineRenderer = _fingerLines[i];
                lineRenderer.enabled = true;

                for (int j = 0; j < 5; j++)
                {
                    int landmarkIndex = _fingerConnections[i][j];
                    var point = landmarks[landmarkIndex];

                    // FIX: Use UnityEngine.Screen to avoid CS0117
                    float screenX = (1 - point.x) * UnityEngine.Screen.width;
                    float screenY = (1 - point.y) * UnityEngine.Screen.height;
                    
                    // Map to World Space at 0.5m depth (same as Physics Finger)
                    Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenX, screenY, 0.5f));
                    
                    lineRenderer.SetPosition(j, worldPos);
                }
            }
        }

        public void ClearLines()
        {
            if (_fingerLines == null) return;
            foreach (var line in _fingerLines)
            {
                if(line != null) line.enabled = false;
            }
        }
        
        // This keeps the Runner script happy
        public bool isVisualizationEnabled { get; set; } = true;
    }
}