using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private RoomManager manager = NetworkManager.singleton as RoomManager;
    public UI_PlayScene UI_Play;
    [SerializeField] private List<Transform> spawnTransforms = new List<Transform>();
    //전체 플레이어 인원 리스트
    [SerializeField] private List<PlayerStateManager> players = new List<PlayerStateManager>();
    //생존 플레이어 인원 리스트
    [SerializeField] private List<PlayerStateManager> alivePlayers = new List<PlayerStateManager>();
    [SerializeField] public List<Sprite> itemSprites = new List<Sprite>();

    private float maxBombGlobalTime;
    private float minBombGlobalTime;
    [SyncVar][SerializeField] public float bombGlobalTimeLeft;
    [SyncVar] public float bombGlobalTime;
    [SyncVar] private int curBombGlobalTime;
    private int roundWinningPoint;
    [SyncVar] public bool isPlayerMovable = true;
    [SyncVar] public bool isBombDecreasable = true;

    private void Awake()
    {
        Instance = this;
        UI_Play = (UI_PlayScene)FindObjectOfType(typeof(UI_PlayScene));
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

    public void bombExplode(PlayerStateManager deadPlayer)
    {
        alivePlayers.Remove(deadPlayer);
        UI_Play.CmdAddLogExplode(deadPlayer);
        StartCoroutine(StopBombTimer(4f));
        if(alivePlayers.Count <= GameRuleStore.Instance.CurGameRule.bombCount) {
            PlayerStateManager winner = alivePlayers[0];
            winner.roundScore += 1;
            Debug.Log(winner.playerNickname + " now have " + winner.roundScore);
            if(winner.roundScore >= roundWinningPoint)
            {
                //최종 라운드 승리자 생기는 경우
                Debug.Log("Round End!");
                UI_Play.SetLeaderBoard(winner, 1);
                StartCoroutine(RoundEnd());
            }
            else
            {
                //점수는 얻었지만 라운드 승리자가 없는 경우
                UI_Play.SetLeaderBoard(winner, 0);
                Debug.Log(winner.playerNickname + " get point!");
                StartCoroutine(RoundReset());
            }
        }
        else
        { 
            //아직 두명이상이 생존해 있는 경우
            StartCoroutine(BombRedistribution(deadPlayer.transform.position));
        }
    }
    
    // 게임이 시작될 시 실행되는 코루틴
    private IEnumerator GameReady()
    {
        isPlayerMovable = false;
        
        while(PlayerSetting.playerNum != players.Count)
        {
            yield return null;
        }
        //레이턴시 감안 로딩 텀
        RpcSetLeaderBoard();
        yield return new WaitForSeconds(.5f);

        alivePlayers = players.ToList();

        //기본 시간설정
        bombGlobalTime = bombGlobalTimeLeft = Mathf.Round(Random.Range(minBombGlobalTime, maxBombGlobalTime));

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

        isPlayerMovable = true;
        
        yield return null;
    }

    //생존자 수에 따른 시간과 폭탄 재분배
    private IEnumerator BombRedistribution(Vector3 explosionPos)
    {
        StartCoroutine(StopPlayer(3f));

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
        CmdUpdateBombState(maxPlayer);

        yield return null;
    }

    private IEnumerator RoundEnd()
    {
        yield return new WaitForSeconds(7f);
        manager.ServerChangeScene(manager.RoomScene);
    }

    private IEnumerator StopPlayer(float stopTime){
        isPlayerMovable = false;
        yield return new WaitForSeconds(stopTime);
        isPlayerMovable = true;
    }

    private IEnumerator StopBombTimer(float stopTime)
    {
        isBombDecreasable = false;
        yield return new WaitForSeconds(stopTime);
        isBombDecreasable = true;
    }

    private IEnumerator RoundReset()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(StopPlayer(3f));
        bombGlobalTime = bombGlobalTimeLeft = Mathf.Round(Random.Range(minBombGlobalTime, maxBombGlobalTime));

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
                player.CheckBombState();
            }
            else
            {
                i--;
            }
        }
        yield return null;
    }

    [Command]
    public void CmdUpdateBombState(PlayerStateManager player)
    {
        player.CheckBombState();
    }

    [ClientRpc]
    public void RpcSetLeaderBoard()
    {
        StartCoroutine(UI_Play.DisplayLoadingPanel(UI_Play.InitializeLeaderBoard()));
    }
}
