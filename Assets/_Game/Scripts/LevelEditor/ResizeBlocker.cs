using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeBlocker : MonoBehaviour
{
    public RectTransform uiPanel;
    public Camera uiCamera;
    public Transform worldPlane; // The plane to project onto (use its position and rotation)

    void Start()
    {
        MatchObjectToPanelOnPlane();
    }

    void MatchObjectToPanelOnPlane()
    {
        Vector3[] corners = new Vector3[4];
        uiPanel.GetWorldCorners(corners); // Get corners in world space (screen-space UI)

        // Convert world corners to screen space
        for (int i = 0; i < 4; i++)
        {
            corners[i] = uiCamera.WorldToScreenPoint(corners[i]);
        }

        // Raycast to world plane to get intersection points
        Vector3[] worldPoints = new Vector3[4];
        Plane plane = new Plane(worldPlane.up, worldPlane.position);

        for (int i = 0; i < 4; i++)
        {
            Ray ray = uiCamera.ScreenPointToRay(corners[i]);
            if (plane.Raycast(ray, out float enter))
            {
                worldPoints[i] = ray.GetPoint(enter);
            }
        }

        // Set position and scale of your object
        Vector3 center = (worldPoints[0] + worldPoints[2]) / 2f;
        transform.position = center;

        Vector3 right = worldPoints[3] - worldPoints[0]; // width
        Vector3 up = worldPoints[1] - worldPoints[0];    // height

        float width = right.magnitude;
        float height = up.magnitude;

        transform.localScale = new Vector3(width, 0.01f, height); // flat object, scale in X/Z

        // Optional: orient to match panel
        transform.rotation = Quaternion.LookRotation(worldPlane.forward, worldPlane.up);
    }
}
