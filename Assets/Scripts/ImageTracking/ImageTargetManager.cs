using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ImageTracking
{
    public class ImageTargetManager : MonoBehaviour
    {
        private ARSessionOrigin _arSessionOrigin;
        public ARTrackedImageManager trackedImageManager;
        private MutableRuntimeReferenceImageLibrary _imageLibrary;

        private Dictionary<string, ContentContainer> _prefabDictionary = new Dictionary<string, ContentContainer>();
        private Dictionary<string, VirtualContent> _instantiatedContents = new Dictionary<string, VirtualContent>();

        // for testing purposes
        private string _statusLog;
        
        //private bool _doOnce;

        private void Start()
        {
            _arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            trackedImageManager = GetComponent<ARTrackedImageManager>();
            if (!trackedImageManager)
            {
                trackedImageManager = gameObject.AddComponent<ARTrackedImageManager>();
            }

            _imageLibrary = trackedImageManager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
            trackedImageManager.referenceLibrary = _imageLibrary;
            trackedImageManager.enabled = true;
        }

        private void OnEnable() => trackedImageManager.trackedImagesChanged += OnChanged;

        private void OnDisable() => trackedImageManager.trackedImagesChanged -= OnChanged;

        private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            // Image was detected for the first time
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                AssignContent(trackedImage);
                _statusLog = "Virtual Content instantiated\n" + _statusLog;
            }

            foreach (ARTrackedImage updatedImage in eventArgs.updated)
            {
                string identifier = updatedImage.referenceImage.name;
                // If using MakeContentAppearAt()
                /*if (!_doOnce)
                {
                    PlaceContent(identifier, updatedImage);
                    _doOnce = true;
                }*/
                PlaceContent(identifier, updatedImage);
            }
        }

        public void PlaceContent(string identifier, ARTrackedImage imageTarget)
        {
            VirtualContent content = _instantiatedContents[identifier];
            if (content)
            {
                content.transform.localScale = content.Scale;
                Vector3 newPosition = imageTarget.transform.position +
                                      imageTarget.transform.TransformDirection(content.Parameters.Offset);
                //_arSessionOrigin.MakeContentAppearAt(content.transform, newPosition, imageTarget.transform.rotation);
                content.transform.SetPositionAndRotation(newPosition, imageTarget.transform.rotation);
            }
        }

        private void AssignContent(ARTrackedImage trackedImage)
        {
            string identifier = trackedImage.referenceImage.name;
            if (_prefabDictionary.TryGetValue(identifier, out var content))
            {
                VirtualContent instantiatedContent =
                    Instantiate(content.ContentPrefab, Vector3.zero, Quaternion.identity)
                        .GetComponent<VirtualContent>();
                instantiatedContent.Initialize(content.Parameters);
                _instantiatedContents[identifier] = instantiatedContent;
                PlaceContent(identifier, trackedImage);
            }
        }

        private IEnumerator AddImage(Texture2D imageToAdd, string identifier, float width)
        {
            if (trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
            {
                if (mutableLibrary.IsTextureFormatSupported(imageToAdd.format))
                {
                    AddReferenceImageJobState jobState = mutableLibrary.ScheduleAddImageWithValidationJob(
                        imageToAdd,
                        identifier,
                        width);

                    yield return new WaitUntil(() => jobState.jobHandle.IsCompleted);
                    if (jobState.status == AddReferenceImageJobStatus.Success)
                    {
                        _statusLog = "Image successfully added! (You can try scanning it now)\n" + _statusLog;
                    }
                    else if (jobState.status == AddReferenceImageJobStatus.ErrorInvalidImage)
                    {
                        _statusLog = "The texture is not suitable for tracking and was therefore rejected\n" +
                                     _statusLog;
                    }
                    else
                    {
                        _statusLog = $"Adding an image failed with status: {jobState.status}\n" + _statusLog;
                    }
                }
                else
                {
                    _statusLog = "Texture format not supported\n" + _statusLog;
                }
            }
        }

        public void AddVirtualContentToImage(Texture2D texture, ContentContainer contentContainer, float width = 0.05f)
        {
            string identifier = Guid.NewGuid().ToString();
            _prefabDictionary.Add(identifier, contentContainer);
            StartCoroutine(AddImage(texture, identifier, width));
        }

        void OnGUI()
        {
            // for testing purposes, later on work with pop ups/dialogs
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 30;
            style.normal.textColor = Color.magenta;
            GUI.Label(new Rect(40, 40, Screen.width / 2f, Screen.height), _statusLog, style);
        }
    }
}