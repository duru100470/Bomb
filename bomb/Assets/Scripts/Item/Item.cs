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
    public Player player;
    public GameObject itemObj;
    public ItemType type;
    public Sprite itemSprite;
    public abstract void OnUse();
}
