using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private RoomManager manager;
    [SerializeField] private List<Transform> spawnTransforms = new List<Transform>();
    //전체 플레이어 인원 리스트
    private List<PlayerStateManager> players = new List<PlayerStateManager>();
    //생존 플레이어 인원 리스트
    private List<PlayerStateManager> alivePlayers = new List<PlayerStateManager>();

    private float maxBombGlobalTime;
    private float minBombGlobalTime;
    [SyncVar][SerializeField] public float bombGlobalTime;
    [SyncVar] private int curBombGlobalTime;
    private int roundWinningPoint;
    [SyncVar] public bool isPlayerMovable = true;
 
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
            roundWinningPoint = GameRuleStore.Instance.CurGameRule.roundWinningPoint;
            StartCoroutine(GameReady());
        }
    }

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
        manager = NetworkManager.singleton as RoomManager;
        // 플레이어들이 모두 접속 시 까지 대기
        while(manager.clientIndex != players.Count)
        {
            yield return null;
        }

        alivePlayers = players.ToList();

        //기본 시간설정
        bombGlobalTime = Mathf.Round(Random.Range(minBombGlobalTime, maxBombGlobalTime));

        // 플레이어들을 지정된 스폰위치에 생성
        for (int i = 0; i < players.Count; i++)
        {
            players[i].playerLocalBombTime = Mathf.Round(bombGlobalTime / 5);
            players[i].RpcTeleport(spawnTransforms[i].position);
        }

        // 랜덤 플레이어에게 폭탄줌
        for (int i = 0; i < GameRuleStore.Instance.CurGameRule.bombCount; i++)
        {
            var player = players[Random.Range(0, players.Count)];
            if (!player.hasBomb)
            {
                player.hasBomb = !player.hasBomb;
            }
            else
            {
                i--;
            }
        }
    }

    public void bombExplode(PlayerStateManager deadPlayer){
        if(!isServer) return;
        alivePlayers.Remove(deadPlayer);

        if(alivePlayers.Count <= GameRuleStore.Instance.CurGameRule.bombCount) {
            PlayerStateManager winner = alivePlayers[0];
            winner.roundScore += 1;
            if(winner.roundScore >= roundWinningPoint)
            {
                //최종 라운드 승리자 생기는 경우
                Debug.Log("winner : " + winner.netId);
                Debug.Log("Round End!");
                manager.ServerChangeScene(manager.RoomScene);
            }
            else
            {
                //점수는 얻었지만 라운드 승리자가 없는 경우
                Debug.Log(alivePlayers[0].netId + " get point!");
                StartCoroutine(RoundReset());
            }
        }
        else
        { 
            //아직 두명이상이 생존해 있는 경우
            StartCoroutine(BombRedistribution(deadPlayer.transform.position));
        }
    }

    //생존자 수에 따른 시간과 폭탄 재분배
    private IEnumerator BombRedistribution(Vector3 explosionPos)
    {
        StartCoroutine(StopPlayer(1f));

        curBombGlobalTime = (int)(bombGlobalTime * alivePlayers.Count / players.Count);

        float max = 0;
        PlayerStateManager maxPlayer = alivePlayers[0]; 
        for(int i=0; i< alivePlayers.Count; i++){
            alivePlayers[i].playerLocalBombTime = Mathf.Round(curBombGlobalTime / 5);
            float dist = Vector3.SqrMagnitude(alivePlayers[i].transform.position - explosionPos);
            if(dist > max){
                max = dist;
                maxPlayer = alivePlayers[i];
            }
        }
        maxPlayer.hasBomb = true;

        yield return null;
    }

    private IEnumerator RoundReset()
    {
        StartCoroutine(StopPlayer(1f));
        
        alivePlayers = players.ToList();
        for (int i=0; i< players.Count; i++)
        {
            players[i].playerLocalBombTime = Mathf.Round(bombGlobalTime / 5);
            players[i].RpcPlayerRoundReset();
            players[i].DiscardItem();
        }

        // 플레이어들을 랜덤한 스폰위치에 생성
        List<Transform> temp = spawnTransforms.ToList();
        List<Transform> rand = new List<Transform>();
        while(temp.Count != 0){
            int target = Random.Range(0, temp.Count);
            rand.Add(temp[target]);
            temp.Remove(temp[target]);
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].RpcTeleport(rand[i].position);
        }

        for (int i = 0; i < GameRuleStore.Instance.CurGameRule.bombCount; i++)
        {
            var player = players[Random.Range(0, players.Count)];
            if (!player.hasBomb)
            {
                player.hasBomb = true;
            }
            else
            {
                i--;
            }
        }

        yield return null;
    }

    private IEnumerator StopPlayer(float stopTime){
        isPlayerMovable = false;
        yield return new WaitForSeconds(stopTime);
        isPlayerMovable = true;
    }
}
