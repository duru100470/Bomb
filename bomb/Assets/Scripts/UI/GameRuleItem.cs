using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRuleItem : MonoBehaviour
{
    [SerializeField] private GameObject inactiveObject;
    void Start()
    {
        if(!RoomPlayer.MyPlayer.isServer)
        {
            inactiveObject.SetActive(false);
        }
    }
}
