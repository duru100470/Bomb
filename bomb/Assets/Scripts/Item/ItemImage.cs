using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemImage : NetworkBehaviour
{
    private PlayerStateManager player;

    public void AddPlayer(PlayerStateManager _player)
    {
        player = _player;
    }

    private void Update()
    {
        Vector3 target = player.transform.position + new Vector3((player.isHeadingRight?-1:1) * .5f, .2f, 0f);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 7f);
    }

}
