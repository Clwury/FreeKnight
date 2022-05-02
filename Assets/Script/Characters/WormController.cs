using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour
{
    public enum WormState
    {
        Idle,
        Walk,
        Dash,
        BeHit,
        FireBall,
        PowerFireBall,
        Death
    }
    
    // 近身距离
    public float closeDistance;
    // 攻击概率
    public float attackChance;
    // 行走速度
    public float walkSpeed;
    // 冲击距离
    public float dashDistance;
    // 冲击时间
    public float dashTime;
    public GameObject fireBallPrefab;

    private WormState _state;
    private Animator _animator;
    private float _hp;
    private bool _isDead;
    // 是否二阶段
    private GameObject _player;
    private bool _powerState;
    private bool _isIdle;
    private bool _isWalk;
    private bool _isDash;
    // private bool _isAttack;
    private Vector3 _currentPos;
    private float _dashDuration;
    

    // Start is called before the first frame update
    void Start()
    {
        _isIdle = true;
        _state = WormState.Idle;
        _hp = 100;
        _player = GameObject.FindWithTag("Player");
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        SwitchState();
        SwitchAnimation();
    }

    public void SwitchState()
    {
        switch (_state)
        {
            case WormState.Idle:
                float value = Random.Range(0, 1f);
                _isIdle = false;
                Debug.Log(value);
                if (value >= attackChance)
                {
                    _state = _powerState ? WormState.PowerFireBall : WormState.FireBall;
                }
                else
                {
                    _state = WormState.Walk;
                }
                break;
            case WormState.Walk:
                Walk();
                break;
            case WormState.Dash:
                DashSkill();
                break;
            case WormState.FireBall:
                FireBallSkill();
                break;
            case WormState.PowerFireBall:
                PowerFireBallSkill();
                break;
            case WormState.BeHit:
                BeHit();
                break;
            case WormState.Death:
                BeDead();
                break;
        }
    }

    public void SwitchAnimation()
    {
        _animator.SetBool("isIdle", _isIdle);
        _animator.SetBool("isWalk", _isWalk);
        _animator.SetBool("isDash", _isDash);
    }

    public void LookAtPlayer()
    {
        Vector3 distance = _player.transform.position - transform.position;
        transform.localScale = new Vector3(distance.x < 0 ? -1 : 1, 1, 1);
    }

    public void Walk()
    {
        // Debug.Log("walk...");
        _isWalk = true;
        LookAtPlayer();
        // 与player距离小于近身距离
        if (Mathf.Abs(_player.transform.position.x - transform.position.x) < closeDistance)
        {
            _isWalk = false;
            _state = WormState.Dash;
            _currentPos = transform.position;
        }
        else
        {
            transform.position += new Vector3(transform.localScale.x * Time.deltaTime * walkSpeed, 0, 0);
        }
    }
    
    public void DashSkill()
    {
        // Debug.Log("Dash...");
        _isDash = true;
        LookAtPlayer();
        // Debug.Log(_dashDuration);
        if (_dashDuration >= dashTime)
        {
            _isDash = false;
            _state = _powerState ? WormState.PowerFireBall : WormState.FireBall;
            return;
        }
        // Debug.Log($"{_currentPos}, {_currentPos + new Vector3(transform.localScale.x * dashDistance, 0, 0)}, {transform.position}");
        transform.position = Vector3.Lerp(_currentPos, _currentPos + new Vector3(transform.localScale.x * dashDistance, 0, 0),
            _dashDuration / dashTime);
        _dashDuration += Time.deltaTime;
    }

    public void FireBallSkill()
    {
        LookAtPlayer();
        _animator.SetTrigger("Attack");
    }

    public void PowerFireBallSkill()
    {
        
    }

    public void SprayFireBall()
    {
        if (_powerState)
        {
            
        }
        else
        {
            StartCoroutine(InitFireBall());
        }
    }

    IEnumerator InitFireBall()
    {
        Vector3 firePoint = transform.Find("FirePoint").position;
        int fireBallCount = 3;
        while (fireBallCount > 0)
        {
            Instantiate(fireBallPrefab, firePoint, Quaternion.identity, transform);
            fireBallCount--;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void BeHit()
    {
        
    }

    public void BeDead()
    {
        
    }
}
