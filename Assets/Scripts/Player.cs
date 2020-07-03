//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{
    //controls the behavior of the Player. Attached to the player prefab

    //keyboard keys for playing the game, used for testing
    [Header("Input Keys")]
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode punchKey = KeyCode.X;
    public KeyCode rollKey = KeyCode.Z;

    [Header("Animation Speeds")]//multiplier for how fast the animation gets played
    public float idleSpeed = 1f;
    public float walkSpeed = 1f;
    public float punchSpeed = 1f;
    public float rollSpeed = 1f;
    public float deathSpeed = 1f;

    [Header("Animation Action Times")]
    public float punchHitTime = 0.46f; //how far through the punch does the enemy get knocked down. 0 is at the start, 1 is at the end
    public float rollStartTime = 0.15f; //how far through the roll does the player start moving fast

    [Header("Sound Action Times")] //the % of the way through the action that the related audio starts
    public float punchSoundTime = 0.2f;
    public float rollSoundTime = 0.1f;

    [Header("Movement Speeds")]
    public float walkingSpeed = 3f;
    public float rollingSpeed = 5f;

    [Header("Health Information")]
    public int startingHP = 3;

    [Header("Tutorials")]
    public bool canRoll = true;
    public bool canTakeDamage = true;



    //the player's current state
    private bool isWalking = false;
    private bool isPunching = false;
    private bool isRolling = false;
    private bool isDying = false;
    private bool hasHitEnemy = false; //if the player has hit an enemy during the current punch
    private bool hasWon = false;
    private bool hasLost = false;
    private bool isDead = false;
    private float actionTimeRemaining = 0f;

    //animation variables
    private Animator animator;
    private float idleDuration;
    private float walkDuration;
    private float punchDuration;
    private float rollDuration;
    private float deathDuration;

    //if the audio clip for some actions has happened yet, used during the animation process
    private bool hasPunchSounded = false;
    private bool hasRollSounded = false;

    [HideInInspector]
    public int HP;
    private List<Enemy> enemiesInRange = new List<Enemy>();//the enemies that are close enough to be punched
    private AudioManager audioManager;
    private GameObject ground;
    private Vector3 walkingDirection;



    void Start() {
        //set starting variables
        if (ground == null) {
            ground = GameObject.Find("Ground");
        }
        audioManager = FindObjectOfType<AudioManager>();
        HP = startingHP;
        animator = GetComponent<Animator>();

        //calculate and set times used for the animator and also for the player's internal calculations
        setAnimationSpeeds();
        getAnimationTimes();
    }

    
    void Update(){
        //determine the player's action
        setAction();

        //if the player is punching, do that stuff
        animatePunch();
        punchSound();
        hitEnemy();

        //if the player is rolling, do that stuff
        animateRoll();
        rollSound();
        roll();

        //if the player is walking, do that stuff
        setWalkingDirection();
        rotateToWalkingDirection();
        animateWalk();
        walk();

        //see if the player should win or lose the level
        checkForWin();
        die();

        //count down the time remaining until the player can make their next action
        countDownActionTime();

        //checks if the player is out of bounds, specifically to prompt the out-of-bounds achievement
        checkIfOutOfBounds();

    }

    // ---------------------------------------------------
    //SETUP FUNCTIONS
    // ---------------------------------------------------

    private void getAnimationTimes() {
        //calculates the duration of several animations depending on the animation clip length and the speed scalar set in the inspector
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips) {
            switch (clip.name) {
                case "Idle":
                    idleDuration = clip.length / idleSpeed;
                    break;
                case "Walk":
                    walkDuration = clip.length / walkSpeed;
                    break;
                case "Roll":
                    rollDuration = clip.length / rollSpeed;
                    break;
                case "Punch":
                    punchDuration = clip.length / punchSpeed;
                    break;
                case "Death":
                    deathDuration = clip.length / deathSpeed;
                    break;
            }
        }
    }

    private void setAnimationSpeeds() {
        //sets the player animation speeds in the animator
        animator.SetFloat("idleSpeed", idleSpeed);
        animator.SetFloat("walkSpeed", walkSpeed);
        animator.SetFloat("punchSpeed", punchSpeed);
        animator.SetFloat("rollSpeed", rollSpeed);
        animator.SetFloat("deathSpeed", deathSpeed);
    }


    // ---------------------------------------------------
    //FUNCTIONS THAT PROCESS PLAYER INPUT AND DETERMINE WHAT YOU ARE DOING WHEN
    // ---------------------------------------------------

    private void setAction() {
        //determines which action the player is taking. Either walking, rolling, or punching
        //if the player is in the middle of an action, a new action is not checked
        //punching takes priority, then rolling, then walking
        if (!StaticVariables.pausedFromAchievements) {
            if (actionTimeRemaining == 0 && !isDying && !hasLost && !isDead && Time.timeScale > 0) {
                if (Input.GetKeyDown(punchKey) || StaticVariables.justPressedPunchButton) {
                    StaticVariables.justPressedPunchButton = false;
                    isPunching = true;
                    isWalking = false;
                    isRolling = false;
                    actionTimeRemaining = punchDuration;
                    hasHitEnemy = false;
                    hasPunchSounded = false;
                    StaticVariables.punchCount++;
                    StaticVariables.enemiesPunchedSimultaneously = new List<Enemy>();
                }
                else if (Input.GetKey(rollKey) || StaticVariables.pressingRollButton) {
                    if (canRoll) {
                        if (isRolling) { animator.SetTrigger("reroll"); }
                        isRolling = true;
                        isWalking = false;
                        isPunching = false;
                        actionTimeRemaining = rollDuration;
                        hasRollSounded = false;
                    }
                }
                else if (Input.GetKey(upKey) || Input.GetKey(downKey) || Input.GetKey(leftKey) || Input.GetKey(rightKey)) {
                    isWalking = true;
                    isRolling = false;
                    isPunching = false;
                }
                else if (StaticVariables.usingJoystick) {
                    isWalking = true;
                    isRolling = false;
                    isPunching = false;
                }
                else {
                    isRolling = false;
                    isWalking = false;
                    isPunching = false;
                }
            }
        }
    }
    
    private void countDownActionTime() {
        //count down the time remaining until the player can make their next action
        float a = actionTimeRemaining;
        actionTimeRemaining -= Time.deltaTime;
        if (actionTimeRemaining < 0) {
            actionTimeRemaining = 0f;
        }        
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT HANDLE WALKING
    // ---------------------------------------------------
    
    private void setWalkingDirection() {
        //uses the keyboard inputs and the joystick to determine the player's walking direction.
        if (isWalking) {
            Vector3 upDir = new Vector3(-1, 0, 0);
            Vector3 downDir = -upDir;
            Vector3 leftDir = new Vector3(0, 0, -1);
            Vector3 rightDir = -leftDir;
            Vector3 walkDir = Vector3.zero;
            if (Input.GetKey(upKey)) { walkDir += upDir; }
            if (Input.GetKey(downKey)) { walkDir += downDir; }
            if (Input.GetKey(leftKey)) { walkDir += leftDir; }
            if (Input.GetKey(rightKey)) { walkDir += rightDir; }
            if (StaticVariables.usingJoystick) {
                walkDir = new Vector3(-StaticVariables.joystickDirection.y, 0, StaticVariables.joystickDirection.x);
            }
            walkingDirection = walkDir;
            if (walkDir != Vector3.zero) { walkingDirection.Normalize(); }
        }
    }

    private void rotateToWalkingDirection() {
        //rotates the player towards their walking direction.
        //rotation happens immediately
        if (isWalking) {
            if (walkingDirection != (new Vector3(0, 0, 0))) {
                Quaternion rotation = Quaternion.LookRotation(walkingDirection);
                transform.rotation = rotation;
            }
        }
    }

    private void walk() {
        //move the player in the direction of their walking speed
        if (isWalking && !StaticVariables.pausedFromAchievements) {
            GetComponent<Rigidbody>().position += (walkingDirection * walkingSpeed * Time.deltaTime);
        }
    }

    private void animateWalk() {
        //if the player is walking, set the bool trigger that starts the walking animation
        if (isWalking && walkingDirection != Vector3.zero) {animator.SetBool("walking", true);}
        else {animator.SetBool("walking", false);}
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT HANDLE PUNCHING
    // ---------------------------------------------------
    
    private void hitEnemy() {
        //if the player is punching and it is the right time in the punching animation, and an enemy is in front of them, hit the enemy
        if (isPunching) {
            float timeIntoPunch = punchDuration - actionTimeRemaining;
            float whenDoesPunchHit = punchHitTime * punchDuration;
            bool punchHappenedYet = (timeIntoPunch >= whenDoesPunchHit);
            if (punchHappenedYet && !hasHitEnemy) {
                hasHitEnemy = true;
                foreach (Enemy e in enemiesInRange){
                    e.hasBeenHit();

                    //used to check if player has accomplished achievement "Defeat all enemies in level 24 with punches"
                    if (!e.isDead) {
                        if (!StaticVariables.enemiesPunched.Contains(e)) {
                            StaticVariables.enemiesPunched.Add(e);
                        }
                    }

                    //used to check if player has accomplished "Hit 3 enemies with one punch"
                    if (!e.isDead) {
                        if (!StaticVariables.enemiesPunchedSimultaneously.Contains(e)) {
                            StaticVariables.enemiesPunchedSimultaneously.Add(e);
                        }
                    }
                }
            }
        }
    }

    private void punchSound() {
        //if the appropriate amount of time into a punch has passed, play the punching sound
        if (isPunching) {
            float timeIntoPunch = punchDuration - actionTimeRemaining;
            float whenDoesPunchSoundStart = punchSoundTime * punchDuration;
            bool soundHappenedYet = (timeIntoPunch >= whenDoesPunchSoundStart);
            if (soundHappenedYet && !hasPunchSounded) {
                audioManager.play("Punch");
                hasPunchSounded = true;
            }
        }
    }

    private void animatePunch() {
        //if the player is punching, set the bool trigger that starts the punching animation
        if (isPunching) { animator.SetBool("punching", true); }
        else { animator.SetBool("punching", false); }
    }

    private void OnTriggerEnter(Collider col) {
        //when the player's collider hits an enemy's collider, add them to the list of enemies that will get hit by a punch
        if (col.gameObject.tag == "Enemy") { enemiesInRange.Add(col.GetComponent<Enemy>()); }
    }

    private void OnTriggerExit(Collider col) {
        //when the player's collider exits an enemy's collider, remove the enemy from the list of enemies that will get hit by a punch
        if (col.gameObject.tag == "Enemy") { enemiesInRange.Remove(col.GetComponent<Enemy>()); }
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT HANDLE ROLLING
    // ---------------------------------------------------
    
    private void rollSound() {
        //in the appropriate amount of time into a roll has passed, play the rolling sound
        if (isRolling) {
            float timeIntoRoll = rollDuration - actionTimeRemaining;
            float whenDoesRollSoundStart = rollSoundTime * rollDuration;
            bool soundHappenedYet = (timeIntoRoll >= whenDoesRollSoundStart);
            if (soundHappenedYet && !hasRollSounded) {
                audioManager.play("Roll");
                hasRollSounded = true;
            }
        }
    }
    
    private void animateRoll() {
        //if the player is rolling, set the bool trigger that starts the rolling animation
        if (isRolling) { animator.SetBool("rolling", true); }
        else { animator.SetBool("rolling", false);}
    }
    
    private void roll() {
        //when the player pushes the roll button, they begin the rolling process, but the animation and the speed-up from the roll don't start immediately
        //this handles that process
        if (isRolling && !StaticVariables.pausedFromAchievements) {
            float timeIntoRoll = rollDuration - actionTimeRemaining;
            float whenDoesSpeedStart = rollStartTime * rollDuration;
            bool rollStartedYet = (timeIntoRoll >= whenDoesSpeedStart);
            if (rollStartedYet) { GetComponent<Rigidbody>().position += (transform.forward * rollingSpeed * Time.deltaTime);} //once the roll starts, the player moves faster
            else { GetComponent<Rigidbody>().position += (transform.forward * walkingSpeed* Time.deltaTime);} //until then, they still move at their normal walking speed
        }
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT HANDLE GETTING HIT, WINNING, AND LOSING
    // ---------------------------------------------------
    
    public void hasBeenHit() {
        //when the player gets hit by a bullet, they will take some damage, and then will die if they lost all their hp
        if (!isDead) {
            if (HP > 0) {
                if (canTakeDamage) { HP -= 1; }
            }
            if (HP <= 0 && !isDying) {
                animator.SetTrigger("dying");
                isWalking = false;
                isRolling = false;
                isPunching = false;
                isDying = true;
                actionTimeRemaining = deathDuration;
                audioManager.fadeOutAll();
                audioManager.play("Player Death");
            }
            if (HP > 0) {
                audioManager.play("Player Gets Hit");
            }
        }
    }

    private void die() {
        //the player dies, at the end of the "dying" animation process
        if (isDying && actionTimeRemaining == 0) {
            isDying = false;
            isDead = true;
        }
    }

    private void checkForWin() {
        //sees if the player has defeated all enemies. If they have, they win!
        if (!hasWon && !hasLost) { 
            GameObject[] enemiesLeft = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemiesLeft.Length == 0) {
                win();
            }
            else {
                foreach (GameObject g in enemiesLeft) {
                    if (!g.GetComponent<Enemy>().getIsDead()) {
                        return;
                    }
                }
                win();
            }
        }
    }

    private void win() { hasWon = true; } //set the winning boolean, used by the levelCanvasController

    private void lose() {
        //once the player dies, sets all of the appropriate variables. haslost is read by the levelCanvasController to start the lose-screen-popup process
        hasLost = true;
        isWalking = false;
        isPunching = false;
        isRolling = false;
        isDying = false;
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT GET DATA FOR OTHER SCRIPTS TO USE
    // ---------------------------------------------------

    public bool hasWonYet() { return hasWon; }

    public bool hasLostYet() {return hasLost;}

    public int getHP() {return HP;}

    public bool getIsDead() { return isDead; }
    
    private void checkIfOutOfBounds() {
        //checks if the player is out of bounds, specifically for the out-of-bounds achievement popup. StaticVariables.isOutOfBounds is read in levelCanvasController
        if ((transform.position.z < -6f) || (transform.position.z > 5.9f) || (transform.position.x < -72.7f) || (transform.position.x > -61.1f))  {
            StaticVariables.isOutOfBounds = true;
        }
    }

}
