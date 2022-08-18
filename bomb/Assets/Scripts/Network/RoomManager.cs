using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;
    [SerializeField] private List<RoomPlayer> roomPlayerList;
    
    [SerializeField] private List<GameObject> playerPrefabList;
    private Dictionary<GameObject, bool> playerPrefabMemory = new Dictionary<GameObject, bool>();

    public void AddPlayer(RoomPlayer player)
    {
        if (!roomPlayerList.Contains(player))
        {
            roomPlayerList.Add(player);
            Debug.Log("roomPlayer Added");
        }
    }

    public List<RoomPlayer> GetPlayerList()
    {
        return roomPlayerList;
    } 
    
    public override void OnStartHost()
    {
        hostIP = PlayerSetting.hostIP;
        foreach(var obj in playerPrefabList)
        {
            playerPrefabMemory.Add(obj, false);
        }
    }

    public override void OnRoomServerPlayersReady()
    {
        PlayerSetting.playerNum = roomSlots.Count;
        base.OnRoomServerPlayersReady();
    }

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        GameObject temp;
        while(true)
        {
            int idx = Random.Range(0, playerPrefabList.Count);
            if(!playerPrefabMemory[playerPrefabList[idx]])
            {
                temp = playerPrefabList[idx];
                playerPrefabMemory[playerPrefabList[idx]] = true;
                break;
            }
        }
        GameObject obj = Instantiate(temp, new Vector3(0,-4,0), Quaternion.identity);
        return obj;
    }


}