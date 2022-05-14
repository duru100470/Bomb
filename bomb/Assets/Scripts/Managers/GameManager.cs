using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private Transform[] spawnTransforms;

    private List<PlayerStateManager> players = new List<PlayerStateManager>();

    public void AddPlayer(PlayerStateManager player){
        if(!players.Contains(player)){
            players.Add(player);
            Debug.Log("Player Added");
        }
    }

    public List<PlayerStateManager> GetPlayerList(){
        return players;
    }

    private IEnumerator GameReady(){
        var manager = NetworkManager.singleton as RoomManager;
        while(manager.roomSlots.Count != players.Count){
            yield return null;
        }

        for (int i = 0; i < players.Count; i++){
            players[i].RpcTeleport(spawnTransforms[i].position);
        }
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (isServer){
            StartCoroutine(GameReady());
        }
    }
}
