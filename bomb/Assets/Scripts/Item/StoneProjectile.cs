using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StoneProjectile : NetworkBehaviour
{
    public float stunTime = 1f;
    public float force;
    [SyncVar]
    public bool dir;
    [SerializeField]
    float projectileSpeed = 10f;
    public PlayerStateManager player;
    public void Update()
    {
        transform.position += (dir ? Vector3.right : Vector3.left) * Time.deltaTime * projectileSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Ground"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
