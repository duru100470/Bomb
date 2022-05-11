using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spawner : NetworkBehaviour {
    public GameObject item_dash;
    public GameObject item_stone;

    public void Start(){
        SpawnItem(item_dash, new Vector3(Random.Range(-9f, 9f),-4f,0f));
        SpawnItem(item_stone, new Vector3(Random.Range(-9f, 9f),-4f,0f));
    }

    [Command(requiresAuthority = false)]
    public void SpawnItem(GameObject obj, Vector3 pos){
        GameObject go = Instantiate(obj, pos, Quaternion.identity);
        NetworkServer.Spawn(go);
    }

}
