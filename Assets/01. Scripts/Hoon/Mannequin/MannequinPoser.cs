using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MannequinPoser : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Settings")]
    public float rotationSpeed = 0.3f;
    public float selectionPixelRadius = 40f;

    [Header("Keyboard Rotation")]
    public float keyboardSpeed = 60f;

    [Header("Excluded Bones")]
    public string[] excludeKeywords = { "Index", "Middle", "Ring", "Little", "Thumb" };

    Transform selectedBone;
    Vector3 lastMousePos;
    bool isDraggingBody;

    RectTransform highlightDot;
    bool isActive = false;

    Stack<(Transform bone, Quaternion rotation)> undoStack = new Stack<(Transform, Quaternion)>();

    Quaternion initialRotation;
    Dictionary<Transform, Quaternion> initialBoneRotations = new Dictionary<Transform, Quaternion>();

    Vector3 initialPosition;
    Vector3 bodyCenter;

    void Start()
    {
        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = false;

        if (targetCamera == null)
            targetCamera = Camera.main;

        foreach (Transform bone in GetComponentsInChildren<Transform>())
        {
            if (bone == transform) continue;
            var col = bone.GetComponent<SphereCollider>();
            if (col) Destroy(col);
        }

        var meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            Bounds bounds = meshRenderers[0].bounds;
            foreach (var r in meshRenderers) bounds.Encapsulate(r.bounds);
            bodyCenter = bounds.center;
        }
        else bodyCenter = transform.position;

        initialRotation = transform.localRotation;
        initialPosition  = transform.localPosition;

        foreach (Transform bone in GetComponentsInChildren<Transform>())
            if (bone != transform)
                initialBoneRotations[bone] = bone.localRotation;
    }

    public void Activate()
    {
        isActive = true;
        CreateHighlightDot();
    }

    public void Deactivate()
    {
        isActive = false;
        selectedBone = null;
        isDraggingBody = false;

        if (highlightDot != null)
        {
            Destroy(highlightDot.transform.parent.gameObject);
            highlightDot = null;
        }
    }

    void CreateHighlightDot()
    {
        var canvasGO = new GameObject("_BoneHighlightCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var dotGO = new GameObject("_Dot");
        dotGO.transform.SetParent(canvasGO.transform, false);

        var image = dotGO.AddComponent<Image>();
        image.sprite = CreateCircleSprite(64);
        image.color = new Color(1f, 0.1f, 0.1f, 0.85f);

        highlightDot = dotGO.GetComponent<RectTransform>();
        highlightDot.sizeDelta = new Vector2(24f, 24f);
        highlightDot.anchorMin = Vector2.zero;
        highlightDot.anchorMax = Vector2.zero;
        highlightDot.pivot = new Vector2(0.5f, 0.5f);

        dotGO.SetActive(false);
    }

    Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float r = center - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float alpha = Mathf.Clamp01((r - dist) * 2f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
            TrySelectBone();

        if (Input.GetMouseButton(0))
        {
            if (selectedBone != null)
                RotateSelectedBone();
            else if (isDraggingBody)
                RotateBody();
        }

        if (Input.GetMouseButtonUp(0))
            isDraggingBody = false;

        if (selectedBone != null)
            KeyboardRotate();
        else
            KeyboardRotateBody();

        if (Input.GetKeyDown(KeyCode.Z) && !Input.GetKey(KeyCode.LeftControl) && undoStack.Count > 0)
        {
            var (bone, rot) = undoStack.Pop();
            bone.localRotation = rot;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            transform.localRotation = initialRotation;
            transform.localPosition = initialPosition;
            Debug.Log("전체 회전/위치 초기화");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.localRotation = initialRotation;
            transform.localPosition = initialPosition;
            foreach (var pair in initialBoneRotations)
                pair.Key.localRotation = pair.Value;
            Debug.Log("포즈 전체 초기화");
        }

        if (Input.GetMouseButtonDown(1))
            selectedBone = null;

        UpdateHighlightDot();
    }

    void UpdateHighlightDot()
    {
        if (highlightDot == null) return;

        if (selectedBone != null)
        {
            Vector3 screenPos = targetCamera.WorldToScreenPoint(selectedBone.position);
            if (screenPos.z > 0)
            {
                highlightDot.gameObject.SetActive(true);
                highlightDot.anchoredPosition = new Vector2(screenPos.x, screenPos.y);
            }
            else
            {
                highlightDot.gameObject.SetActive(false);
            }
        }
        else
        {
            highlightDot.gameObject.SetActive(false);
        }
    }

    void TrySelectBone()
    {
        Transform closest = null;
        float minDist = selectionPixelRadius;

        foreach (Transform bone in GetComponentsInChildren<Transform>())
        {
            if (bone == transform) continue;
            if (IsExcluded(bone.name)) continue;

            Vector3 screenPos = targetCamera.WorldToScreenPoint(bone.position);
            if (screenPos.z < 0) continue;

            float dist = Vector2.Distance(
                new Vector2(Input.mousePosition.x, Input.mousePosition.y),
                new Vector2(screenPos.x, screenPos.y)
            );

            if (dist < minDist)
            {
                minDist = dist;
                closest = bone;
            }
        }

        if (closest != null)
        {
            undoStack.Push((closest, closest.localRotation));
            selectedBone = closest;
            isDraggingBody = false;
        }
        else
        {
            selectedBone = null;
            isDraggingBody = true;
            // 몸체 드래그는 RotateAround로 위치도 바뀌므로 언두 스택에 쌓지 않음
        }

        lastMousePos = Input.mousePosition;
    }

    void RotateSelectedBone()
    {
        Vector3 delta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (delta.sqrMagnitude < 0.01f) return;

        Transform cam = targetCamera.transform;
        selectedBone.Rotate(cam.up,    -delta.x * rotationSpeed, Space.World);
        selectedBone.Rotate(cam.right,  delta.y * rotationSpeed, Space.World);
    }

    void RotateBody()
    {
        Vector3 delta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (delta.sqrMagnitude < 0.01f) return;

        transform.RotateAround(bodyCenter, Vector3.up, -delta.x * rotationSpeed);
        transform.RotateAround(bodyCenter, targetCamera.transform.right, delta.y * rotationSpeed);
    }

    void KeyboardRotate()
    {
        float step = keyboardSpeed * Time.deltaTime;
        Vector3 axis = Vector3.zero;

        if (Input.GetKey(KeyCode.A)) axis = Vector3.up      * -step;
        if (Input.GetKey(KeyCode.D)) axis = Vector3.up      *  step;
        if (Input.GetKey(KeyCode.W)) axis = Vector3.right   * -step;
        if (Input.GetKey(KeyCode.S)) axis = Vector3.right   *  step;
        if (Input.GetKey(KeyCode.Q)) axis = Vector3.forward *  step;
        if (Input.GetKey(KeyCode.E)) axis = Vector3.forward * -step;

        if (axis == Vector3.zero) return;

        bool isFirstPress =
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E);

        if (isFirstPress)
            undoStack.Push((selectedBone, selectedBone.localRotation));

        selectedBone.Rotate(axis, Space.Self);
    }

    void KeyboardRotateBody()
    {
        float step = keyboardSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A)) transform.RotateAround(bodyCenter, Vector3.up,    -step);
        if (Input.GetKey(KeyCode.D)) transform.RotateAround(bodyCenter, Vector3.up,     step);
        if (Input.GetKey(KeyCode.W)) transform.RotateAround(bodyCenter, targetCamera.transform.right, -step);
        if (Input.GetKey(KeyCode.S)) transform.RotateAround(bodyCenter, targetCamera.transform.right,  step);
        if (Input.GetKey(KeyCode.Q)) transform.RotateAround(bodyCenter, targetCamera.transform.forward,  step);
        if (Input.GetKey(KeyCode.E)) transform.RotateAround(bodyCenter, targetCamera.transform.forward, -step);
    }

    bool IsExcluded(string boneName)
    {
        foreach (var keyword in excludeKeywords)
            if (boneName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

    void OnDrawGizmos()
    {
        if (selectedBone == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(selectedBone.position, 0.08f);
    }
}
