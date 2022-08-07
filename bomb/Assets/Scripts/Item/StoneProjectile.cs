using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StoneProjectile : NetworkBehaviour
{
    public float StunTime => stunTime;
    [SerializeField] private float stunTime = 1f;
    [SyncVar] public float force;
    [SyncVar] public bool dir;
    [SyncVar] public bool heading;
    [SerializeField] float projectileSpeed = 10f;
    public PlayerStateManager player;

    public void Start()
    {
        GetComponent<SpriteRenderer>().flipX = heading;
    }

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
