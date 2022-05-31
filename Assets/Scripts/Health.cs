using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Health : MonoBehaviour
{
    public int health = 100;


    public static int _playerCount;
    public static int _enemyCount;
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("playerCount") || !PlayerPrefs.HasKey("enemyCount"))
        {
            PlayerPrefs.SetInt("playerCount",10);
            PlayerPrefs.SetInt("enemyCount",10);
            Debug.Log("yok");
        }
        else
        {
            _playerCount = PlayerPrefs.GetInt("playerCount");
            _enemyCount = PlayerPrefs.GetInt("enemyCount");
            Debug.Log("var");
        }
        Debug.Log(_playerCount);
    }

    public void TakeDamage(int amount, string aggressor)
        {
            if (health < amount)
            {
                switch (aggressor)
                {
                    case "Player":
                        if (_playerCount != 0)
                        {
                            _playerCount = _playerCount - 1;
                            PlayerPrefs.SetInt("playerCount",_playerCount);
                            health = 100;
                            GameManager.m_GameManager.Revive("Player");
                        }
                        else
                        {
                            GameManager.m_GameManager.EndGame();
                        }
                        break;
                    
                    case "Enemy":
                        if (_enemyCount != 0)
                        {
                            _enemyCount = _playerCount - 1;
                            PlayerPrefs.SetInt("enemyCount",_enemyCount);
                            health = 100;
                            GameManager.m_GameManager.Revive("Enemy");
                        }
                        else
                        {
                            GameManager.m_GameManager.EndGame();
                        }
                    
                        break;
                }
            }
            
            else
            {
                health -= amount;
                Debug.Log(health);
            }
            
        }
}