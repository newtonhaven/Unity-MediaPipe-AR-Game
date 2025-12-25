using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
    {
        [Header("AR Setup")]
        public ARRawCameraSource arSource;
        public Camera mainARCamera; 
        public Transform physicsFinger; 

        [Header("Depth Calibration")]
        public float depthMultiplier = 0.5f;
        public float minDepth = 0.3f;
        public float maxDepth = 2.0f; 

        [Header("Events")]
        public UnityEvent<HandLandmarkerResult> onHandLandmarksOutput = new UnityEvent<HandLandmarkerResult>();

        [Header("Visualization")]

        [SerializeField] private HandLandmarkerResultAnnotationController _annotationController;

        private Experimental.TextureFramePool _textureFramePool;
        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            MainThreadDispatcher.Instance();
            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
            taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            if (arSource == null) arSource = FindObjectOfType<ARRawCameraSource>();

            while (arSource == null || arSource.GetCurrentTexture() == null)
            {
                Debug.Log("Waiting for AR Source...");
                yield return null;
            }

            var currentTexture = arSource.GetCurrentTexture();
            _textureFramePool = new Experimental.TextureFramePool(currentTexture.width, currentTexture.height, TextureFormat.RGBA32, 10);

            if (_annotationController != null) 
            {
                _annotationController.isVisualizationEnabled = true;
            }

            // ROTATION: -90 usually works best for Android Portrait
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

            var waitForEndOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                if (arSource.GetCurrentTexture() == null) { yield return null; continue; }
                if (isPaused) yield return new WaitWhile(() => isPaused);

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                textureFrame.ReadTextureOnCPU(arSource.GetCurrentTexture(), false, false);
                var image = textureFrame.BuildCPUImage();
                textureFrame.Release();

                taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                yield return waitForEndOfFrame;
            }
        }

        private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
        {
            // 1. Move Physics Finger
            if (result.handedness != null && result.handedness.Count > 0)
            {
                var landmarks = result.handLandmarks[0].landmarks;
                var indexTip = landmarks[8]; 
                var wrist = landmarks[0];
                var middleMcp = landmarks[9];

                float screenX = (1 - indexTip.x) * UnityEngine.Screen.width;
                float screenY = (1 - indexTip.y) * UnityEngine.Screen.height;

                // DEPTH ESTIMATION (Scale-based)
                // Calculate 2D distance between Wrist (0) and Middle Finger MCP (9)
                // The closer the hand, the larger this distance.
                float apparentSize = Vector2.Distance(new Vector2(wrist.x, wrist.y), new Vector2(middleMcp.x, middleMcp.y));
                
                // Avoid divide by zero
                if (apparentSize < 0.001f) apparentSize = 0.001f;

                // Calculate depth: proportional to 1/size
                float estimatedDepth = depthMultiplier / apparentSize;
                
                // Clamp to reasonable range
                estimatedDepth = Mathf.Clamp(estimatedDepth, minDepth, maxDepth);

                if (physicsFinger != null && mainARCamera != null)
                {
                    MainThreadDispatcher.Instance().Enqueue(() => 
                    {
                        Vector3 worldPos = mainARCamera.ScreenToWorldPoint(new Vector3(screenX, screenY, estimatedDepth));
                        physicsFinger.position = worldPos;
                    });
                }
            }

            // 2. Draw Skeleton Lines
            if (_annotationController != null)
            {
                MainThreadDispatcher.Instance().Enqueue(() => 
                {
                    _annotationController.DrawLater(result);
                    onHandLandmarksOutput?.Invoke(result);
                });
            }
        }
    }
}