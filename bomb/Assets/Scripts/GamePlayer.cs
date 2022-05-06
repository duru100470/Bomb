using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GamePlayer : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody2D rigid;
    [SerializeField]
    private float moveSpeed = 1.0f;        

    private void Start() {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if(!isLocalPlayer)
            return;
        Move();
    }

    private void Move(){
        this.transform.position += new Vector3(Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime, Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime, 0.0f);
    }
}
