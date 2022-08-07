using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_PlayScene : NetworkBehaviour
{
    [SerializeField] RectTransform Panel_Winner;
    [SerializeField] RectTransform Panel_LeaderBoard;
    [SerializeField] GameObject SeparatorPrefab;
    [SerializeField] GameObject LeaderBoardIconPrefab;
    public GameObject Panel_Loading;
    int roundCount;
    List<PlayerStateManager> players;
    List<GameObject> leaderBoardIcon = new List<GameObject>();

    [Header("Log")]
    [SerializeField] RectTransform Panel_Log;
    [SerializeField] GameObject Panel_TransitionLog;
    [SerializeField] GameObject Panel_ExplosionLog;

    [Header("ESC")]
    [SerializeField] RectTransform Panel_ESC;
    [SerializeField] RectTransform Panel_Option;


    public void Start() 
    {
        roundCount = GameRuleStore.Instance.CurGameRule.roundWinningPoint;
        players = GameManager.Instance.GetPlayerList();
    }

    public void InitializeLeaderBoard()
    {
        players = GameManager.Instance.GetPlayerList();
        for(int i=0; i<roundCount; i++)
        {
            GameObject obj = Instantiate(SeparatorPrefab, Panel_LeaderBoard);
            RectTransform rectT = obj.GetComponent<RectTransform>();
            rectT.position = new Vector3(Screen.width/2, Screen.height * (i+1) / (roundCount+1), 0);
        }

        Debug.Log(players.Count);
        for(int i=0; i< players.Count; i++)
        {
            GameObject obj = Instantiate(LeaderBoardIconPrefab, Panel_LeaderBoard);
            obj.GetComponent<Image>().sprite = players[i].LeaderBoardIcon;
            leaderBoardIcon.Add(obj);
            RectTransform rectT = obj.GetComponent<RectTransform>();
            rectT.position = new Vector3(Screen.width * (i+1) / (players.Count+1), Screen.height / (roundCount+1) / 2, 0);
        }
    }

    public void ActivateESC()
    {
        if(Panel_Option.gameObject.activeInHierarchy) return;
        Panel_ESC.gameObject.SetActive(true);
    }

    public void OnClickButtonResume()
    {
        Panel_ESC.gameObject.SetActive(false);
    }

    public void SetLeaderBoard(PlayerStateManager winner, int state)
    {
        for(int i=0; i<players.Count; i++)
        {
            if(winner == players[i])
            {
                RpcSetLeaderBoard(i, state);
                break;
            }
        }
    }

    private IEnumerator UpdateLeaderBoard(int index, int state)
    {
        yield return new WaitForSeconds(1f);
        Panel_LeaderBoard.gameObject.SetActive(true);
        float curTime = 0;
        RectTransform rectT = leaderBoardIcon[index].GetComponent<RectTransform>();
        players = GameManager.Instance.GetPlayerList();
        for(int i=0; i<leaderBoardIcon.Count; i++)
        {
            leaderBoardIcon[i].GetComponentInChildren<Text>().text = players[i].playerNickname;
        }
        while(curTime < 2f)
        {
            rectT.position = new Vector3(rectT.position.x, rectT.position.y + Screen.height / (roundCount+1) * (Time.deltaTime / 2f) ,0);
            curTime += Time.deltaTime;
            yield return null;
        }
        rectT.position = new Vector3(rectT.position.x, Screen.height / (roundCount+1) * (players[index].roundScore + 0.5f) ,0);
        yield return new WaitForSeconds(1f);
        Panel_LeaderBoard.gameObject.SetActive(false);
        if(state == 1 && isServer) RpcSetWinnerBoard(players[index].playerNickname);
    }

    public IEnumerator SetLog(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        float curTime = 0f;
        float dippuseTime = 1f;
        var texts = obj.GetComponentsInChildren<Text>();
        var images = obj.GetComponentsInChildren<Image>();
        while(curTime < dippuseTime)
        {
            if(obj == null) yield break;
            foreach(var text in texts) text.color = new Color(0f, 0f, 0f, 1 - curTime/dippuseTime);
            foreach(var image in images) image.color = new Color(1f, 1f, 1f, 1 - curTime/dippuseTime);
            curTime += Time.deltaTime;
            yield return null;
        }
        if(obj != null) Destroy(obj);
    }

    [Command(requiresAuthority = false)]
    public void CmdAddLogTransition(PlayerStateManager from, PlayerStateManager to)
    {
        GameObject obj = Instantiate(Panel_TransitionLog, Panel_Log);
        obj.transform.GetChild(0).GetComponent<Text>().text = from.playerNickname;
        obj.transform.GetChild(2).GetComponent<Text>().text = to.playerNickname;
        //obj.transform.SetParent(Panel_Log);
        NetworkServer.Spawn(obj);
        RpcSetLogTransition(obj.GetComponent<NetworkIdentity>().netId, from.playerNickname, to.playerNickname);
        StartCoroutine(SetLog(obj));
    }

    [Command(requiresAuthority = false)]
    public void CmdAddLogExplode(PlayerStateManager explosion)
    {
        GameObject obj = Instantiate(Panel_ExplosionLog, Panel_Log);
        obj.transform.GetChild(0).GetComponent<Text>().text = explosion.playerNickname;
        //obj.transform.SetParent(Panel_Log);
        NetworkServer.Spawn(obj);
        RpcSetLogExplosion(obj.GetComponent<NetworkIdentity>().netId, explosion.playerNickname);
        StartCoroutine(SetLog(obj));
    }

    [ClientRpc]
    public void RpcSetLogTransition(uint netId, string from, string to)
    {
        if(isClient) 
        {
            GameObject obj = NetworkClient.spawned[netId].gameObject;
            obj.transform.GetChild(0).GetComponent<Text>().text = from;
            obj.transform.GetChild(2).GetComponent<Text>().text = to;
            obj.transform.SetParent(Panel_Log);
            StartCoroutine(SetLog(obj));
        }
    }

    [ClientRpc]
    public void RpcSetLogExplosion(uint netId, string name)
    {
        if(isClient) 
        {
            GameObject obj = NetworkClient.spawned[netId].gameObject;
            obj.transform.GetChild(0).GetComponent<Text>().text = name; 
            obj.transform.SetParent(Panel_Log);
            StartCoroutine(SetLog(obj));
        }
    }
    
    [ClientRpc]
    public void RpcSetLeaderBoard(int index, int state)
    {
        StartCoroutine(UpdateLeaderBoard(index, state));   
    }

    [ClientRpc]
    public void RpcSetWinnerBoard(string name)
    {
        Panel_Winner.gameObject.SetActive(true);
        Panel_Winner.GetComponentInChildren<Text>().text = name + "\nWin!";
    }
}
