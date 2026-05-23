using UnityEngine;

namespace Hyeongyu
{
    public class ClueManager : MonoBehaviour
    {
        public static ClueManager Instance { get; private set; }

        [SerializeField] private int totalClues = 3;

        private int _foundCount;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void OnClueFound()
        {
            _foundCount++;

            if (_foundCount == 1)
                FlowManager.Instance?.StartFlow(new Flow_ClueFound_First());
            else if (_foundCount == totalClues)
                FlowManager.Instance?.StartFlow(new Flow_ClueFound_Last());
            else
                FlowManager.Instance?.StartFlow(new Flow_ClueFound_Middle());
        }
    }
}
