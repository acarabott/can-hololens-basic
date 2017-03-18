using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class TapToPlaceParent : MonoBehaviour, IHoldHandler
{
    bool placing = false;

    public void OnHoldCanceled(HoldEventData eventData)
    {
        Debug.Log("hold cancelled");
        //placing = false;
        //SpatialMapping.Instance.DrawVisualMeshes = false;
        SpatialMapping.Instance.DrawVisualMeshes = false;
    }

    public void OnHoldCompleted(HoldEventData eventData)
    {
        Debug.Log("hold completed");
        //placing = false;
        //SpatialMapping.Instance.DrawVisualMeshes = false;
    }

    public void OnHoldStarted(HoldEventData eventData)
    {
        Debug.Log("Hold started");
        //placing = true;
        SpatialMapping.Instance.DrawVisualMeshes = true;
    }

    // Update is called once per frame
    void Update()
    {
        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (placing)
        {
            // Do a raycast into the world that will only hit the Spatial Mapping mesh.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                30.0f, SpatialMapping.PhysicsRaycastMask))
            {
                // Move this object's parent object to
                // where the raycast hit the Spatial Mapping mesh.
                // Offset by the hand'es y position
                var newPos = hitInfo.point;
                newPos.y -= this.gameObject.transform.localPosition.y;
                this.transform.parent.position = newPos; 

                // Rotate this object's parent object to face the user.
                Quaternion toQuat = Camera.main.transform.localRotation;
                toQuat.x = 0;
                toQuat.z = 0;
                this.transform.parent.rotation = toQuat;
            }
        }
    }
}
