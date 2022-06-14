using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTargetManager : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    private MutableRuntimeReferenceImageLibrary _imageLibrary;

    private Dictionary<string, GameObject> _prefabDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _instantiatedContent = new Dictionary<string, GameObject>();

    // for testing purposes
    private string _statusLog;

    private void Start()
    {
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
            float minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
            trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
            AssignPrefab(trackedImage);
            _statusLog = "Virtual Content instantiated\n" + _statusLog;
        }
    }

    void AssignPrefab(ARTrackedImage trackedImage)
    {
        if (_prefabDictionary.TryGetValue(trackedImage.referenceImage.name, out var prefab))
            _instantiatedContent[trackedImage.referenceImage.name] = Instantiate(prefab, trackedImage.transform);
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
                    _statusLog = "Image successfully added!\n(You can try scanning it now)\n" + _statusLog;
                }
                else if (jobState.status == AddReferenceImageJobStatus.ErrorInvalidImage)
                {
                    _statusLog = "The texture is not suitable for tracking and was therefore rejected\n" + _statusLog;
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

    public void AddVirtualContentToImage(Texture2D texture, GameObject content, float width = 0.05f)
    {
        string identifier = Guid.NewGuid().ToString();
        _prefabDictionary.Add(identifier, content);
        StartCoroutine(AddImage(texture, identifier, width));
    }

    void OnGUI()
    {
        // for testing purposes, later on work with pop ups/dialogs
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 30;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(40, 40, Screen.width / 2f, Screen.height), _statusLog, style);
    }
}