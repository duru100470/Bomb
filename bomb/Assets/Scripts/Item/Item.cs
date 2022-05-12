using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType{
    Dash = 0,
    Stone
}

public abstract class Item : NetworkBehaviour
{
    [SyncVar]
    public Player player;
    public GameObject itemObj;
    public ItemType type;
    public Sprite itemSprite;
    public void OnUse(){
        _OnUse();
        CmdDestroy(GetComponent<NetworkIdentity>().netId);
    }
    public abstract void _OnUse();

    [Command(requiresAuthority = false)]
    protected void CmdDestroy(uint netId){
        player.RpcItemDestroy(netId);
    }
}
