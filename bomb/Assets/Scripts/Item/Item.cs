using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum ItemType
{
    Dash = 0,
    Stone,
    Jump
}

public abstract class Item : NetworkBehaviour
{
    [SyncVar] public PlayerStateManager player;
    public GameObject itemObj;
    public ItemType Type => type;
    [SerializeField] private ItemType type;
    public Sprite itemSprite;
    public ItemSpawner spawner;
    private SpriteRenderer spriteRenderer;

    public void Start() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemSprite;
    }

    public void OnUse()
    {
        _OnUse();
        player.CmdSetItemAnim((int)type);
        CmdDestroy(netId);
    }
    public abstract void _OnUse();

    public void DiscardItem()
    {
        CmdDestroy(netId);
    }

    [Command(requiresAuthority = false)]
    protected void CmdDestroy(uint netId)
    {   
        NetworkServer.Destroy(player.curItem.gameObject);
    }
}
