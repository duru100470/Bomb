using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_PlayScene : NetworkBehaviour
{
    [Header("Log")]
    [SerializeField] RectTransform Panel_Log;
    [SerializeField] GameObject Panel_TransitionLog;
    [SerializeField] GameObject Panel_ExplosionLog;

    [Command(requiresAuthority = false)]
    public void CmdAddLogTransition(PlayerStateManager from, PlayerStateManager to)
    {
        GameObject obj = Instantiate(Panel_TransitionLog);
        obj.transform.GetChild(0).GetComponent<Text>().text = from.playerNickname;
        obj.transform.GetChild(2).GetComponent<Text>().text = to.playerNickname;
        obj.transform.SetParent(Panel_Log);
        NetworkServer.Spawn(obj);
        RpcSetLogTransition(obj.GetComponent<NetworkIdentity>().netId, from.playerNickname, to.playerNickname);
        StartCoroutine(SetLog(obj));
    }

    [Command(requiresAuthority = false)]
    public void CmdAddLogExplode(PlayerStateManager explosion)
    {
        GameObject obj = Instantiate(Panel_ExplosionLog);
        obj.transform.GetChild(0).GetComponent<Text>().text = explosion.playerNickname;
        obj.transform.SetParent(Panel_Log);
        NetworkServer.Spawn(obj);
        RpcSetLogExplosion(obj.GetComponent<NetworkIdentity>().netId, explosion.playerNickname);
        StartCoroutine(SetLog(obj));
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
}
