using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class ARRawCameraSource : MonoBehaviour
{
    [Header("AR Setup")]
    public ARCameraManager arCameraManager;

    private Texture2D _outputTexture;
    private Texture2D _outputTexture;
    private bool _isInitialized = false;

#if UNITY_EDITOR
    [Header("Editor Options")]
    public int webcamIndex = 0;
    private WebCamTexture _webCamTexture;
#endif

    private void Start()
    {
#if UNITY_EDITOR
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            int index = Mathf.Clamp(webcamIndex, 0, devices.Length - 1);
            _webCamTexture = new WebCamTexture(devices[index].name);
            _webCamTexture.Play();
        }
        else
        {
            Debug.LogWarning("No webcam found for Editor testing.");
        }
#else
        if (!arCameraManager) arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCameraManager)
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
            _isInitialized = true;
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (_webCamTexture != null && _webCamTexture.isPlaying) _webCamTexture.Stop();
#else
        if (arCameraManager) arCameraManager.frameReceived -= OnCameraFrameReceived;
#endif
        if (_outputTexture) Destroy(_outputTexture);
    }

    private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        if (_outputTexture == null || _outputTexture.width != image.width || _outputTexture.height != image.height)
        {
            _outputTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        }

        int size = image.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
        _outputTexture.LoadRawTextureData(buffer);
        _outputTexture.Apply();

        image.Dispose();
        buffer.Dispose();
    }

    // Public method for the Runner to grab the latest frame
    public Texture GetCurrentTexture()
    {
#if UNITY_EDITOR
        return _webCamTexture;
#else
        return _outputTexture;
#endif
    }
}