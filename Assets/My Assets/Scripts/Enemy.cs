using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    public Player StalkingTarget;

    public enum State { NotCharmed, GettingCharmed, Charmed }
    public State EnemyState;

    public enum Gender { Male, Female };
    public Gender EnemyGender;

    Transform trEnemy;
    BoxCollider2D colEnemy;
    Rigidbody2D rb2DEnemy;
    SpriteRenderer srEnemy;
    Animator animEnemy;
    AudioSource audioEnemy;


    void Awake ()
    {
        EnemyState = State.NotCharmed;
        trEnemy = GetComponent<Transform>();
        colEnemy = GetComponent<BoxCollider2D>();
        rb2DEnemy = GetComponent<Rigidbody2D>();
        srEnemy = GetComponent<SpriteRenderer>();
        animEnemy = GetComponent<Animator>();
        audioEnemy = GetComponent<AudioSource>();
        _Manager.totalEnemies++;
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(DisappearWhenCharmed());
	}
	
	// Update is called once per frame
	void Update () {
        if (!_Manager.isPaused && !_Manager.isGameOver)
        {
            if (tag == "EnemyGuy")
                EnemyGender = Gender.Male;
            else if (tag == "EnemyGirl")
                EnemyGender = Gender.Female;

            if (EnemyState != State.Charmed)
            {
                if (StalkingTarget.transform.position.x > trEnemy.position.x)
                    trEnemy.eulerAngles = new Vector3(trEnemy.eulerAngles.x, 180f, trEnemy.eulerAngles.z);
                else if (StalkingTarget.transform.position.x < trEnemy.position.x)
                    trEnemy.eulerAngles = new Vector3(trEnemy.eulerAngles.x, 0f, trEnemy.eulerAngles.z);

                if (rb2DEnemy.velocity.y >= 0 && rb2DEnemy.drag <= 100f)
                    rb2DEnemy.drag += 10f * Time.deltaTime;
            }
            
            if (EnemyState == State.Charmed)
            {                
                colEnemy.enabled = false;
                rb2DEnemy.gravityScale = 0;
                rb2DEnemy.mass = 0;
                rb2DEnemy.drag = 0;
                rb2DEnemy.Sleep();
            }
            else if (EnemyState == State.GettingCharmed)
            {
                animEnemy.SetTrigger("GettingCharmed");
                EnemyState = State.Charmed;
            }
            else if (EnemyState == State.NotCharmed && Vector2.Distance(StalkingTarget.transform.position, trEnemy.position) <= 2.5f && Mathf.Abs(StalkingTarget.transform.position.y - trEnemy.position.y) <= 0.1f)
            {
                if (EnemyGender == Gender.Female && StalkingTarget.PlayerGender == Player.Gender.Male && StalkingTarget.animPlayer.GetBool("isGuy"))
                {
                    EnemyState = State.GettingCharmed;
                    audioEnemy.Play();
                }
                if (EnemyGender == Gender.Male && StalkingTarget.PlayerGender == Player.Gender.Female && !StalkingTarget.animPlayer.GetBool("isGuy"))
                {
                    EnemyState = State.GettingCharmed;
                    audioEnemy.Play();
                }
            }
        }
	}

    void OnDestroy ()
    {
        _Manager.totalEnemies--;
        _Manager.enemyList.Remove(_Manager.enemyList[_Manager.enemyList.Count - 1]);
    }

    IEnumerator DisappearWhenCharmed ()
    {
        while(true)
        {
            if (!_Manager.isPaused)
            {
                if (EnemyState == State.Charmed)
                {
                    if (Mathf.Approximately(transform.eulerAngles.y, 0f)) // left
                    {
                        float i = 1.0f;
                        while (i > 0)
                        {
                            i -= 0.6f * Time.deltaTime;
                            trEnemy.position = new Vector2(trEnemy.position.x - 0.01f * i, trEnemy.position.y);
                            srEnemy.color = new Color(srEnemy.color.r, srEnemy.color.g, srEnemy.color.b, i);
                            yield return new WaitForEndOfFrame();

                        }
                    }
                    else if (Mathf.Approximately(transform.eulerAngles.y, 180f)) // right
                    {
                        float i = 1.0f;
                        while (i > 0)
                        {
                            i -= 0.6f * Time.deltaTime;
                            trEnemy.position = new Vector2(trEnemy.position.x + 0.01f * i, trEnemy.position.y);
                            srEnemy.color = new Color(srEnemy.color.r, srEnemy.color.g, srEnemy.color.b, i);
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    if (srEnemy.color.a <= 0f)
                    {
                        Destroy(this.gameObject);
                        _Manager.totalCharmed++;
                        yield break;
                    }
                }
            }
            yield return new WaitForEndOfFrame();    
        }        
    }
}
