using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stone : Item
{
    public override void _OnUse()
    {
        CmdStoneSpawn();
    }

    [Command]
    public void CmdStoneSpawn(){
        GameObject projectile = Instantiate(player.curItemObj, player.transform.position + (player.coll.bounds.size.x + 0.1f) * (player.isHeadingRight ? Vector3.right : Vector3.left), Quaternion.identity);
        projectile.GetComponent<StoneProjectile>().dir = player.isHeadingRight;
        NetworkServer.Spawn(projectile);
    }
}
