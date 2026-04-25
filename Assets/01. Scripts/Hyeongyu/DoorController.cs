using System.Collections;
using UnityEngine;

namespace Hyeongyu
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private float closeDuration = 1f;

        public Coroutine Close() => StartCoroutine(CloseCoroutine());

        private IEnumerator CloseCoroutine()
        {
            Vector3 start = transform.localPosition;
            Vector3 end = start + Vector3.down * 3f;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / closeDuration;
                transform.localPosition = Vector3.Lerp(start, end, t);
                yield return null;
            }
            transform.localPosition = end;
        }
    }
}
