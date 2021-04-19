using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 move;
    public float forwardSpeed;
    public float maxSpeed;

    private int desiredLane = 1;//0:left, 1:middle, 2:right
    public float laneDistance = 2.5f;//The distance between tow lanes

    public bool isGrounded;
    public LayerMask groundLayer;
    public Transform groundCheck;

    public float gravity = -12f;
    public float jumpHeight = 2;
    private Vector3 velocity;

    public Animator animator;
    private bool isSliding = false;

    public float slideDuration = 1.5f;

    bool toggle = false;

    Thread receiveThread; 
    UdpClient client; 
    int port;
    bool spacebarPressed = true;

    // TODO: Remove this
    int previousActionIndex = 0;
    int currentTile = 0;
    bool timerCanStart = false;
    bool timerFinished = false;
    bool correct = true;

    private int previousIndex;

    private System.Timers.Timer actionTimer;

    private string text = "";

    void Start()
    {
        port = 5065;
        // spacebarPressed = false; 
        controller = GetComponent<CharacterController>();
        Time.timeScale = 1.2f;

        if (CameraController.firstRun)
        {
            CameraController.firstRun = false;
            InitUDP();
        }

        text = "";
        previousIndex = 0;
        correct = true;
    }

    private void InitUDP()
    {
        print("UDP Initialized");

        receiveThread = new Thread(new ThreadStart(ReceiveData)); //1 
        receiveThread.IsBackground = true; //2
        receiveThread.Start(); //3

    }

    private void ReceiveData()
    {
        client = new UdpClient(port); 
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port); 
        while (true) 
        {
            try
            {
                //IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port); 
                byte[] data = client.Receive(ref anyIP); 

                text = Encoding.UTF8.GetString(data); 
                // Debug.Log(">> " + text + ">>" + System.DateTime.Now.ToLongTimeString());

                // spacebarPressed = true; 



            }
            catch (Exception e)
            {
                print(e.ToString()); 
            }
        }
    }
    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;

        //Increase Speed
        //if (toggle)
        //{
        //    toggle = false;
        //    if (forwardSpeed < maxSpeed)
        //        forwardSpeed += 0.1f * Time.fixedDeltaTime;
        //}
        //else
        //{
        //    toggle = true;
        //    if (Time.timeScale < 2f)
        //        Time.timeScale += 0.005f * Time.fixedDeltaTime;
        //}
    }

    void Update()
    {
        if (!PlayerManager.isGameStarted || PlayerManager.gameOver)
            return;


        // Move this piece up the receiveData() to finish
        //if (previousLetter != TileManager.letters[TileManager.letters.Count - 2])
        //{
        //    previousLetter = TileManager.letters[TileManager.letters.Count - 2];
        //    correct = false;
        //}

        Debug.Log(TileManager.letters.Count);
        if (previousIndex != TileManager.letters.Count)
        {
            previousIndex = TileManager.letters.Count;
            correct = false;
        }

        if (correct == false)
        {
            Debug.Log(TileManager.letters[previousIndex - 2].ToLower() + " == " + text.ToLower());
            if (TileManager.letters[previousIndex - 2].ToLower() == "") 
            {
                correct = true;
            }
                if (text.ToLower().Equals(TileManager.letters[previousIndex - 2].ToLower()))
            {
                correct = true;
            }
        }


        animator.SetBool("isGameStarted", true);
        move.z = forwardSpeed;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.17f, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
            velocity.y = -1f;

        if (isGrounded)
        {
            if (spacebarPressed == true || Input.GetKey("w") || Input.GetKey("space"))
            {
                //Jump();
                smartMove();
                //spacebarPressed = false;
            }

            if ((spacebarPressed == true && !isSliding) || (Input.GetKey("s") && !isSliding) || (Input.GetKey("space") && !isSliding))
            {
                smartMove();
                //spacebarPressed = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            if (SwipeManager.swipeDown && !isSliding)
            {
                smartMove();
                velocity.y = -10;
            }

        }
        controller.Move(velocity * Time.deltaTime);

        //Gather the inputs on which lane we should be
        if (spacebarPressed == true || Input.GetKeyDown("d") || Input.GetKey("space"))
        {
            /*
            desiredLane++;
            if (desiredLane == 3)
                desiredLane = 2;*/
            smartMove();
            //spacebarPressed = false;
        }
        if (spacebarPressed == true || Input.GetKeyDown("a") || Input.GetKey("space"))
        {
            /*
            desiredLane--;
            if (desiredLane == -1)
                desiredLane = 0;*/
            smartMove();
            //spacebarPressed = false;
        }

        //Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;
        if (desiredLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * laneDistance;

        //transform.position = targetPosition;
        if (transform.position != targetPosition)
        {
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 30 * Time.deltaTime;
            if (moveDir.sqrMagnitude < diff.magnitude)
                controller.Move(moveDir);
            else
                controller.Move(diff);
        }

        controller.Move(move * Time.deltaTime);
    }

    private void Jump()
    {
        StopCoroutine(Slide());
        animator.SetBool("isSliding", false);
        animator.SetTrigger("jump");
        controller.center = Vector3.zero;
        controller.height = 2;
        isSliding = false;

        velocity.y = Mathf.Sqrt(jumpHeight * 2 * -gravity);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.tag == "Obstacle")
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().PlaySound("GameOver");
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);
        yield return new WaitForSeconds(0.25f / Time.timeScale);
        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;

        yield return new WaitForSeconds((slideDuration - 0.25f) / Time.timeScale);

        animator.SetBool("isSliding", false);

        controller.center = Vector3.zero;
        controller.height = 2;

        isSliding = false;
    }

    private void smartMove()
    {
        currentTile = TileManager.tileTypes.Count;
        if (currentTile != previousActionIndex)
        {
            previousActionIndex = currentTile;
            timerCanStart = true;
            GameObject.Find("Letter").GetComponent<Text>().text = TileManager.letters[TileManager.letters.Count - 2];
        }
        switch (TileManager.tileTypes[TileManager.tileTypes.Count - 2])
        {
            case 0:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 2;
                }
                break;
            case 1:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 0;
                }
                break;
            case 2:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 1;
                }
                break;
            case 3:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 1;
                }
                break;
            case 4:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 1;
                }
                break;
            case 5:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    StartCoroutine(Slide());
                }
                break;
            case 6:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    if (isGrounded)
                    {
                        Jump();
                    }
                }
                break;
            case 7:
                if (timerCanStart)
                {
                    actionTimer = new System.Timers.Timer(800);
                    actionTimer.Elapsed += OnTimedEvent;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                    timerFinished = false;
                    timerCanStart = false;
                    // StartCoroutine(Slide());
                }
                if (timerFinished && correct)
                {
                    desiredLane = 0;
                }
                break;
            default:
                break;
        }
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        actionTimer.Enabled = false;
        timerFinished = true;
        actionTimer.Stop();
        actionTimer.Dispose();
    }
}