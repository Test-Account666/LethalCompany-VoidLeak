using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace VoidLeak.NetworkBehaviours;

[AddComponentMenu("Weather Electric/Void Leak/Destroy On Timer")]
public class DestroyOnTimer : NetworkBehaviour {
    [Tooltip("The object to destroy. If null, the script will destroy whatever object the script is on.")]
    public GameObject objectToDestroy;

    [Space(10f)]
    [Tooltip("The time to destroy the object.")]
    public float timeToDestroy = 5f;

    private void Start() => StartCoroutine(Destroy(objectToDestroy == null? gameObject : objectToDestroy, timeToDestroy));

    private IEnumerator Destroy(GameObject obj, float time) {
        yield return new WaitForSeconds(time);

        if (!IsHost && !IsServer) yield break;

        var networkObject = obj.GetComponent<NetworkObject>();

        networkObject?.Despawn();
        Object.Destroy(obj);
    }
}