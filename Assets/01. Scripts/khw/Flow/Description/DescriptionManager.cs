using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameDescription
{
    public GameDescription (string texts, float timeShown)
    {
        this.descriptionText = texts;
        this.timeShown = timeShown;
    }

    public string descriptionText;
    public float timeShown;
}

public class DescriptionManager : MonoBehaviour
{
    public static DescriptionManager Instance { get; private set; }

    // 1. 실제 화면에 보여줄 컨트롤러 참조 (인스펙터에서 할당)
    [SerializeField] private DescriptionController descriptionController;

    private Queue<GameDescription> descriptionQueue = new Queue<GameDescription>();
    private bool isBeingShown = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 2. 만약 할당을 안했다면 자동으로 찾아보기
        if (descriptionController == null)
            descriptionController = FindAnyObjectByType<DescriptionController>();
    }

    public void AddDescription(GameDescription description)
    {
        descriptionQueue.Enqueue(description);

        if (!isBeingShown)
        {
            StartCoroutine(ShowDescription());
        }
    }

    public void AddDescrptions(List<GameDescription> descriptions)
    {
        foreach (var line in descriptions)
        {
            descriptionQueue.Enqueue(line);
        }

        if (!isBeingShown)
        {
            StartCoroutine(ShowDescription());
        }
    }

    public void ForceStartDescrptions(List<GameDescription> descriptions)
    {
        StopAllCoroutines();
        descriptionQueue.Clear();
        isBeingShown = false;

        AddDescrptions(descriptions);
    }

    private IEnumerator ShowDescription()
    {
        isBeingShown = true;

        while (descriptionQueue.Count > 0)
        {
            GameDescription currentDescription = descriptionQueue.Dequeue();

            // 3. 핵심: 컨트롤러에게 보여달라고 명령하기!
            if (descriptionController != null)
            {
                descriptionController.ShowDescription(
                    currentDescription.descriptionText,
                    currentDescription.timeShown
                );
            }

            // 컨트롤러가 보여주는 시간만큼 여기서도 기다려줘야 큐의 다음 내용과 겹치지 않습니다.
            yield return new WaitForSeconds(currentDescription.timeShown);

            // 만약 자막 사이의 아주 약간의 간격을 두고 싶다면 여기에 추가 연출을 넣을 수 있습니다.
        }

        isBeingShown = false;
    }
}
