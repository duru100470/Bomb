using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;
    [SerializeField] private List<GameObject> roomPlayerPrefabList;
    [SerializeField] private List<GameObject> playerPrefabList;
    public List<bool> playerPrefabMemory = new List<bool>();
    public bool playersReady = false;

    public override void OnStartHost()
    {
        hostIP = PlayerSetting.hostIP;
        // foreach(var obj in roomPlayerPrefabList)
        // {
        //     playerPrefabMemory.Add(false);
        // }
    }

    public override void OnStartClient()
    {
        foreach(var obj in roomPlayerPrefabList)
        {
            playerPrefabMemory.Add(false);
        }
    }

    public override void OnRoomServerPlayersReady()
    {
        playersReady = true;
    }

    public void StartGame()
    {
        PlayerSetting.playerNum = roomSlots.Count;
        ServerChangeScene(GameplayScene);
    }

    public override void OnRoomServerPlayersNotReady()
    {
        playersReady = false;
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
        GameObject obj = Instantiate(playerPrefabList[roomPlayer.GetComponent<RoomPlayer>().PrefabIndex], new Vector3(-4, 0, 0), Quaternion.identity);
        return obj;
    }

    public override void OnRoomStopServer()
    {
        SoundManager.Instance.ResetAudioSource();
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        SoundManager.Instance.RemoveAudioSource(roomPlayer.GetComponent<AudioSource>());
        Destroy(roomPlayer, .5f);
        return true;
    }


}