using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Hyeongyu
{
    public class DoorTrigger : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private PlayableDirector cutsceneDirector;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private Behaviour cinemachineBrain;
        [SerializeField] private Behaviour cutsceneCamera;

        private bool _triggered;

        private void Awake()
        {
            ResolveRuntimeReferences();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag("Player")) return;
            _triggered = true;
            StartCoroutine(PlayCutscene());
        }

        private IEnumerator PlayCutscene()
        {
            ResolveRuntimeReferences();
            BindTimelineOutputs();

            if (playerMovement != null)
                playerMovement.enabled = false;

            Camera cam = cinemachineBrain != null ? cinemachineBrain.GetComponent<Camera>() : null;
            Transform camTransform = cam != null ? cam.transform : null;
            Vector3 cachedLocalPos = Vector3.zero;
            Quaternion cachedLocalRot = Quaternion.identity;
            float cachedFov = 60f;
            if (camTransform != null)
            {
                cachedLocalPos = camTransform.localPosition;
                cachedLocalRot = camTransform.localRotation;
                cachedFov = cam.fieldOfView;
                if (cutsceneCamera != null)
                    cutsceneCamera.enabled = true;
                cinemachineBrain.enabled = true;
            }

            if (cutsceneDirector != null)
                cutsceneDirector.Play();

            Coroutine doorClose = door != null ? door.Close() : null;

            if (cutsceneDirector != null)
                yield return new WaitForSeconds((float)cutsceneDirector.duration);
            else if (doorClose != null)
                yield return doorClose;
            else
                yield return null;

            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = false;
                if (cutsceneCamera != null)
                    cutsceneCamera.enabled = false;
                if (camTransform != null)
                {
                    camTransform.localPosition = cachedLocalPos;
                    camTransform.localRotation = cachedLocalRot;
                    cam.fieldOfView = cachedFov;
                }
            }

            if (playerMovement != null)
                playerMovement.enabled = true;
        }

        private void ResolveRuntimeReferences()
        {
            if (door == null)
                door = GetComponentInChildren<DoorController>(true);

            if (cutsceneDirector == null)
                cutsceneDirector = GetComponentInChildren<PlayableDirector>(true);

            if (cutsceneCamera == null)
            {
                foreach (Behaviour behaviour in GetComponentsInChildren<Behaviour>(true))
                {
                    if (behaviour.GetType().FullName == "Unity.Cinemachine.CinemachineCamera")
                    {
                        cutsceneCamera = behaviour;
                        break;
                    }
                }
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (playerMovement == null && player != null)
                playerMovement = player.GetComponent("PlayerMovement") as MonoBehaviour;

            if (cinemachineBrain == null)
            {
                Camera cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
                if (cam != null)
                    cinemachineBrain = cam.GetComponent("CinemachineBrain") as Behaviour;
            }
        }

        private void BindTimelineOutputs()
        {
            if (cutsceneDirector == null || cutsceneDirector.playableAsset == null || cinemachineBrain == null)
                return;

            foreach (PlayableBinding output in cutsceneDirector.playableAsset.outputs)
            {
                Object source = output.sourceObject;
                if (source != null && source.GetType().FullName == "Unity.Cinemachine.CinemachineTrack")
                    cutsceneDirector.SetGenericBinding(source, cinemachineBrain);
            }
        }
    }
}
