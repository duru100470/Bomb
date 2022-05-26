using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Item
{
    public override void _OnUse()
    {
        player.rigid2d.velocity = new Vector2(player.rigid2d.velocity.x, 0);
        player.rigid2d.velocity += Vector2.up * player.JumpForce;
    }
}
