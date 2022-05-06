using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{
    Dash = 0,
    Stone
}

public abstract class Item : MonoBehaviour
{
    public ItemType type;
    public Sprite itemSprite;
    public abstract void OnUse();
}
