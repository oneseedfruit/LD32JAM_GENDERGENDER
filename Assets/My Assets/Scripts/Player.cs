using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float movingForce;
    public Vector2 maxVelocity;

    public enum State { Idle, Walk, Jump, Fall }
    public State PlayerState;

    public enum Gender { Male, Female }
    public Gender PlayerGender;

    public Sprite spriteJumpGuy;
    public Sprite spriteJumpGirl;

    BoxCollider2D col2DPlayer;
    Rigidbody2D rb2DPlayer;
    public Animator animPlayer;
    SpriteRenderer srPlayer;
    Sprite spritePlayer;
    AudioSource audioPlayer;

    int jumpCount = 0;
    int pressCount = 0;
    bool makeNoise = false;

    void Awake ()
    {
        col2DPlayer = GetComponent<BoxCollider2D>();
        rb2DPlayer = GetComponent<Rigidbody2D>();
        rb2DPlayer.drag = 1f;
        animPlayer = GetComponent<Animator>();
        srPlayer = GetComponent<SpriteRenderer>();
        spritePlayer = srPlayer.sprite;
        audioPlayer = GetComponent<AudioSource>();
    }



	// Use this for initialization
	void Start () {
        StartCoroutine(Swap());
	}
	


	// Update is called once per frame
	void Update () {
	    if (!_Manager.isPaused && !_Manager.isGameOver)
        {
            #region SPRITE AND ANIMATION CONTROLS
            
            if (PlayerGender == Gender.Male)
                animPlayer.SetBool("isGuy", true);
            else
                animPlayer.SetBool("isGuy", false);
            animPlayer.enabled = true;
            switch (PlayerState)
            {
                case State.Idle:
                    animPlayer.enabled = true;
                    animPlayer.SetBool("isIdling", true);
                    if (PlayerGender == Gender.Male)
                        animPlayer.SetBool("isGuy", true);
                    else if (PlayerGender == Gender.Female)
                        animPlayer.SetBool("isGuy", false);
                    break;
                case State.Walk:
                    animPlayer.enabled = true;
                    animPlayer.SetBool("isIdling", false);
                    if (PlayerGender == Gender.Male)
                    {
                        animPlayer.SetBool("isGuy", true);
                        if (rb2DPlayer.velocity.y <= 0)
                        {
                            animPlayer.enabled = false;
                            srPlayer.sprite = spriteJumpGuy;
                        }
                    }
                    else if (PlayerGender == Gender.Female)
                    {
                        animPlayer.SetBool("isGuy", false);
                        if (rb2DPlayer.velocity.y <= 0)
                        {
                            animPlayer.enabled = false;
                            srPlayer.sprite = spriteJumpGirl;
                        }
                    }
                    break;
                case State.Jump:
                    animPlayer.enabled = false;
                    if (PlayerGender == Gender.Male)
                        srPlayer.sprite = spriteJumpGuy;
                    else if (PlayerGender == Gender.Female)
                        srPlayer.sprite = spriteJumpGirl;
                    break;
                case State.Fall:
                    animPlayer.enabled = false;
                    if (PlayerGender == Gender.Male)
                        srPlayer.sprite = spriteJumpGuy;
                    else if (PlayerGender == Gender.Female)
                        srPlayer.sprite = spriteJumpGirl;
                    break;
            }

            if (PlayerState == State.Jump && rb2DPlayer.velocity.y == 0)
            {
                rb2DPlayer.velocity = new Vector2(rb2DPlayer.velocity.x, -rb2DPlayer.velocity.y);
            }

            #endregion

            if (!audioPlayer.isPlaying && rb2DPlayer.velocity.y > 0f && PlayerState == State.Jump)
            {
                audioPlayer.loop = false;
                if (jumpCount == 0)
                    if (PlayerState != State.Walk)
                        if (srPlayer.sprite == spriteJumpGuy || srPlayer.sprite == spriteJumpGirl)
                            audioPlayer.Play();
                if (jumpCount == 1)
                    if (PlayerState != State.Walk)
                        if (srPlayer.sprite == spriteJumpGuy || srPlayer.sprite == spriteJumpGirl)
                            audioPlayer.Play();
            }
            else if (PlayerState != State.Jump)
                audioPlayer.Stop();
        }
	}



    void FixedUpdate ()
    {
        if (!_Manager.isPaused && !_Manager.isGameOver)
        {
            #region MOVEMENT CONTROLS
            
            if (Input.GetAxis("Jump") < 1 && Input.GetAxis("Horizontal") > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                if (Mathf.Abs(rb2DPlayer.velocity.x) <= Mathf.Abs(maxVelocity.x))
                    rb2DPlayer.AddForce(Vector2.right * movingForce);
                if (!Mathf.Approximately(rb2DPlayer.velocity.y, 0f) && !Mathf.Equals(rb2DPlayer.velocity.x, 0f))
                    PlayerState = State.Idle;
                else if (PlayerState == State.Idle && !Mathf.Approximately(rb2DPlayer.velocity.y, 0f) && !Mathf.Equals(rb2DPlayer.velocity.x, 0f))
                    PlayerState = State.Walk;
            }
            else if (Input.GetAxis("Jump") < 1 && Input.GetAxis("Horizontal") < 0)
            {
                transform.eulerAngles = new Vector3(0, 180f, 0);
                if (Mathf.Abs(rb2DPlayer.velocity.x) <= Mathf.Abs(maxVelocity.x))
                    rb2DPlayer.AddForce(-Vector2.right * movingForce);
                if (!Mathf.Approximately(rb2DPlayer.velocity.y, 0f) && !Mathf.Approximately(rb2DPlayer.velocity.x, 0f))
                    PlayerState = State.Idle;
                else if (PlayerState == State.Idle && !Mathf.Approximately(rb2DPlayer.velocity.y, 0f) && !Mathf.Equals(rb2DPlayer.velocity.x, 0f))
                    PlayerState = State.Walk;
            }
            else if (Input.GetAxis("Jump") > 0 && PlayerState != State.Jump && jumpCount < 2)
            {
                if (jumpCount < 2)
                {
                    rb2DPlayer.AddForce(Vector2.up * movingForce, ForceMode2D.Impulse);
                    PlayerState = State.Jump;
                }
                jumpCount++;
            }
            else if (rb2DPlayer.velocity.y <= -0.01f)
            {
                PlayerState = State.Fall;
            }
            else if (rb2DPlayer.velocity.x <= 0.001f && rb2DPlayer.velocity.y == 0f)
            {                
                PlayerState = State.Idle;
            }
            #endregion

            #region DETECTS PLATFORMS/WALLS
            RaycastHit2D[] hit = new RaycastHit2D[2];            
            Physics2D.CircleCastNonAlloc(col2DPlayer.bounds.center,
                                         0.5f * Vector2.Distance(col2DPlayer.bounds.max, col2DPlayer.bounds.min),
                                         -Vector2.up,
                                         hit);                                      
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider != null)
                {
                    if (hit[i].collider.tag == "WallPlatform" && rb2DPlayer.velocity.y == 0 && PlayerState != State.Jump)
                    {
                        jumpCount = 0;
                        break;
                    }
                }
            }
            #endregion
        }
    }
    
    IEnumerator Swap ()
    {
        while (true)
        {
            if (!_Manager.isPaused && !_Manager.isGameOver)
            {
                if (Input.GetAxis("Fire1") > 0 && PlayerState != State.Jump && PlayerState != State.Fall)
                {
                    for (int h = 0; h < 1; h++)
                    {
                        float i = 1.0f;
                        while (i > 0)
                        {
                            i -= 7f * Time.deltaTime;
                            srPlayer.color = new Color(srPlayer.color.r, srPlayer.color.g, srPlayer.color.b, i);
                            yield return new WaitForEndOfFrame();
                        }
                        while (i < 1.0f)
                        {
                            i += 7f * Time.deltaTime;
                            srPlayer.color = new Color(srPlayer.color.r, srPlayer.color.g, srPlayer.color.b, i);
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    if (Input.GetAxis("Fire1") > 0.9)
                    {
                        if (PlayerGender == Gender.Male)
                            animPlayer.SetBool("isGuy", false);
                        else
                            animPlayer.SetBool("isGuy", true);

                        yield return new WaitForEndOfFrame();
                        PlayerGender = PlayerGender == Gender.Male ? Gender.Female : Gender.Male;
                        
                    }

                    while (Input.GetAxis("Fire1") > 0)
                        yield return new WaitForEndOfFrame();
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
