using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

public class QREncoder
{
    public Texture2D Encode(string text){
        var encoded = new Texture2D(256, 256);
        var writer = new BarcodeWriter{
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions {
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
                Height = encoded.height,
                Width = encoded.width
            }
        };
        Color32[] colors = writer.Write(text);
        encoded.SetPixels32(colors);
        encoded.Apply();
        return encoded;
    }
}
