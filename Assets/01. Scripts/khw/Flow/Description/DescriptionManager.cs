using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameDescription
{
    public GameDescription (string texts, float postDelay)
    {
        this.descriptionText = texts;
        this.postDelay = postDelay;
    }

    public string descriptionText;
    public float postDelay;
}

public class DescriptionManager : MonoBehaviour
{
    public static DescriptionManager Instance { get; private set; }

    private Queue<GameDescription> descriptionQueue = new Queue<GameDescription>();
    private bool isBeingShown = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
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

            yield return new WaitForSeconds(currentDescription.postDelay);
        }

        isBeingShown = false;
    }
}
