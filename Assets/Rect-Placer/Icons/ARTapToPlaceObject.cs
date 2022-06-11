using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject primaryIndicator;
    public GameObject secondaryIndicator;
    public GameObject placementPlane;

    private ARRaycastManager arRaycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;

    private bool touching;

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        placementPlane.SetActive(false);
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdateprimaryIndicator();
        ReactToTouch();
    }

    private void ReactToTouch()
    {
        if (Input.touchCount > 0)
        {
            if (!touching)
            {
                secondaryIndicator.SetActive(true);

                secondaryIndicator.transform.SetPositionAndRotation(
                    placementPose.position,
                    placementPose.rotation
                );

                touching = true;
            }

            placementPlane.SetActive(true);
            
            Vector3 position = Vector3.Lerp(secondaryIndicator.transform.position, primaryIndicator.transform.position, 0.5f);

            placementPlane.transform.SetPositionAndRotation(
                position,
                secondaryIndicator.transform.rotation
            );
            placementPlane.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            touching = false;
        }
    }

    private void UpdateprimaryIndicator()
    {
        if (placementPoseIsValid)
        {
            primaryIndicator.SetActive(true);
            primaryIndicator.transform.SetPositionAndRotation(
                placementPose.position,
                placementPose.rotation
            );
        }
        else
        {
            primaryIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        Debug.Log(placementPoseIsValid);
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
            var upVector = placementPose.up;

            if (Math.Abs(upVector.x) + Math.Abs(upVector.z) < Math.Abs(upVector.y))
            {
                var cameraForward = Camera.main.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
            else
            {
                Quaternion orientation = Quaternion.identity;
                Quaternion zUp = Quaternion.identity;
                GetWallPlacement(hits[0], out orientation, out zUp);
                placementPose.rotation = zUp;
            }
        }
    }

    private void GetWallPlacement(
        ARRaycastHit _planeHit,
        out Quaternion orientation,
        out Quaternion zUp
    )
    {
        TrackableId planeHit_ID = _planeHit.trackableId;
        ARPlaneManager arPlaneManager = FindObjectOfType<ARPlaneManager>();
        ARPlane planeHit = arPlaneManager.GetPlane(planeHit_ID);
        Vector3 planeNormal = planeHit.normal;
        orientation = Quaternion.FromToRotation(Vector3.up, planeNormal);
        Vector3 forward = _planeHit.pose.position - (_planeHit.pose.position + Vector3.down);
        zUp = Quaternion.LookRotation(forward, planeNormal);
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" ", guiStyle);
    }
}
