using Unity.Netcode;
using UnityEngine;

namespace VoidLeak.NetworkBehaviours;

[AddComponentMenu("Weather Electric/Void Leak/Spawn Gun")]
public class SpawnGun : GrabbableObject {
    [Space(30f)]
    public Transform firePoint;

    [Space(10f)]
    public GameObject spawnObject;

    [Space(10f)]
    public float raycastDistance = 15f;

    [Space(10f)]
    public AudioSource spawnAudio;

    [Space(10f)]
    public AudioSource missFireAudio;

    [Space(10f)]
    public GameObject laser;

    [Space(10f)]
    public Animator animator;

    private static readonly int _Shoot = Animator.StringToHash("Shoot");

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);
        if (GameNetworkManager.Instance.localPlayerController is null)
            return;

        if (insertedBattery.charge <= 0) {
            missFireAudio.Play();
            SetActive(true);
            return;
        }

        UseSpawnGunServerRpc();

        if (insertedBattery.charge <= 0) {
            SetActive(true);
            return;
        }

        insertedBattery.charge -= 0.1f;
    }

    public override void ChargeBatteries() {
        base.ChargeBatteries();
        SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseSpawnGunServerRpc() {
        UseSpawnGunClientRpc();

        var ray = new Ray(firePoint.position, firePoint.forward);
        if (!Physics.Raycast(ray, out var hit, raycastDistance))
            return;

        var soupObject = Instantiate(spawnObject, hit.point, Quaternion.identity);

        var networkObject = soupObject.GetComponent<NetworkObject>();

        networkObject.Spawn();
    }

    [ClientRpc]
    private void UseSpawnGunClientRpc() {
        animator.SetTrigger(_Shoot);

        spawnAudio.Play();
    }

    public override void DiscardItem() {
        base.DiscardItem();
        SetActive(false);
    }

    public override void PocketItem() {
        base.PocketItem();
        SetActive(false);
    }

    public override void GrabItem() {
        base.GrabItem();
        SetActive(true);
    }

    public override void EquipItem() {
        base.EquipItem();
        SetActive(true);
    }

    private void SetActive(bool active) {
        var notEmpty = insertedBattery.charge > 0;

        if (laser.activeSelf != (active && notEmpty)) {
            laser.SetActive((active && notEmpty));
            var meshRenderer = laser.GetComponent<MeshRenderer>();

            if (meshRenderer is not null && meshRenderer.enabled != (active && notEmpty))
                meshRenderer.enabled = active && notEmpty;
        }

        if (playerHeldBy is null)
            return;

        playerHeldBy.equippedUsableItemQE = active;
    }
}