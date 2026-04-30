using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Hyeongyu
{
    public class FloorRiseTrigger : MonoBehaviour
    {
        private const string CinemachineBrainTypeName = "Unity.Cinemachine.CinemachineBrain";

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
            ResolveFloorReferences();
            if (!HasAnyValidFloor())
                yield break;

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
            ResolveFloorReferences();

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

            if (!IsValidCinemachineBrain(mainCameraBrain))
            {
                Camera mainCamera = mainCameraBrain != null ? mainCameraBrain.GetComponent<Camera>() : null;
                if (mainCamera == null)
                    mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();

                mainCameraBrain = mainCamera != null
                    ? mainCamera.GetComponent("CinemachineBrain") as Behaviour
                    : null;
            }
        }

        private void ResolveFloorReferences()
        {
            if (floorTransformsToRaise != null)
            {
                for (int i = 0; i < floorTransformsToRaise.Length; i++)
                {
                    if (!IsUsableFloorTransform(floorTransformsToRaise[i]))
                        floorTransformsToRaise[i] = null;
                }
            }

            if (HasAnyValidFloor())
            {
                PrepareFloorsForAnimation();
                return;
            }

            List<Transform> candidates = new List<Transform>();
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allChildren.Length; i++)
            {
                Transform child = allChildren[i];
                if (child == null || child == transform)
                    continue;
                if (IsUsableFloorTransform(child))
                    candidates.Add(child);
            }

            if (candidates.Count == 0)
                return;

            candidates.Sort((a, b) => a.localPosition.z.CompareTo(b.localPosition.z));
            floorTransformsToRaise = candidates.ToArray();
            PrepareFloorsForAnimation();
        }

        private bool HasAnyValidFloor()
        {
            if (floorTransformsToRaise == null || floorTransformsToRaise.Length == 0)
                return false;

            for (int i = 0; i < floorTransformsToRaise.Length; i++)
            {
                if (floorTransformsToRaise[i] != null)
                    return true;
            }

            return false;
        }

        private bool IsUsableFloorTransform(Transform floorTransform)
        {
            if (floorTransform == null)
                return false;
            if (floorTransform.GetComponent<FloorRiseTrigger>() != null)
                return false;
            if (floorTransform.GetComponent<PlayableDirector>() != null || HasCinemachineCameraComponent(floorTransform))
                return false;

            return floorTransform.name.IndexOf("floor", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   floorTransform.GetComponent<Renderer>() != null ||
                   floorTransform.GetComponentInChildren<Renderer>(true) != null;
        }

        private void PrepareFloorsForAnimation()
        {
            if (floorTransformsToRaise == null)
                return;

            for (int i = 0; i < floorTransformsToRaise.Length; i++)
            {
                Transform floorTransform = floorTransformsToRaise[i];
                if (floorTransform == null)
                    continue;

                if (floorTransform.gameObject.isStatic)
                    floorTransform.gameObject.isStatic = false;

                Renderer[] renderers = floorTransform.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renderers.Length; j++)
                {
                    if (renderers[j] != null && renderers[j].gameObject.isStatic)
                        renderers[j].gameObject.isStatic = false;
                }
            }
        }

        private static bool HasCinemachineCameraComponent(Transform target)
        {
            Behaviour[] behaviours = target.GetComponents<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().FullName == "Unity.Cinemachine.CinemachineCamera")
                    return true;
            }

            return false;
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

        private static bool IsValidCinemachineBrain(Behaviour behaviour)
        {
            return behaviour != null && behaviour.GetType().FullName == CinemachineBrainTypeName;
        }
    }
}
