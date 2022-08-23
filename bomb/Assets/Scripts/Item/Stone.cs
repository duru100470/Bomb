using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stone : Item
{
    [SerializeField] private float force = 20f;
    public override void _OnUse()
    {
        CmdStoneSpawn();
    }

    [Command]
    public void CmdStoneSpawn()
    {
        player.CmdPlayAudio(AudioType.Stone_Throw);
        GameObject projectile = Instantiate(player.curItemObj, player.transform.position + (player.coll.bounds.size.x + 0.1f) * (player.isHeadingRight ? Vector3.right : Vector3.left), Quaternion.identity);
        StoneProjectile sProj = projectile.GetComponent<StoneProjectile>();
        sProj.dir = player.isHeadingRight;
        sProj.heading = !player.isHeadingRight;
        sProj.playerNickname = player.playerNickname;
        sProj.force = force;
        NetworkServer.Spawn(projectile);
    }
}
