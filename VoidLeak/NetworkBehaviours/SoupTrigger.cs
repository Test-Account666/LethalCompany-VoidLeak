using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace VoidLeak.NetworkBehaviours;

public class SoupTrigger : NetworkBehaviour {
    private static bool _underTheInfluence;

    [SerializeField]
    private NetworkObject? networkObject;

    [SerializeField]
    private GameObject? parentObject;

    public void Interact() {
        if (this is not {
                parentObject: not null,
                networkObject: not null,
            }) return;

        if (_underTheInfluence) {
            HUDManager.Instance.DisplayTip("Soup???", "Nah, I think I'm good :)");
            return;
        }

        HUDManager.Instance.DisplayTip("Soup???", "You feel refreshed!");

        var localPlayer = StartOfRound.Instance.localPlayerController;

        localPlayer.StartCoroutine(Energize(localPlayer, localPlayer.sprintTime));

        EatSoupServerRpc();
    }

    private static IEnumerator Energize(PlayerControllerB? localPlayer, float sprintTime) {
        if (localPlayer == null) {
            _underTheInfluence = false;
            yield break;
        }

        _underTheInfluence = true;

        localPlayer.sprintMeter = 1F;
        localPlayer.sprintTime = 666F;

        yield return new WaitForSeconds(12);

        if (localPlayer == null) {
            _underTheInfluence = false;
            yield break;
        }

        localPlayer.sprintTime = sprintTime;

        var times = 0;

        while (true) {
            if (localPlayer == null) break;

            if (localPlayer.isPlayerDead) break;

            if (times >= 20) break;

            yield return new WaitForSeconds(.5F);
            localPlayer.sprintMeter = 0;
            localPlayer.drunkness += .1F;
            if (times < 10) localPlayer.DamagePlayer(1, causeOfDeath: CauseOfDeath.Suffocation);
            times += 1;
        }

        yield return new WaitForSeconds(.5F);
        _underTheInfluence = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EatSoupServerRpc() {
        EatSoupClientRpc();
    }

    [ClientRpc]
    public void EatSoupClientRpc() {
        //TODO: Play sound

        if (!IsHost && !IsServer) return;

        networkObject?.Despawn();

        Destroy(parentObject);
    }
}