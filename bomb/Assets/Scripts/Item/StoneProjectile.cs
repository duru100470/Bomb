using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StoneProjectile : NetworkBehaviour
{
    public float stunTime = 1f;
    public bool dir;
    [SerializeField]
    float projectileSpeed = 10f;
    public void Update(){
        transform.position += (dir?new Vector3(1,0,0):new Vector3(-1,0,0)) * Time.deltaTime * projectileSpeed;
    }
}
