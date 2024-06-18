using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace VoidLeak.NetworkBehaviours;

public class DevGun : PhysicsProp {
    private static readonly System.Random _Random = new();

    [SerializeField]
    private Animator? animator;

    [SerializeField]
    private Transform? gunLaserPoint;

    [FormerlySerializedAs("particleSystem")]
    [SerializeField]
    private ParticleSystem? shootParticles;

    [SerializeField]
    private ParticleSystem? steamParticles;

    [SerializeField]
    private AudioSource? audioSource;

    [SerializeField]
    private AudioClip? shootAudioClip;

    [SerializeField]
    private List<AudioClip>? noObjectAudioClips;

    public override void DiscardItem() {
        base.DiscardItem();

        StopParticles();
    }

    public override void PocketItem() {
        base.PocketItem();

        StopParticles();
    }

    private void StopParticles() {
        shootParticles?.Stop();
        steamParticles?.Stop();
    }


    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (animator is null) return;

        if (playerHeldBy.playerEye is null) return;

        var grabbableObject = GetGrabbableObject();

        if (grabbableObject is null) {
            PlayNoObjectSoundServerRpc();
            return;
        }

        UseDevGunServerRpc(grabbableObject.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    public void UseDevGunClientRpc(NetworkObjectReference grabbableObject, int newValue) {
        var item = ((NetworkObject) grabbableObject).GetComponent<GrabbableObject>();

        StartCoroutine(SetValue(item, newValue));

        StartCoroutine(PlayParticles());
    }

    private static IEnumerator SetValue(GrabbableObject? grabbableObject, int newValue) {
        if (grabbableObject is null) yield break;

        var currentValue = grabbableObject.scrapValue;
        var startTime = Time.time;
        const float duration = 1f; // 1 Second

        while (Time.time - startTime < duration) {
            var normalizedTime = (Time.time - startTime) / duration;
            var interpolatedValue = Mathf.Lerp(currentValue, newValue, normalizedTime);

            grabbableObject.SetScrapValue((int) interpolatedValue);
            yield return null; // Wait for the next frame
        }

        grabbableObject.SetScrapValue(newValue); // Ensure the final value is set correctly
    }


    private IEnumerator PlayParticles() {
        if (audioSource is not null && shootAudioClip is not null) {
            audioSource.clip = shootAudioClip;
            audioSource.Play();
        }

        shootParticles?.Play();

        yield return new WaitForSeconds(.8F);

        steamParticles?.Play();
    }

    private GrabbableObject? GetGrabbableObject() {
        if (playerHeldBy.playerEye is null) return null;

        const int intersectingLayerMask = 1 << 0 | 1 << 8 | 1 << 24 | 1 << 25 | 1 << 26;

        const int propsLayerMask = 1 << 6;

        var rayCastHit = Physics.Raycast(playerHeldBy.playerEye.position, playerHeldBy.playerEye.forward, out var hitInfo, 5,
                                         propsLayerMask, QueryTriggerInteraction.Ignore);

        if (!rayCastHit) return null;

        var lineCastHit = Physics.Linecast(playerHeldBy.playerEye.position, hitInfo.point, intersectingLayerMask,
                                           QueryTriggerInteraction.Ignore);

        if (lineCastHit) return null;

        var grabbableObject = hitInfo.collider.GetComponent<GrabbableObject>();


        if (grabbableObject is not null) return !grabbableObject.itemProperties.isScrap? null : grabbableObject;

        Plugin.logger.LogError("Dev gun found no object???");
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayNoObjectSoundServerRpc() {
        PlayNoObjectSoundClientRpc();
    }

    [ClientRpc]
    public void PlayNoObjectSoundClientRpc() {
        if (audioSource is null) return;

        if (noObjectAudioClips is not {
                Count: > 0,
            }) return;

        audioSource.clip = noObjectAudioClips[_Random.Next(0, noObjectAudioClips.Count)];
        audioSource.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseDevGunServerRpc(NetworkObjectReference grabbableObjectReference) {
        var networkObject = (NetworkObject) grabbableObjectReference;

        var grabbableObject = networkObject.GetComponent<GrabbableObject>();

        if (grabbableObject is null) {
            PlayNoObjectSoundClientRpc();
            return;
        }

        var currentValue = grabbableObject.scrapValue;

        var action = GenerateAction();

        ExecuteAction(action, ref currentValue, grabbableObject, networkObject);

        UseDevGunClientRpc((NetworkObjectReference) networkObject, currentValue);
    }

    private void ExecuteAction(DevGunAction action, ref int currentValue, GrabbableObject grabbableObject, NetworkObject networkObject) {
        switch (action) {
            case DevGunAction.DESTROY:
                currentValue += 1600;
                StartCoroutine(DelayedAction(() => {
                    SpawnExplosionClientRpc(grabbableObject.transform.position);

                    networkObject.Despawn();
                    Destroy(grabbableObject.gameObject);
                }));
                break;
            case DevGunAction.ADD:
                currentValue += _Random.Next(action.GetMinimum(), action.GetMaximum());
                break;
            case DevGunAction.SUBTRACT:
                currentValue -= _Random.Next(action.GetMinimum(), action.GetMaximum());
                break;
            case DevGunAction.MULTIPLY:
                currentValue *= _Random.Next(action.GetMinimum(), action.GetMaximum());
                break;
            case DevGunAction.DIVIDE:
                currentValue /= _Random.Next(action.GetMinimum(), action.GetMaximum());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Is not implemented, yet???");
        }
    }

    [ClientRpc]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void SpawnExplosionClientRpc(Vector3 position) {
        Landmine.SpawnExplosion(position, true, killRange: .5F, damageRange: 3F, nonLethalDamage: 25, physicsForce: 1F);
    }

    private static IEnumerator DelayedAction(Action action) {
        yield return new WaitForSeconds(1F);
        action.Invoke();
    }

    private static DevGunAction GenerateAction() {
        var randomNumber = _Random.Next(0, 300);

        while (true) {
            foreach (var action in DevGunConfig.ActionSet) {
                randomNumber -= action.GetWeight();

                if (randomNumber > 0) continue;

                return action;
            }
        }
    }

    public enum DevGunAction {
        DIVIDE,
        MULTIPLY,
        DESTROY,
        SUBTRACT,
        ADD,
    }
}