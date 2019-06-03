using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour{


    [Header("Animation Speeds")]
    public float idleSpeed = 1f;
    public float deathSpeed = 1f;
    public float shootSpeed = 1f;

    [Header("Animation Action Times")]
    public float shotFiredTime = 0.45f; //how far through the shot animation is the bullet fired

    [Header("Time Between Shots")]
    public float minShootInterval = 3f;
    public float maxShootInterval = 5f;

    [Header("Bullet Prefab")]
    public Bullet bulletPrefab;

    [Header("Tutorials and Menus")]
    public bool canShoot = true;





    private Animator animator;
    private Player player;
    private float idleDuration;
    private float deathDuration;
    private float shootDuration;
    private float actionTimeRemaining = 0f;
    private float timeLeftUntilShoot = 0f;
    private bool isIdle = false;
    private bool isDying = false;
    private bool isShooting = false;
    private bool isDead = false;
    private bool isReviving = false;
    private bool hasFiredBullet = false; //if the enemy has fired a bullet during the current shot animation
    private AudioManager audioManager;


    void Start() {
        audioManager = FindObjectOfType<AudioManager>();
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player").GetComponent<Player>();

        getAnimationTimes();
        setAnimationSpeeds();

        startIdleAtRandom();
    }


    void Update() {
        countDownActionTime();
        countDownToShoot();

        rotateTowardsPlayer();

        shoot();
        fireBullet();
        goBackToIdle();

        die();
        revive();
    }

    private void getAnimationTimes() {
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
        animator.SetFloat("idleSpeed", idleSpeed);
        animator.SetFloat("deathSpeed", deathSpeed);
        animator.SetFloat("shootSpeed", shootSpeed);
    }

    private void startIdleAtRandom() {
        float rand = Random.value;

        animator.Play("Idle", 0, rand);
        isIdle = true;

        float countdownStart = rand * minShootInterval;
        timeLeftUntilShoot = minShootInterval - countdownStart;
    }

    private void countDownActionTime() {
        actionTimeRemaining -= Time.deltaTime;
        if (actionTimeRemaining < 0) {
            actionTimeRemaining = 0f;
        }
    }

    private void countDownToShoot() {
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
        if (isIdle || isShooting) {
            Vector3 playerDirection = player.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(playerDirection);
            transform.rotation = rotation;
        }
    }

    private void shoot() {
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
        if (isShooting && actionTimeRemaining == 0) {
            isIdle = true;
            isShooting = false;
            isDying = false;
            animator.SetBool("shooting", false);
        }
    }

    private void die() {
        if (isDying && actionTimeRemaining <= 0) {
            isDying = false;
            isDead = true;
        }
    }


    public void hasBeenHit() {
        if (!isDying && !isDead && !isReviving) {
            animator.SetTrigger("dying");
            isDying = true;
            isIdle = false;
            isShooting = false;
            actionTimeRemaining = deathDuration;

            audioManager.play("Enemy Death");
        }
    }

    public bool getIsDead() {
        return isDead;
    }

    public void startReviving() {
        if (isDead) {
            animator.SetTrigger("respawning");
            isReviving = true;
            isDead = false;
            actionTimeRemaining = deathDuration;
        }
    }

    private void revive() {
        if (isReviving && actionTimeRemaining <= 0) {
            isReviving = false;
            startIdleAtRandom();
            animator.SetTrigger("respawned");
            animator.SetBool("shooting", false);
            resetShootCounter();
        }
    }
}
