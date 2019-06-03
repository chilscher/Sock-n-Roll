using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{
    
    [Header("Input Keys")]
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode punchKey = KeyCode.X;
    public KeyCode rollKey = KeyCode.Z;

    [Header("Animation Speeds")]
    public float idleSpeed = 1f;
    public float walkSpeed = 1f;
    public float punchSpeed = 1f;
    public float rollSpeed = 1f;
    public float deathSpeed = 1f;

    [Header("Animation Action Times")]
    public float punchHitTime = 0.46f; //how far through the punch does the enemy get knocked down. 0 is at the start, 1 is at the end
    public float rollStartTime = 0.15f; //how far through the roll does the player start moving fast

    [Header("Sound Action Times")]
    public float punchSoundTime = 0.2f;

    [Header("Movement Speeds")]
    public float walkingSpeed = 3f;
    public float rollingSpeed = 5f;

    [Header("Health Information")]
    public int startingHP = 3;
    public bool onMenu = false;

    [Header("Tutorials and Menus")]
    public bool canRoll = true;
    public bool canTakeDamage = true;


    private Vector3 walkingDirection;
    private Animator animator;
    private bool isWalking = false;
    private bool isPunching = false;
    private bool isRolling = false;
    private bool isDying = false;
    private bool hasHitEnemy = false; //if the player has hit an enemy during the current punch
    private bool hasWon = false;
    private bool hasLost = false;
    private bool isDead = false;
    private bool isReviving = false;
    private float actionTimeRemaining = 0f;
    private float idleDuration;
    private float walkDuration;
    private float punchDuration;
    private float rollDuration;
    private float deathDuration;
    private int HP;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    private bool hasPunchSounded = false;
    private AudioManager audioManager;


    void Start() {
        audioManager = FindObjectOfType<AudioManager>();
        HP = startingHP;
        animator = GetComponent<Animator>();
        setAnimationSpeeds();
        getAnimationTimes();
    }

    
    void Update(){
        setAction();

        animatePunch();
        punchSound();
        hitEnemy();

        animateRoll();
        roll();

        setWalkingDirection();
        rotateToWalkingDirection();
        animateWalk();
        walk();

        checkForWin();
        die();
        revive();

        countDownActionTime();
    }

    private void countDownActionTime() {
        actionTimeRemaining -= Time.deltaTime;
        if (actionTimeRemaining < 0) {
            actionTimeRemaining = 0f;
        }
    }

    private void setAction() {
        //determines which action the player is taking. Either walking, rolling, or punching
        //if the player is in the middle of an action, a new action is not checked
        //punching takes priority, then rolling, then walking
        if (actionTimeRemaining == 0 && !isDying && !hasLost && !isDead && Time.timeScale > 0) {
            isWalking = false;
            isPunching = false;
            isRolling = false;
            if (Input.GetKeyDown(punchKey)) {
                isPunching = true;
                actionTimeRemaining = punchDuration;
                hasHitEnemy = false;
                hasPunchSounded = false;
            }
            else if (Input.GetKeyDown(rollKey)) {
                if (canRoll) {
                    isRolling = true;
                    actionTimeRemaining = rollDuration;
                }
            }
            else if (Input.GetKey(upKey) || Input.GetKey(downKey) || Input.GetKey(leftKey) || Input.GetKey(rightKey)) {
                isWalking = true;
            }
        }
    }


    private void setWalkingDirection() {
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
            walkingDirection = walkDir;
            if (walkDir != Vector3.zero) { walkingDirection.Normalize(); }
        }
    }

    private void rotateToWalkingDirection() {
        if (isWalking) {
            if (walkingDirection != (new Vector3(0, 0, 0))) {
                Quaternion rotation = Quaternion.LookRotation(walkingDirection);
                transform.rotation = rotation;
            }
        }
    }

    private void walk() {
        if (isWalking) {GetComponent<Rigidbody>().position += (walkingDirection * walkingSpeed * Time.deltaTime);}
    }

    private void animateWalk() {
        if (isWalking && walkingDirection != Vector3.zero) {animator.SetBool("walking", true);}
        else {animator.SetBool("walking", false);}
    }
    
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
                }
            }
        }
    }

    private void punchSound() {
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
        if (isPunching) { animator.SetBool("punching", true); }
        else { animator.SetBool("punching", false); }
    }

    private void animateRoll() {
        if (isRolling) { animator.SetBool("rolling", true); }
        else { animator.SetBool("rolling", false); }
    }

    private void getAnimationTimes() {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips) {
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
        animator.SetFloat("idleSpeed", idleSpeed);
        animator.SetFloat("walkSpeed", walkSpeed);
        animator.SetFloat("punchSpeed", punchSpeed);
        animator.SetFloat("rollSpeed", rollSpeed);
        animator.SetFloat("deathSpeed", deathSpeed);
    }

    private void roll() {
        if (isRolling) {
            float timeIntoRoll = rollDuration - actionTimeRemaining;
            float whenDoesSpeedStart = rollStartTime * rollDuration;
            bool rollStartedYet = (timeIntoRoll >= whenDoesSpeedStart);
            if (rollStartedYet) { GetComponent<Rigidbody>().position += (transform.forward * rollingSpeed * Time.deltaTime);}
            else { GetComponent<Rigidbody>().position += (transform.forward * walkingSpeed* Time.deltaTime);}
        }
    }

    private void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Enemy") { enemiesInRange.Add(col.GetComponent<Enemy>()); }
    }

    private void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Enemy") { enemiesInRange.Remove(col.GetComponent<Enemy>()); }
    }

    public void hasBeenHit() {
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
            if (HP > 0){
                audioManager.play("Player Gets Hit");
            }
        }
    }

    private void die() {
        if (isDying && actionTimeRemaining == 0) {
            isDying = false;
            isDead = true;
            if (!onMenu) { lose(); }
        }
    }

    private void revive() {
        if (isReviving && actionTimeRemaining == 0) {
            isReviving = false;
            animator.SetTrigger("respawned");
        }
    }

    private void checkForWin() {
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

    private void win() { hasWon = true; }

    private void lose() {
        hasLost = true;
        isWalking = false;
        isPunching = false;
        isRolling = false;
        isDying = false;
    }

    public bool hasWonYet() { return hasWon; }

    public bool hasLostYet() {return hasLost;}

    public void startReviving() {
        if (isDead) {
            HP = startingHP;
            animator.SetTrigger("respawning");
            isWalking = false;
            isRolling = false;
            isPunching = false;
            isDying = false;
            isReviving = true;
            isDead = false;
            actionTimeRemaining = deathDuration;
        }
    }

    public int getHP() {return HP;}

    public bool getIsDead() { return isDead; }
    
}
