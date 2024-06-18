using Unity.Netcode;
using UnityEngine;

namespace VoidLeak.NetworkBehaviours;

[AddComponentMenu("Soup")]
public class SoupBehaviour : NetworkBehaviour {
    private const float KICK_STRENGTH = .5F;

    [Tooltip("The rigidBody of this object.")]
    [SerializeField]
    public Rigidbody rigidBody;

    [Tooltip("The audio source of this object.")]
    [SerializeField]
    public AudioSource collideAudioSource;

    private void OnTriggerEnter(Collider other) => HandleCollision(other);

    private void OnTriggerStay(Collider other) => HandleCollision(other);

    private void HandleCollision(Collider other) {
        if (other.attachedRigidbody == rigidBody) return;

        if (!other.CompareTag("PlayerBody") && !other.CompareTag("Player")) return;

        if (!collideAudioSource.isPlaying)
            collideAudioSource.Play();

        if (this is not {
                IsHost: true,
            }) return;

        var direction = other.transform.forward;

        direction.Normalize();

        direction = -direction;

        direction.y = .7F;

        rigidBody.AddForce(direction * KICK_STRENGTH, ForceMode.Impulse);
    }
}