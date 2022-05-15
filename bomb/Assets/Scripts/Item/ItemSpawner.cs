using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> itemList = new List<GameObject>();
    [SerializeField]
    private float spawnCoolDown = 5f;
    private float curSpawnCoolDown;
    public bool isSpawnable = true;
    private void Start(){
        curSpawnCoolDown = spawnCoolDown;
    }
    private void Update(){
        if(!isServer) return;
        if(!isSpawnable) return;
        curSpawnCoolDown += Time.deltaTime;
        if(curSpawnCoolDown > spawnCoolDown){
            curSpawnCoolDown = 0f;
            CmdSpawnRandomItem();
        }
    }
    
    [Command (requiresAuthority = false)]
    public void CmdSpawnRandomItem(){
        GameObject obj = Instantiate(itemList[Random.Range(0,itemList.Count)],transform.position,Quaternion.identity);
        if(obj == null) return;
        isSpawnable = false;
        obj.GetComponent<Item>().spawner = this;
        NetworkServer.Spawn(obj);
    }
}
