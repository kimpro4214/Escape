using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    // 현재 진행 중인 스테이지 (부모 타입으로 보관)
    [SerializeField] private ProgressStepBase currentProgress;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 시작 단계가 설정되어 있다면 실행
        currentProgress?.OnEnterProgress();
    }

    private void Update()
    {
        // 매 프레임 업데이트 실행
        currentProgress?.OnUpdateProgress();
    }

    // 단계를 변경하는 핵심 메서드
    public void ChangeProgress(ProgressStepBase nextProgress)
    {
        if (nextProgress == null) return;

        currentProgress?.OnExitProgress(); // 기존 단계 퇴장
        currentProgress = nextProgress;     // 교체
        currentProgress.OnEnterProgress(); // 새 단계 입장

        Debug.Log($"단계 변경됨: {nextProgress.GetType().Name}");
    }
}
