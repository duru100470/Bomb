using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stone : Item
{
    private GameObject projectile;
    public override void OnUse()
    {
        //Debug.Log("Stone");
        player.CmdStoneSpawn();
        player.CmdDestroy(gameObject);
    }
}
