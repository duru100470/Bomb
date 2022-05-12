using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stone : Item
{
    public override void _OnUse()
    {
        //Debug.Log("Stone");
        CmdStoneSpawn();
    }

    
    [Command(requiresAuthority = false)]
    public void CmdStoneSpawn(){
        GameObject projectile = Instantiate(player.curItemObj, player.transform.position + (player.isHeadingRight ? new Vector3(1,0,0) : new Vector3(-1,0,0)), Quaternion.identity);
        projectile.GetComponent<StoneProjectile>().dir = player.isHeadingRight;
        NetworkServer.Spawn(projectile);
    }
}
