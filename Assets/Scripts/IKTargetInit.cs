using UnityEngine;

public class IKTargetInit : MonoBehaviour
{
    public HumanBodyBones bone = HumanBodyBones.RightHand;

    void Awake()
    {
        var animator = GetComponentInParent<Animator>();
        if (animator == null) return;

        var boneTransform = animator.GetBoneTransform(bone);
        if (boneTransform == null) return;

        transform.position = boneTransform.position;
        transform.rotation = boneTransform.rotation;
    }
}
