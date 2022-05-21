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

    private float maxBombGlobalTime;
    private float minBombGlobalTime;
    [SyncVar]
    public float bombGlobalTime;
    

    // 플레이어 리스트에 플레이어 추가
    public void AddPlayer(PlayerStateManager player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log("Player Added");
        }
    }

    public List<PlayerStateManager> GetPlayerList()
    {
        return players;
    }

    // 게임이 시작될 시 실행되는 코루틴
    private IEnumerator GameReady()
    {
        var manager = NetworkManager.singleton as RoomManager;
        // 플레이어들이 모두 접속 시 까지 대기
        while (manager.roomSlots.Count != players.Count)
        {
            yield return null;
        }

        //기본 시간설정
        bombGlobalTime = Mathf.Round(Random.Range(minBombGlobalTime, maxBombGlobalTime));

        // 랜덤 플레이어에게 폭탄줌
        for (int i = 0; i < GameRuleStore.Instance.CurGameRule.bombCount; i++)
        {
            var player = players[Random.Range(0, players.Count)];
            if (!player.hasBomb)
            {
                player.hasBomb = !player.hasBomb;
                player.playerLocalBombTime = Mathf.Round(bombGlobalTime / 5);
            }
            else
            {
                i--;
            }
        }

        // 플레이어들을 지정된 스폰위치에 생성
        for (int i = 0; i < players.Count; i++)
        {
            players[i].RpcTeleport(spawnTransforms[i].position);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (isServer)
        {
            maxBombGlobalTime = GameRuleStore.Instance.CurGameRule.maxBombTime;
            minBombGlobalTime = GameRuleStore.Instance.CurGameRule.minBombTime;
            StartCoroutine(GameReady());
        }
    }
}
