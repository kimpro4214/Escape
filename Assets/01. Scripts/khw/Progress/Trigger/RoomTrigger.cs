using UnityEngine;

public class RoomTrigger : MonoBehaviour
{ 
    [SerializeField] private EStepType stepType;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StepCaller.CallStep(stepType);

            Destroy(gameObject);
        }
    }
}
