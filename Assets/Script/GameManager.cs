using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject boss;
    
    private static GameManager _instance;
    public PlayerController playerController;
    public WormController wormController;

    public static GameManager Instance => _instance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        DontDestroyOnLoad(this);
        playerController = player.GetComponent<PlayerController>();
        wormController = boss.GetComponent<WormController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
