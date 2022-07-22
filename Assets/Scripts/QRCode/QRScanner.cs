using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRScanner : MonoBehaviour
{
    public QRDemonstrator demonstrator;

    private IBarcodeReader reader;
    private ARCameraManager arCamera;
    private Texture2D arCameraTexture;
    private bool doOnce;
    private string result;

    void Start()
    {
        arCamera = FindObjectOfType<ARCameraManager>();
        reader = new BarcodeReader();
        arCamera.frameReceived += OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if ((Time.frameCount % 15) == 0)
        {
            if (arCamera.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                StartCoroutine(ProcessQRCode(image));
                image.Dispose();
            }
        }
    }

    IEnumerator ProcessQRCode(XRCpuImage image)
    {
        var request = image.ConvertAsync(
            new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
                outputFormat = TextureFormat.RGB24,
                transformation = XRCpuImage.Transformation.MirrorY
            }
        );

        while (!request.status.IsDone())
            yield return null;

        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            request.Dispose();
            yield break;
        }

        var rawData = request.GetData<byte>();

        if (arCameraTexture == null)
        {
            arCameraTexture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false
            );
        }

        arCameraTexture.LoadRawTextureData(rawData);
        arCameraTexture.Apply();
        request.Dispose();

        byte[] barcodeBitmap = arCameraTexture.GetRawTextureData();

        LuminanceSource source = new RGBLuminanceSource(
            barcodeBitmap,
            arCameraTexture.width,
            arCameraTexture.height
        );

        if (!doOnce)
        {
            doOnce = true;
            result = reader.Decode(source).Text;

            demonstrator.Receive(result);
        }
        else
        {
            doOnce = false;
        }
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        // GUILayout.Label(" ", guiStyle);
    }
}
