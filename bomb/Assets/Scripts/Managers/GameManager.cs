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

    // 플레이어 리스트에 플레이어 추가
    public void AddPlayer(PlayerStateManager player){
        if(!players.Contains(player)){
            players.Add(player);
            Debug.Log("Player Added");
        }
    }

    public List<PlayerStateManager> GetPlayerList(){
        return players;
    }

    // 게임이 시작될 시 실행되는 코루틴
    private IEnumerator GameReady(){
        var manager = NetworkManager.singleton as RoomManager;
        // 플레이어들이 모두 접속 시 까지 대기
        while(manager.roomSlots.Count != players.Count){
            yield return null;
        }

        // 랜덤 플레이어에게 폭탄 부여
        for (int i = 0; i < 1; i++){
            var player = players[Random.Range(0, players.Count)];
            if (player.hasBomb == false){
                player.hasBomb = true;
            }else{
                i--;
            }
        }

        // 플레이어들을 지정된 스폰위치에 생성
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
