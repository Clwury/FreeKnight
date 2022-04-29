using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallController : MonoBehaviour
{
    private GameObject _worm;
    private Animator _animator;
    private Collider2D _collider2D;
    // private Vector3 dir;

    public float speed = 5f;
    public float damage = 8f;
    public float lifeTime = 7f;

    // Start is called before the first frame update
    void Start()
    {
        _worm = GameObject.FindWithTag("Boss");
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        // dir = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
    }

    public void Move()
    {
        if (_worm.transform.localScale.x < 0)
        {
            transform.position += -transform.right * (speed * Time.deltaTime);
        }
        else
        {
            transform.position += transform.right * (speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            _animator.Play("Explosion");
            //@TODO player受伤
            
            _collider2D.enabled = false;
        }
        if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _animator.Play("Explosion");
            _collider2D.enabled = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
