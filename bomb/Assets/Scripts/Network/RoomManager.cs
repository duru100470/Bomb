using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;
    [SerializeField] private List<RoomPlayer> roomPlayerList;
    [SerializeField] private List<GameObject> roomPlayerPrefabList;
    [SerializeField] private List<GameObject> playerPrefabList;
    public Dictionary<int, bool> playerPrefabMemory = new Dictionary<int, bool>();

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
        foreach(var obj in roomPlayerPrefabList)
        {
            playerPrefabMemory.Add(roomPlayerPrefabList.IndexOf(obj), false);
        }
    }

    public override void OnRoomServerPlayersReady()
    {
        PlayerSetting.playerNum = roomSlots.Count;
        base.OnRoomServerPlayersReady();
    }

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        if(roomPlayerPrefabList.Count == roomSlots.Count) return null;
        GameObject temp;
        int idx;
        while(true)
        {
            idx = Random.Range(0, roomPlayerPrefabList.Count);
            if(!playerPrefabMemory[idx])
            {
                temp = roomPlayerPrefabList[idx];
                playerPrefabMemory[idx] = true;
                break;
            }
        }
        GameObject obj = Instantiate(temp, new Vector3(0,-4,0), Quaternion.identity);
        obj.GetComponent<RoomPlayer>().PrefabIndex = idx;
        return obj;
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        GameObject obj = Instantiate(playerPrefabList[roomPlayer.GetComponent<RoomPlayer>().PrefabIndex], Vector3.zero, Quaternion.identity);
        return obj;
    }
}