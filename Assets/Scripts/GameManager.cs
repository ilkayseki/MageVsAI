using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager m_GameManager;
    public GameObject panel;

    public GameObject [] _spawnPoits;

    private void Awake()
    {
        if (m_GameManager == null)
        {
            m_GameManager = this;
        }
        else
        {
            Destroy(this);
        }

        _spawnPoits = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }

    public void EndGame()
    {
        Time.timeScale= 0;
        Cursor.visible = true;
        panel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Revive(string dead)
    {
        int rand = Random.Range(0, _spawnPoits.Length);
        switch (dead)
        {
            case "Player":
                GameObject.FindWithTag("Player").GetComponent<Transform>().position =
                    _spawnPoits[rand].transform.position;
                break;
                    
            case "Enemy":
                GameObject.FindWithTag("Enemy").GetComponent<Transform>().position =
                    _spawnPoits[rand].transform.position;
                break;
        }
        
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("playerCount",9);
        PlayerPrefs.SetInt("enemyCount", 9);

    }
}
