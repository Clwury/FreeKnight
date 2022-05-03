using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject player;
    public GameObject boss;
    public Image hpBar0;
    public Image hpBar1;
    public Image enemyHpBar;
    public float offSpeed;
    private PlayerController _playerController;
    private WormController _wormController;

    private void Awake()
    {
        _playerController = player.GetComponent<PlayerController>();
        _wormController = boss.GetComponent<WormController>();

        _playerController.UpdateHealthBar += UpdatePlayerHpBar;
        _wormController.UpdateHealthBar += UpdateEnemyHpBar;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdatePlayerHpBar(float currentHp, float maxHp)
    {
        hpBar0.rectTransform.sizeDelta = new Vector2(currentHp / maxHp * 49, 3);
        StartCoroutine(DecreaseHp(currentHp / maxHp * 49));
    }
    
    private void UpdateEnemyHpBar(float currentHp, float maxHp)
    {
        enemyHpBar.rectTransform.sizeDelta = new Vector2(currentHp / maxHp * 220, 8);
    }

    IEnumerator DecreaseHp(float hp)
    {
        float rectWidth = hpBar1.rectTransform.rect.width;
        while (rectWidth > hp)
        {
            rectWidth -= Time.deltaTime * offSpeed;
            hpBar1.rectTransform.sizeDelta = new Vector2(rectWidth, 3);
            yield return null;
        }
    }
}
