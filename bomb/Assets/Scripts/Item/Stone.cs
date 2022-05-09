using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stone : Item
{
    [SerializeField]
    private GameObject projectile;
    public override void OnUse()
    {
        Debug.Log("Stone");
        player.StoneSpawn(projectile);
    }
}
