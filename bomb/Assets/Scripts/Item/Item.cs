using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{
    Dash,
    Stone
}

public class Item : MonoBehaviour
{
    private Player player;
    [SerializeField]
    private Sprite itemSprite;

    // 아이템 사용 시 효과
    public virtual void OnUse(){
        
    }

    // 플레이어와 아이템 충돌 시 획득
    private void OnCollisionEnter(Collision other) {
        
    }
}
