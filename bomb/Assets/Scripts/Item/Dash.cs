using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Item
{
    [SerializeField]
    private float force = 20f;
    [SerializeField]
    private float dashTime = 1f;
    private float refVelocity = 0.0f;
    private Rigidbody2D rbody;
        
    public override void OnUse()
    {
        Debug.Log("Dash");
        rbody = player.GetComponent<Rigidbody2D>();
        rbody.gravityScale = 0f;
        rbody.velocity = Vector2.zero;
        player.isCasting = true;
        rbody.AddForce((player.isHeadingRight? new Vector2(1,0) : new Vector2(-1,0)) * force, ForceMode2D.Impulse);
        player.dashTime = dashTime;
        Invoke("DashDone",dashTime);
        Destroy(gameObject,dashTime +0.1f);
    }
    private void DashDone(){
        player.isCasting = false;
        rbody.velocity = Vector2.zero;
        rbody.gravityScale = 1f;
    }
}