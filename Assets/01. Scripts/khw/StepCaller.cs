using UnityEngine;

public enum EStepType
{
    None,
    GameStart,
    FirstRoom,
    SecondRoom,
    ThirdRoom
}

public class StepCaller : MonoBehaviour
{
    public static void CallStep(EStepType stepType)
    {
        switch (stepType)
        {
            case EStepType.None: 
                break;
            case EStepType.GameStart: 
                ProgressManager.Instance.ChangeProgress(FindAnyObjectByType<GameStartStep>());
                break;
            case EStepType.FirstRoom: 
                ProgressManager.Instance.ChangeProgress(FindAnyObjectByType<FirstRoomStep>());
                break;
            case EStepType.SecondRoom:
                ProgressManager.Instance.ChangeProgress(FindAnyObjectByType<SecondRoomStep>());
                break;
            case EStepType.ThirdRoom:
                ProgressManager.Instance.ChangeProgress(FindAnyObjectByType<ThirdRoomStep>());
                break;
        }

    }
}