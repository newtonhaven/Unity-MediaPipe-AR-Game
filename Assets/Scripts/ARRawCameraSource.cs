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
    private bool _isInitialized = false;

    private void Start()
    {
        if (!arCameraManager) arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCameraManager)
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
            _isInitialized = true;
        }
    }

    private void OnDestroy()
    {
        if (arCameraManager) arCameraManager.frameReceived -= OnCameraFrameReceived;
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
        return _outputTexture;
    }
}