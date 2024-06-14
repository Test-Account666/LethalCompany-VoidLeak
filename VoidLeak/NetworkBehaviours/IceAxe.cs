using UnityEngine;

namespace VoidLeak.NetworkBehaviours;

[AddComponentMenu("Weather Electric/Void Leak/Ice Axe")]
public class IceAxe : KnifeItem {
    [SerializeField]
    public Animator? animator;

    private static readonly int _SmashAnimatorHash = Animator.StringToHash("Smash");

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        animator?.SetTrigger(_SmashAnimatorHash);
    }
}