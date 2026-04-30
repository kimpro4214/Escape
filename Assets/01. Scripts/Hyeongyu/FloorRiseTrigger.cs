using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Hyeongyu
{
    public class FloorRiseTrigger : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("floorsToRise")] private Transform[] floorTransformsToRaise;
        [SerializeField, FormerlySerializedAs("targetY")] private float targetLocalY;
        [SerializeField, FormerlySerializedAs("riseDuration")] private float raiseDurationSeconds = 2f;
        [SerializeField, FormerlySerializedAs("cutsceneDirector")] private PlayableDirector cutscenePlayableDirector;
        [SerializeField, FormerlySerializedAs("useScriptFloorRise")] private bool animateFloorsInScript = true;
        [SerializeField, FormerlySerializedAs("playerMovement")] private MonoBehaviour playerMovementBehaviour;
        [SerializeField, FormerlySerializedAs("cinemachineBrain")] private Behaviour mainCameraBrain;
        [SerializeField, FormerlySerializedAs("cutsceneCamera")] private Behaviour cutsceneVirtualCamera;

        private bool hasTriggered;

        private void Awake()
        {
            ResolveRuntimeReferences();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered || !other.CompareTag("Player")) return;
            hasTriggered = true;
            StartCoroutine(PlayCutscene());
        }

        private IEnumerator PlayCutscene()
        {
            ResolveRuntimeReferences();
            BindTimelineOutputs();

            if (playerMovementBehaviour != null)
                playerMovementBehaviour.enabled = false;

            Camera mainCamera = mainCameraBrain != null ? mainCameraBrain.GetComponent<Camera>() : null;
            Transform mainCameraTransform = mainCamera != null ? mainCamera.transform : null;
            Vector3 cachedCameraLocalPosition = Vector3.zero;
            Quaternion cachedCameraLocalRotation = Quaternion.identity;
            float cachedCameraFieldOfView = 60f;
            if (mainCameraTransform != null)
            {
                cachedCameraLocalPosition = mainCameraTransform.localPosition;
                cachedCameraLocalRotation = mainCameraTransform.localRotation;
                cachedCameraFieldOfView = mainCamera.fieldOfView;
                if (cutsceneVirtualCamera != null)
                    cutsceneVirtualCamera.enabled = true;
                mainCameraBrain.enabled = true;
            }

            if (cutscenePlayableDirector != null)
                cutscenePlayableDirector.Play();

            if (animateFloorsInScript)
                yield return RiseFloors();
            else if (raiseDurationSeconds > 0f)
                yield return new WaitForSeconds(raiseDurationSeconds);

            if (cutscenePlayableDirector != null)
            {
                float remainingTimelineSeconds = (float)cutscenePlayableDirector.duration - raiseDurationSeconds;
                if (remainingTimelineSeconds > 0f)
                    yield return new WaitForSeconds(remainingTimelineSeconds);
            }

            if (mainCameraBrain != null)
            {
                mainCameraBrain.enabled = false;
                if (cutsceneVirtualCamera != null)
                    cutsceneVirtualCamera.enabled = false;
                if (mainCameraTransform != null)
                {
                    mainCameraTransform.localPosition = cachedCameraLocalPosition;
                    mainCameraTransform.localRotation = cachedCameraLocalRotation;
                    mainCamera.fieldOfView = cachedCameraFieldOfView;
                }
            }

            if (playerMovementBehaviour != null)
                playerMovementBehaviour.enabled = true;
        }

        private IEnumerator RiseFloors()
        {
            float elapsedSeconds = 0f;
            Vector3[] startLocalPositions = System.Array.ConvertAll(
                floorTransformsToRaise,
                floorTransform => floorTransform != null ? floorTransform.localPosition : Vector3.zero);
            while (elapsedSeconds < raiseDurationSeconds)
            {
                elapsedSeconds += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedSeconds / raiseDurationSeconds);
                for (int i = 0; i < floorTransformsToRaise.Length; i++)
                {
                    if (floorTransformsToRaise[i] == null) continue;

                    Vector3 localPosition = startLocalPositions[i];
                    localPosition.y = Mathf.Lerp(startLocalPositions[i].y, targetLocalY, normalizedTime);
                    floorTransformsToRaise[i].localPosition = localPosition;
                }
                yield return null;
            }

            for (int i = 0; i < floorTransformsToRaise.Length; i++)
            {
                if (floorTransformsToRaise[i] == null) continue;

                Vector3 localPosition = floorTransformsToRaise[i].localPosition;
                localPosition.y = targetLocalY;
                floorTransformsToRaise[i].localPosition = localPosition;
            }
        }

        private void ResolveRuntimeReferences()
        {
            if (cutscenePlayableDirector == null)
                cutscenePlayableDirector = GetComponentInChildren<PlayableDirector>(true);

            if (cutsceneVirtualCamera == null)
            {
                foreach (Behaviour behaviour in GetComponentsInChildren<Behaviour>(true))
                {
                    if (behaviour.GetType().FullName == "Unity.Cinemachine.CinemachineCamera")
                    {
                        cutsceneVirtualCamera = behaviour;
                        break;
                    }
                }
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (playerMovementBehaviour == null && player != null)
                playerMovementBehaviour = player.GetComponent("PlayerMovement") as MonoBehaviour;

            if (mainCameraBrain == null)
            {
                Camera mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
                if (mainCamera != null)
                    mainCameraBrain = mainCamera.GetComponent("CinemachineBrain") as Behaviour;
            }
        }

        private void BindTimelineOutputs()
        {
            if (cutscenePlayableDirector == null || cutscenePlayableDirector.playableAsset == null || mainCameraBrain == null)
                return;

            foreach (PlayableBinding output in cutscenePlayableDirector.playableAsset.outputs)
            {
                Object source = output.sourceObject;
                if (source != null && source.GetType().FullName == "Unity.Cinemachine.CinemachineTrack")
                    cutscenePlayableDirector.SetGenericBinding(source, mainCameraBrain);
            }
        }
    }
}
