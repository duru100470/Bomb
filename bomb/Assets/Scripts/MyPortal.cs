using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class MyPortal : MonoBehaviour
{
    [SerializeField] MyPortal oppositePortal;

    [Tooltip("Indicate which side will be player emits\nIf exitHeading is true, it means this portal emits player to right side of this portal")]
    public bool exitHeading;
    public Vector3 exitTransform;

    private void Start()
    {
        exitTransform = new Vector3(transform.position.x + (exitHeading? 1: -1) * 1f,transform.position.y,0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.CompareTag("Player") || other.transform.CompareTag("Projectile"))
        {
            if(NetworkManager.IsSceneActive("Play_Scene")) SoundManager.Instance.PlayAudio(AudioType.Portal);
            Vector3 vec = other.transform.position - transform.position;
            other.transform.position = oppositePortal.exitTransform + new Vector3(exitHeading ? vec.x : -vec.y , vec.y, 0);
            Rigidbody2D rigid = other.GetComponent<Rigidbody2D>();
            rigid.velocity = new Vector2((oppositePortal.exitHeading ? 1 : -1) * Mathf.Abs(rigid.velocity.x), rigid.velocity.y);
        }
    }
}
