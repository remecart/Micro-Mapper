using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawInEditor : MonoBehaviour
{
    public static DrawInEditor instance;
    public Camera mainCamera;
    public LineRenderer linePrefab;
    private LineRenderer currentLine;
    private Vector3 lastMousePosition;
    private bool isDrawing = false;
    public bool drawing;
    public Transform parent;
    public RawImage img;
    public List<Texture2D> textures;

    public void ToggleDrawing()
    {
        drawing = !drawing;
        img.texture = drawing ? textures[0] : textures[1];
    }

    void Start()
    {
        instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        DrawLines();
    }

    void DrawLines()
    {
        if (drawing && !Settings.instance.isHovering)
        {
            if (Input.GetMouseButtonDown(0)) StartDrawing();
            else if (Input.GetMouseButton(0) && isDrawing) UpdateDrawing();
            else if (Input.GetMouseButtonUp(0)) EndDrawing();
        }
        else if (parent.childCount > 0)
        {
            foreach (Transform item in parent) Destroy(item.gameObject);
        }
    }

    void StartDrawing()
    {
        var colorSettings = Settings.instance.config.mapping.colorSettings;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(colorSettings.leftNote.ToColor(), 0.0f),
                new GradientColorKey(colorSettings.rightNote.ToColor(), 1.0f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );

        currentLine = Instantiate(linePrefab);
        currentLine.colorGradient = gradient;
        currentLine.positionCount = 0;
        currentLine.transform.parent = parent;
        lastMousePosition = GetMouseWorldPositionOnPlane();
        AddPointToLine(lastMousePosition);
        isDrawing = true;
    }

    void UpdateDrawing()
    {
        Vector3 mousePosition = GetMouseWorldPositionOnPlane();

        if (Vector3.Distance(mousePosition, lastMousePosition) > 0.01f)
        {
            lastMousePosition = mousePosition;
            AddPointToLine(mousePosition);
        }
    }

    void EndDrawing()
    {
        isDrawing = false;
        currentLine = null;
    }

    void AddPointToLine(Vector3 position)
    {
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount - 1, position);
    }

    Vector3 GetMouseWorldPositionOnPlane()
    {
        Vector3 viewportPoint = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        Ray ray = mainCamera.ViewportPointToRay(viewportPoint);
        Plane plane = new Plane(Vector3.forward, Vector3.zero); // Plane at Z=0

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance); // Intersection on the Z=0 plane
        }
        return Vector3.zero;
    }
}
