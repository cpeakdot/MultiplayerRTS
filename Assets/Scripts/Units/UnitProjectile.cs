using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damageToDeal = 20;

    private void Start() 
    {
        rigidBody.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent(out NetworkIdentity networkIdentity))
        {
            if (networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        if(other.TryGetComponent(out Health health))
        {
            health.DealDamage(damageToDeal);
        }

        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }
} 
