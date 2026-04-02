using System.Collections.Generic;
using UnityEngine;

public class Puzzle2Node : MonoBehaviour
{
    [Header("이웃 노드 연결")]
    public List<Puzzle2Node> neighbors;

    private Puzzle2 _manager;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _propBlock;

    public bool isOn = false;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _manager = FindAnyObjectByType<Puzzle2>(); // 매니저 자동 찾기
        _propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        UpdateVisual();
    }

    // 상호작용 시 호출. 임시로 버튼 클릭으로 했지만 상호작용 가능한 오브젝트 인터페이스 구현 함수로 바꾸면 됨.
    private void OnMouseDown()
    {
        GetComponentInParent<Puzzle2>().HandleNodeClick(this);
    }

    // 노드 상태 반전
    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
    }

    // 활성화 여부에 따라 색상 변경 (onColor < - > offColor)
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
