using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ImageTracking
{
    public class AddQrExample : MonoBehaviour
    {
        public VirtualContent contentPrefab;
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
            // The content should spawn 25cm away from the image
            Vector3 offset = new Vector3(-0.25f, 0f, 0f);
            
            // The content should have these dimensions (in cm)
            float width = 0.2f;
            float height = 0.1f;
            
            ContentParameters contentParameters = new ContentParameters("http://url.test", offset, width, height);
            
            // Later textures will be directly passed to the _imageTargetManager after their creation
            Texture2D texture = Resources.Load("QR-example") as Texture2D;
            
            if (texture)
            {
                _imageTargetManager = FindObjectOfType<ImageTargetManager>();
                if (_imageTargetManager && contentPrefab)
                {
                    _imageTargetManager.AddVirtualContentToImage(texture,
                        new ContentContainer(contentParameters, contentPrefab));
                }
            }

            _isDone = true;
        }
    }
}