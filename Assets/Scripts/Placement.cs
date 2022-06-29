using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Placement : MonoBehaviour
{
    public GameObject primaryIndicator;
    public GameObject secondaryIndicator;
    public GameObject placementPlane;
    public GameObject qrMock;

    // Holding buttons to begin next placement phase or cancel
    public GameObject rectPlacementOptions;
    public GameObject qrPlacementOptions;

    private ARRaycastManager arRaycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;

    private bool touching;

    private BoxCollider planeCollider;
    private BoxCollider qrCollider;

    private bool planeIsFrozen;
    private bool qrPlacementStarted;
    private bool qrPlacementFinished;

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        placementPlane.SetActive(false);
        qrMock.SetActive(false);
        rectPlacementOptions.SetActive(false);
        qrPlacementOptions.SetActive(false);
        planeCollider = placementPlane.GetComponentInChildren<BoxCollider>();
        qrCollider = qrMock.GetComponentInChildren<BoxCollider>();
    }

    void Update()
    {
        if (!planeIsFrozen)
        {
            UpdatePlacementPose();
            UpdateprimaryIndicator();
            ReactToTouch();
        }
        else
        {
            UpdateQrMockPlacement();
        }
    }

    private void ReactToTouch()
    {
        if (Input.touchCount > 0)
        {
            if (!touching)
            {
                setActiveAndRotateToPlacementPost(secondaryIndicator);
                touching = true;
            }

            placementPlane.SetActive(true);

            // Set Plane position to midpoint between first and second placement indicator
            Vector3 position = Vector3.Lerp(
                secondaryIndicator.transform.position,
                primaryIndicator.transform.position,
                0.5f
            );
            placementPlane.transform.SetPositionAndRotation(
                position,
                secondaryIndicator.transform.rotation
            );

            // Find Cornerpoints of square and scale the Square accodingly
            Vector3 diagonal = secondaryIndicator.transform.InverseTransformPoint(
                primaryIndicator.transform.position
            );
            placementPlane.transform.localScale = new Vector3(diagonal.x, 1, diagonal.z);

            // Rotate primaryPlacementIndicator in same direction as the secondary
            primaryIndicator.transform.rotation = secondaryIndicator.transform.rotation;

            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                FreezePlane();
            }
        }
        else
        {
            secondaryIndicator.SetActive(false);
            touching = false;
        }
    }

    private void setActiveAndRotateToPlacementPost(GameObject indicator)
    {
        indicator.SetActive(true);
        indicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
    }

    private void UpdateprimaryIndicator()
    {
        if (placementPoseIsValid)
        {
            setActiveAndRotateToPlacementPost(primaryIndicator);
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
        //Debug.Log(placementPoseIsValid);
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

    private void UpdateQrMockPlacement()
    {
        if (Input.touchCount > 0 && qrPlacementStarted && !qrPlacementFinished)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            var touch = Input.GetTouch(0);
            if (arRaycastManager.Raycast(touch.position, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                Vector3 planePosition = placementPlane.transform.position;
                Quaternion planeRotation = placementPlane.transform.rotation;

                // Adjust target position to be on level with plane, first check if its on a wall, then ground and else -> must be at the ceiling
                Vector3 targetedPosition;
                if (planeRotation.y == planeRotation.z)
                {
                    targetedPosition = new Vector3(hitPose.position.x,
                        hitPose.position.y, planePosition.z);
                }
                else if (planeRotation.x == planeRotation.z)
                {
                    Debug.Log("on ground");
                    targetedPosition = new Vector3(hitPose.position.x, planePosition.y,
                        hitPose.position.z);
                }
                else
                {
                    targetedPosition = new Vector3(planePosition.x,
                        hitPose.position.y, hitPose.position.z);
                }

                Vector3 planeSurfacePoint = planeCollider.ClosestPoint(targetedPosition);

                var tempCenter = qrCollider.center;
                qrCollider.center = qrCollider.transform.InverseTransformPoint(targetedPosition);

                // Skip moving to position if targeted pos is inside plane or the QR Mock would touch the plane surface point
                if (planeCollider.bounds.Contains(targetedPosition) || qrCollider.bounds.Contains(planeSurfacePoint))
                {
                    qrCollider.center = tempCenter;
                    return;
                }

                qrCollider.center = tempCenter;

                float planeToSurfacePointDistance =
                    Vector3.Distance(planePosition, planeSurfacePoint);
                float maxRadius = planeToSurfacePointDistance + 0.1f; // 10cm
                Vector3 fromPlaneToQr = targetedPosition - planePosition;
                fromPlaneToQr = Vector3.ClampMagnitude(fromPlaneToQr, maxRadius);
                targetedPosition = planePosition + fromPlaneToQr;
                qrMock.transform.SetPositionAndRotation(
                    targetedPosition,
                    placementPlane.transform.rotation
                );
            }
        }
    }

    private void FreezePlane()
    {
        const float minPlaneSize = 0.05f; // 5cm
        if (planeCollider.bounds.size.x < minPlaneSize || planeCollider.bounds.size.y < minPlaneSize)
        {
            RedoPlanePlacement();
        }
        else
        {
            secondaryIndicator.SetActive(false);
            primaryIndicator.SetActive(false);
            rectPlacementOptions.SetActive(true);
            planeIsFrozen = true;
        }
    }

    public void RedoPlanePlacement()
    {
        enabled = false;
        planeIsFrozen = false;
        qrPlacementStarted = false;
        qrMock.SetActive(false);
        placementPlane.SetActive(false);
        rectPlacementOptions.SetActive(false);
        qrPlacementOptions.SetActive(false);
        enabled = true;
    }

    public void StartQrMockPlacement()
    {
        enabled = false;
        qrPlacementStarted = true;
        qrMock.SetActive(true);
        qrMock.transform.position = placementPlane.transform.position +
                                    Vector3.right * (planeCollider.bounds.size.x / 2f + 0.05f);
        qrMock.transform.rotation = placementPlane.transform.rotation;

        qrMock.transform.parent = placementPlane.transform;
        rectPlacementOptions.SetActive(false);
        qrPlacementOptions.SetActive(true);
        enabled = true;
    }

    public void CancelQrMockPlacement()
    {
        RedoPlanePlacement();
    }

    public void FinishPlacement()
    {
        enabled = false;
        qrPlacementFinished = true;

        Vector3 qrSurfacePoint = qrCollider.ClosestPointOnBounds(placementPlane.transform.position);
        Vector3 planeSurfacePoint = planeCollider.ClosestPointOnBounds(qrMock.transform.position);

        // Future ContentParameters
        float distanceBetweenQrAndPlane = Vector3.Distance(qrSurfacePoint, planeSurfacePoint);
        Vector3 offsetBetweenQrAndPlane = qrMock.transform.position - placementPlane.transform.position;
        float planeWidth = planeCollider.bounds.size.x;
        float planeHeight = planeCollider.bounds.size.y;

        //TODO Write Manager class that accepts this information...

        qrPlacementOptions.SetActive(false);
        enabled = true;
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" ", guiStyle);
    }
}