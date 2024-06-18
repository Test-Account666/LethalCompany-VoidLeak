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

        if (playerHeldBy is null) return;

        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        if (!IsHost) ShootSpawnGunServerRpc();
        else ShootSpawnGunClientRpc();

        if (insertedBattery.charge <= 0) return;

        var position = firePoint.position + firePoint.forward * raycastDistance;

        var ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out var hit, raycastDistance)) position = hit.point;

        if (!IsHost) UseSpawnGunServerRpc(position);
        else SpawnSoupClientRpc(position);
    }

    public override void ChargeBatteries() {
        base.ChargeBatteries();
        SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UseSpawnGunServerRpc(Vector3 position) {
        SpawnSoupClientRpc(position);
    }

    [ClientRpc]
    private void SpawnSoupClientRpc(Vector3 position) {
        var soupObject = Instantiate(spawnObject, position, Quaternion.identity);

        insertedBattery.charge -= 0.1f;

        if (insertedBattery.charge <= 0) SetActive(false);

        if (!IsHost) return;

        var networkObject = soupObject.GetComponent<NetworkObject>();

        networkObject.Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootSpawnGunServerRpc() {
        ShootSpawnGunClientRpc();
    }

    [ClientRpc]
    private void ShootSpawnGunClientRpc() {
        if (insertedBattery.charge <= 0) {
            missFireAudio.Play();
            SetActive(false);
            return;
        }

        animator.SetTrigger(_Shoot);

        spawnAudio.Play();
    }

    public override void UseUpBatteries() {
        base.UseUpBatteries();
        SetActive(false);
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
        if (insertedBattery.charge <= 0) {
            SetActive(false);
            return;
        }

        SetActive(true);
    }

    public override void EquipItem() {
        base.EquipItem();
        if (insertedBattery.charge <= 0) {
            SetActive(false);
            return;
        }

        SetActive(true);
    }

    private void SetActive(bool active) {
        active = active && playerHeldBy is not null;

        if (laser.activeSelf != active) {
            laser.SetActive(active);
            var meshRenderer = laser.GetComponent<MeshRenderer>();

            if (meshRenderer is not null && meshRenderer.enabled != active)
                meshRenderer.enabled = active;
        }

        if (playerHeldBy is null) return;

        playerHeldBy.equippedUsableItemQE = true;
    }
}