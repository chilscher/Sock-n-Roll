//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour{
    //controls the behavior of an Enemy. Attached to the Enemy prefab

    [Header("Animation Speeds")]
    public float idleSpeed = 1f;
    public float deathSpeed = 1f;
    public float shootSpeed = 1f;
    private float idleDuration;
    private float deathDuration;
    private float shootDuration;

    [Header("Animation Action Times")]
    public float shotFiredTime = 0.45f; //how far through the shot animation is the bullet fired

    [Header("Time Between Shots")] //the time between shots is a random amount of time between minShootInterval and maxShootInterval, in seconds
    public float minShootInterval = 3f;
    public float maxShootInterval = 5f;

    [Header("Bullet Prefab")]
    public Bullet bulletPrefab;

    [Header("Tutorials and Menus")]
    public bool canShoot = true; //if the enemy is on the main menu or in the first few levels, they cannot shoot
    
    //general-purpose variables, defined at the start of the level
    private Animator animator;
    private Player player;
    private AudioManager audioManager;

    //timers for the enemy's animations and shooting
    private float actionTimeRemaining = 0f; //the enemy cannot do their dying animation until their previous animation is completed
    private float timeLeftUntilShoot = 0f;

    //what state the enemy is in
    private bool isIdle = false;
    private bool isDying = false;
    private bool isShooting = false;
    public bool isDead = false;
    private bool hasFiredBullet = false; //if the enemy has fired a bullet during the current shot animation


    void Start() {
        //set variables
        audioManager = FindObjectOfType<AudioManager>();
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player").GetComponent<Player>();

        //calculate and set times used for the animator and also for the enemy's internal calculations
        getAnimationTimes();
        setAnimationSpeeds();

        //set the enemy up to make their first shot at a random time in the near future
        startIdleAtRandom();
    }


    void Update() {
        //count down the time until the enemy's next shot, or until they are free to start another animation (like the dying animation)
        countDownActionTime();
        countDownToShoot();

        rotateTowardsPlayer(); //the enemy will always track the player's movements
        if (!StaticVariables.pausedFromAchievements) {
            //if the game is not paused for the acheivement popup, then see if the enemy can shoot
            shoot();
            fireBullet();
        }
        //if the enemy is done with the shooting animation, go back to their idle animation
        goBackToIdle();
        //if the enemy is done with the shooting animation and they can die, let them die
        die();
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT ARE INVOLVED WITH SETTING UP THE ENEMY AT THE START OF THE LEVEL
    // ---------------------------------------------------
    
    private void getAnimationTimes() {
        //calculates the duration of several animations depending on the animation clip length and the speed scalar set in the inspector
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips) {
            switch (clip.name) {
                case "Idle":
                    idleDuration = clip.length / idleSpeed;
                    break;
                case "Death":
                    deathDuration = clip.length / deathSpeed;
                    break;
                case "Shoot":
                    shootDuration = clip.length / shootSpeed;
                    break;
            }
        }
    }

    private void setAnimationSpeeds() {
        //sets the enemy animation speeds in the animator
        animator.SetFloat("idleSpeed", idleSpeed);
        animator.SetFloat("deathSpeed", deathSpeed);
        animator.SetFloat("shootSpeed", shootSpeed);
    }

    private void startIdleAtRandom() {
        //at the start of the level, start the enemy at some point in their idle animation, so all of the enemies are not being idle together
        //also sets the random amount of time until the enemy's first shot
        float rand = Random.value;

        animator.Play("Idle", 0, rand);
        isIdle = true;

        float countdownStart = rand * minShootInterval;
        timeLeftUntilShoot = minShootInterval - countdownStart;
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH SHOOTING, FIRING BULLETS, AND BEING IDLE, AND THEIR ANIMATIONS
    // ---------------------------------------------------

    private void countDownActionTime() {
        //count down the timer on the enemy's current action
        actionTimeRemaining -= Time.deltaTime;
        if (actionTimeRemaining < 0) {
            actionTimeRemaining = 0f;
        }
    }

    private void countDownToShoot() {
        //count down the timer until the enemy shoots next
        if (isIdle) {
            timeLeftUntilShoot -= Time.deltaTime;
            if (timeLeftUntilShoot < 0) {
                timeLeftUntilShoot = 0;
            }
        }
        else {
            resetShootCounter();
        }
    }

    private void rotateTowardsPlayer() {
        //if the enemy is alive, they should always be rotated towards the player's current position
        if (isIdle || isShooting) {
            Vector3 playerDirection = player.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(playerDirection);
            transform.rotation = rotation;
        }
    }

    private void shoot() {
        //starts the shooting animation if the enemy is done with their shoot countdown
        if (canShoot && isIdle && timeLeftUntilShoot == 0) {
            isShooting = true;
            isIdle = false;
            isDying = false;
            resetShootCounter();
            animator.SetBool("shooting", true);
            actionTimeRemaining = shootDuration;
            hasFiredBullet = false;
        }
    }

    private void resetShootCounter() {
        //sets up the time until the enemy shoots next
        float timeUntilNextShot = (Random.value * (maxShootInterval - minShootInterval)) + minShootInterval; //the next shot happens in a random amount of time between the min and max shoot intervals
        timeLeftUntilShoot = timeUntilNextShot;
    }

    private void fireBullet() {
        //if the enemy is shooting and the appropriate amount of time has passed, let the bullet loose
        if (isShooting) {
            float timeIntoShot = shootDuration - actionTimeRemaining;
            float whenDoesBulletFire = shotFiredTime * shootDuration;
            bool bulletFiredYet = (timeIntoShot >= whenDoesBulletFire);
            if (bulletFiredYet && hasFiredBullet == false) {
                hasFiredBullet = true;

                
                Bullet bullet = Instantiate(bulletPrefab);
                Vector3 bulletPos = GetComponent<SphereCollider>().bounds.center;
                bullet.transform.position = bulletPos;

                Vector3 playerPos = player.transform.position;
                Vector3 dirToPlayer = playerPos - bulletPos;
                Vector3 trajectory = new Vector3(dirToPlayer.x, 0, dirToPlayer.z).normalized;
                bullet.setTrajectory(trajectory);

                audioManager.play("Bullet Fired");
            }
        }
    }

    private void goBackToIdle() {
        //returns the enemy to their idle animation after their shooting animation is over
        if (isShooting && actionTimeRemaining == 0) {
            isIdle = true;
            isShooting = false;
            isDying = false;
            animator.SetBool("shooting", false);
        }
    }

    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE ENEMY'S DEATH
    // ---------------------------------------------------

    private void die() {
        //if the enemy has just died, and they are not doing a shooting animation, they die
        if (isDying && actionTimeRemaining <= 0) {
            isDying = false;
            isDead = true;
        }
    }


    public void hasBeenHit() {
        //called when the enemy gets hit by a bullet. they die
        if (!isDying && !isDead) {
            animator.SetTrigger("dying");
            isDying = true;
            isIdle = false;
            isShooting = false;
            actionTimeRemaining = deathDuration;

            audioManager.play("Enemy Death");
        }
    }

    public bool getIsDead() {
        //returns if the enemy is dead, as a bool, obviously
        return isDead;
    }
}
