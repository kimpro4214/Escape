using System.Collections.Generic;
using UnityEngine;

public class Puzzle2Node : MonoBehaviour
{
    [Header("이웃 노드 연결")]
    public List<Puzzle2Node> neighbors;

    [Header("호버 효과")]
    public float hoverScale = 1.2f;
    public float hoverDuration = 0.15f;

    private Puzzle2 _manager;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _propBlock;

    public bool isOn = false;

    private Vector3 _baseScale;
    private Vector3 _targetScale;
    private Vector3 _fromScale;
    private float _lerpT;
    private bool _isLerping;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _manager = FindAnyObjectByType<Puzzle2>();
        _propBlock = new MaterialPropertyBlock();
        _baseScale = transform.localScale;
        _targetScale = _baseScale;
        _fromScale = _baseScale;
    }

    void Start()
    {
        UpdateVisual();
    }

    void Update()
    {
        if (!_isLerping) return;

        _lerpT += Time.deltaTime / hoverDuration;
        transform.localScale = Vector3.Lerp(_fromScale, _targetScale, _lerpT);

        if (_lerpT >= 1f)
        {
            transform.localScale = _targetScale;
            _isLerping = false;
        }
    }

    private void OnMouseEnter()
    {
        SetTargetScale(_baseScale * hoverScale);
    }

    private void OnMouseExit()
    {
        SetTargetScale(_baseScale);
    }

    private void SetTargetScale(Vector3 target)
    {
        _fromScale = transform.localScale;
        _targetScale = target;
        _lerpT = 0f;
        _isLerping = true;
    }

    private void OnMouseDown()
    {
        GetComponentInParent<Puzzle2>().HandleNodeClick(this);
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (_renderer != null && _manager != null)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", isOn ? _manager.onColor : _manager.offColor);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
