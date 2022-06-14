using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AddQrExample : MonoBehaviour
{
    // Acts as the Virtual Content -> could have multiple objects and scripts attached
    public GameObject contentPrefab;
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
            if (_imageTargetManager && contentPrefab)
            {
                _imageTargetManager.AddVirtualContentToImage(texture, contentPrefab, 0.05f);
            }
        }

        _isDone = true;
    }
}