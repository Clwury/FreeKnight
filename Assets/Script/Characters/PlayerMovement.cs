using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public Vector2 bottomOffset;
    public LayerMask groundLayerMask;
    public float xVelocity;
    public float scaleX;
    public bool onGround;
    public bool wallSide;
    
    private Rigidbody2D _rb;
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 3;
    }

    // Update is called once per frame
    void Update()
    {
        xVelocity = Input.GetAxis("Horizontal");
        if (xVelocity > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (xVelocity < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        scaleX = transform.localScale.x;
        _rb.velocity = new Vector2(xVelocity * speed, _rb.velocity.y);
    }
    
    void RayCheck()
    {
        onGround = Physics2D.Raycast((Vector2)transform.position - scaleX * bottomOffset, Vector2.down, 0.1f, groundLayerMask);
        Debug.DrawRay(transform.position + new Vector3(-scaleX * bottomOffset.x, bottomOffset.y, 0), Vector3.down * 0.1f, onGround ? Color.red : Color.green);

        wallSide = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0.2f * scaleX, 0.9f), 0.1f, groundLayerMask);
    }
}
