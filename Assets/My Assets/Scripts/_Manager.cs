using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class _Manager : MonoBehaviour {

    public static bool isPaused = false;
    public static int totalCharmed = 0;
    public static int totalEnemies = 0;
    bool pausedKeyPressed = false;
    public Enemy[] Enemies;
    
    public Vector2 minPos;
    public Vector2 maxPos;

    public Player player;
    public Text textScore;
    public Text textTimer;
    public Text textGameOver;

    public static bool isGameOver = false;

    Enemy spawn;
    public static List<Enemy> enemyList = new List<Enemy>();
    public int maxTime;

    float initTime;
    float timer;
    int countDown = 30;

    void Awake ()
    {
        initTime = Time.time;
        countDown = maxTime;
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(GameOverControl());
	}
	
	// Update is called once per frame
	void Update () {
        timer = Time.time - initTime;
        countDown = maxTime - (int)timer;
        textTimer.text = "" + (countDown < 10 ? "0" : "") + countDown;

        if (countDown == 0)
            isGameOver = true;

        if (!isGameOver)
        {
            int chance = Random.Range(0, 1);
            if (isPaused)
            {
                Time.timeScale = 0;
                //Debug.Log("Paused.");            
            }
            else
            {
                Time.timeScale = 1;
                //Debug.Log("Resumed.");            

                if (totalEnemies <= 15 && Random.value > Random.value / 2)
                {
                    Spawn(spawn);
                }
            }

            if (totalCharmed > 0)
                textScore.text = " " + totalCharmed;
        }
	}

    void Spawn(Enemy spawn)
    {
        if (!_Manager.isPaused && !_Manager.isGameOver)
        {
            for (int i = 0; i < Enemies.Length; i++)
            {
                if (Random.value > 0.5f)
                    spawn = Enemies[i];
            }

            GameObject checkCol = new GameObject();
            Collider2D col = checkCol.AddComponent<BoxCollider2D>();

            Collider2D[] hit = new Collider2D[2];
            Physics2D.OverlapAreaNonAlloc(col.bounds.center, col.bounds.extents, hit);

            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].gameObject != null)
                {
                    if (hit[i].tag == "WallPlatform")
                    {
                        checkCol.transform.position = new Vector2(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y));
                    }
                }
            }

            if (enemyList.Count > 0)
            {
                if (enemyList.Count > 0 && enemyList[enemyList.Count - 1] != null)
                {
                    if (Vector2.Distance(enemyList[enemyList.Count - 1].transform.position, checkCol.transform.position) >= 10f)
                    {
                        if (spawn != null)
                        {
                            Enemy spawned = Instantiate(spawn, checkCol.transform.position, Quaternion.identity) as Enemy;
                            spawned.StalkingTarget = player;
                            enemyList.Add(spawned);
                        }
                    }
                }
            }
            else
            {
                if (spawn != null)
                {
                    Enemy spawned = Instantiate(spawn, checkCol.transform.position, Quaternion.identity) as Enemy;
                    spawned.StalkingTarget = player;
                    enemyList.Add(spawned);
                }
            }

            Destroy(checkCol.gameObject);
        }
    }

    IEnumerator GameOverControl()
    {
        while (true)
        {
            if (!isGameOver)
            {
            }
            else
            {
                Time.timeScale = 0;
                textTimer.text = (countDown < 10 ? "0" : "") + countDown + " < TIMES UP";
                textGameOver.text = "GAME OVER \n <PRESS ANY KEY TO CONTINUE>";
                isGameOver = false;

                if (Input.anyKey)
                {
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    StartCoroutine(LoadLevel());
                    yield break;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator LoadLevel ()
    {
        while (true)
        {
            if (Input.anyKey)
            {
                totalCharmed = 0;
                Application.LoadLevel("Splash");
                textGameOver.text = "";
                isGameOver = false;
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }        
    }
}
