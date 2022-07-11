using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Placement : MonoBehaviour
{
    public GameObject primaryIndicator;
    public GameObject secondaryIndicator;
    public GameObject placementPlane;
    public GameObject qrMock;

    private CreationManager creationManager;
    private Camera mainCamera;
    private ARRaycastManager arRaycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;

    private bool touching;

    private MeshCollider planeCollider;
    private BoxCollider qrCollider;
    private Plane planeStruct;

    private bool planeIsFrozen;
    private bool qrPlacementStarted;
    private bool qrPlacementFinished;

    public ContentParameters contentParameters = new ContentParameters();

    void Start()
    {
        mainCamera = Camera.main;
        creationManager = FindObjectOfType<CreationManager>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        qrCollider = qrMock.GetComponentInChildren<BoxCollider>();
        DeactivateUI();
    }

    public void DeactivateUI()
    {
        secondaryIndicator.SetActive(false);
        primaryIndicator.SetActive(false);
        placementPlane.SetActive(false);
        qrMock.SetActive(false);
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
        var screenCenter = mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
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
                var cameraForward = mainCamera.transform.forward;
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
            foreach (var touch in Input.touches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
            }

            var ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
            if (!planeStruct.Raycast(ray, out float planeEntry)) return;

            Vector3 targetedPosition = ray.GetPoint(planeEntry);
            Vector3 nearestPlanePoint = planeCollider.ClosestPoint(targetedPosition);

            // Skip moving to position if the QR Mock would touch the nearest plane surface point
            var tempCenter = qrCollider.center;
            qrCollider.center = qrCollider.transform.InverseTransformPoint(targetedPosition);
            if (qrCollider.bounds.Contains(nearestPlanePoint))
            {
                qrCollider.center = tempCenter;
                return;
            }

            qrCollider.center = tempCenter;

            // Make it so the QR Mock will never be more than maxRadius away from the plane
            Vector3 fromSurfaceToQr = targetedPosition - nearestPlanePoint;
            fromSurfaceToQr = Vector3.ClampMagnitude(fromSurfaceToQr, 0.1f); // 10cm
            targetedPosition = nearestPlanePoint + fromSurfaceToQr;

            qrMock.transform.SetPositionAndRotation(
                targetedPosition,
                placementPlane.transform.rotation
            );
        }
    }

    private void FreezePlane()
    {
        const float minPlaneSize = 0.05f; // 5cm
        planeCollider = placementPlane.GetComponentInChildren<MeshCollider>();
        if (planeCollider.bounds.size.magnitude < minPlaneSize)
        {
            RestartPlanePlacement();
        }
        else
        {
            planeIsFrozen = true;

            // Construct plane struct from rect object
            MeshFilter filter = placementPlane.GetComponentInChildren<MeshFilter>();
            Vector3 planeNormal = filter.transform.TransformDirection(filter.mesh.normals[0]);
            planeStruct = new Plane(planeNormal, placementPlane.transform.position);

            secondaryIndicator.SetActive(false);
            primaryIndicator.SetActive(false);
            creationManager.EndRectPlacement();
        }
    }

    public void RestartPlanePlacement()
    {
        planeIsFrozen = false;
        qrPlacementStarted = false;
        qrPlacementFinished = false;
        qrMock.SetActive(false);
        placementPlane.SetActive(false);
    }

    public void StartQrMockPlacement()
    {
        qrPlacementFinished = false;
        qrPlacementStarted = true;

        if (!qrMock.activeSelf)
        {
            // Place QR Mock next to the plane
            MeshFilter filter = placementPlane.GetComponentInChildren<MeshFilter>();
            Vector3 planeTangent = filter.transform.TransformDirection(filter.mesh.tangents[0]).normalized; // x axis of plane
            float planeWidth = Mathf.Abs(placementPlane.transform.localScale.x);
            float maxRadius = planeWidth/2 + 0.05f; // 5cm
            Vector3 spawnOffset = Vector3.ClampMagnitude(planeTangent, maxRadius);
            qrMock.transform.position = placementPlane.transform.position + spawnOffset;
            qrMock.transform.rotation = placementPlane.transform.rotation;
            qrMock.SetActive(true);
        }
    }

    public void FinishPlacement()
    {
        qrPlacementFinished = true;
        //Vector3 qrSurfacePoint = qrCollider.ClosestPointOnBounds(placementPlane.transform.position);
        //Vector3 planeSurfacePoint = planeCollider.ClosestPointOnBounds(qrMock.transform.position);
        //float distanceBetweenQrAndPlane = Vector3.Distance(qrSurfacePoint, planeSurfacePoint);
        Vector3 offset = qrMock.transform.position - placementPlane.transform.position;
        offset = placementPlane.transform.InverseTransformVector(offset);
        contentParameters.Offset = offset;
        contentParameters.Width = Mathf.Abs(placementPlane.transform.localScale.x); 
        contentParameters.Height = Mathf.Abs(placementPlane.transform.localScale.y); 
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" ", guiStyle);
    }
}