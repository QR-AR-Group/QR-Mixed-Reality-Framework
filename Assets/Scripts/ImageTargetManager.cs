using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTargetManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    private MutableRuntimeReferenceImageLibrary _imageLibrary;
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
        foreach (ARTrackedImage newImage in eventArgs.added)
        {
            _statusLog = "Image is being tracked (name=" + newImage.referenceImage.name + ")\n" + _statusLog;
        }

        foreach (ARTrackedImage updatedImage in eventArgs.updated)
        {
        }

        foreach (ARTrackedImage removedImage in eventArgs.removed)
        {
        }
    }

    public IEnumerator AddImage(Texture2D imageToAdd, float width)
    {
        if (trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            if (mutableLibrary.IsTextureFormatSupported(imageToAdd.format))
            {
                AddReferenceImageJobState jobState = mutableLibrary.ScheduleAddImageWithValidationJob(
                    imageToAdd,
                    "test",
                    width);

                yield return new WaitUntil(() => jobState.jobHandle.IsCompleted);
                if (jobState.status == AddReferenceImageJobStatus.Success)
                {
                    _statusLog = "Image successfully added! You can try scanning it now\n" + _statusLog;
                }
                else if (jobState.status == AddReferenceImageJobStatus.ErrorInvalidImage)
                {
                    _statusLog = "The Texture is not suitable for tracking and was therefore rejected\n" + _statusLog;
                }
                else
                {
                    _statusLog = "Adding Image job failed with status: " + jobState.status + "\n" + _statusLog;
                }
            }
            else
            {
                _statusLog = "Texture format not supported\n" + _statusLog;
            }
        }
    }

    void OnGUI()
    {
        // for testing purposes
        var style = new GUIStyle(GUI.skin.label);
        style.fontSize = 30;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(40, 40, Screen.width / 2, Screen.height), _statusLog, style);
    }
}