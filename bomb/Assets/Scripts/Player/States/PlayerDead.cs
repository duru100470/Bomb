using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDead : IState
{
    private PlayerStateManager player;

    public PlayerDead(PlayerStateManager player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        player.CmdSetGhostSprite(true);
        player.gameObject.layer = LayerMask.NameToLayer("GhostPlayer");
        player.rigid2d.gravityScale = 0;
        player.rigid2d.velocity = Vector2.zero;
        player.spriteRenderer.material.color = new Color(player.spriteRenderer.material.color.r, player.spriteRenderer.material.color.g, player.spriteRenderer.material.color.b, 0.5f);
    }
    public void OperateExit()
    {
        player.CmdSetGhostSprite(false);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        player.rigid2d.gravityScale = 1f;
        player.spriteRenderer.material.color = new Color(player.spriteRenderer.material.color.r, player.spriteRenderer.material.color.g, player.spriteRenderer.material.color.b, 1f);
    }
    public void OperateUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        player.transform.position += new Vector3(horizontal, vertical, 0) * Time.deltaTime * player.GhostSpeed;
    }
}