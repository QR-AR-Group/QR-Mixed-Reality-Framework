using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AddQrExample : MonoBehaviour
{
    private ImageTargetManager _imageTargetManager;
    private bool _isDone;

    private void Start()
    {
        ARSession.stateChanged += ARSessionStateChanged;
    }

    private void ARSessionStateChanged(ARSessionStateChangedEventArgs session)
    {
        if (session.state == ARSessionState.SessionTracking && !_isDone)
        {
            
            CreateImageTargetTest();
        }
    }

    private void CreateImageTargetTest()
    {
        Texture2D texture = Resources.Load("QR-example") as Texture2D;
        if (texture)
        {
            _imageTargetManager = FindObjectOfType<ImageTargetManager>();
            if (_imageTargetManager)
            {
                StartCoroutine(_imageTargetManager.AddImage(texture, 0.05f));
            }
        }
        _isDone = true;
    }
}