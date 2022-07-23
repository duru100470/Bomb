using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> itemList = new List<GameObject>();
    [SerializeField] private float spawnCoolDown = 5f;
    private float curSpawnCoolDown;
    public bool isSpawnable = true;
    private void Start()
    {
        curSpawnCoolDown = spawnCoolDown;
    }
    private void Update()
    {
        if (!isServer || !isSpawnable) return;
        curSpawnCoolDown += Time.deltaTime;
        if (curSpawnCoolDown > spawnCoolDown)
        {
            curSpawnCoolDown = 0f;
            CmdSpawnRandomItem();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnRandomItem()
    {
        GameObject toSpawn = itemList[Random.Range(0, itemList.Count)];
        GameObject obj = Instantiate(toSpawn, transform.position + new Vector3(0, 0.35f,0), Quaternion.identity);
        if (obj != null)
        {
            isSpawnable = false;
            obj.GetComponent<Item>().spawner = this;
            NetworkServer.Spawn(obj);
        }
    }
}
