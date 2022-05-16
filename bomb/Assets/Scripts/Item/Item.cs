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
    public PlayerStateManager player;
    public GameObject itemObj;
    public ItemType type;
    public Sprite itemSprite;
    public ItemSpawner spawner;
    public void OnUse(){
        _OnUse();
        CmdDestroy(netId);
    }
    public abstract void _OnUse();

    [Command]
    protected void CmdDestroy(uint netId){
        player.RpcItemDestroy(netId);
    }
}
