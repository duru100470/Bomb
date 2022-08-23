using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Dash : Item
{
    [SerializeField] private float force = 20f;
    [SerializeField] private float dashTime = 1f;
    private Rigidbody2D rbody;

    public override void _OnUse()
    {
        player.CmdPlayAudio(AudioType.Dash);
        rbody = player.GetComponent<Rigidbody2D>();
        rbody.gravityScale = 0f;
        player.isCasting = true;
        rbody.velocity = Vector2.zero;
        rbody.velocity += new Vector2((player.isHeadingRight ? 1 : -1) * force, 0);
        player.dashVel = force;
        player.DashDone(dashTime);
    }
}