//for Sock 'n Roll, copyright Cole Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour{
    //controls the movement and collision behavior of a bullet. Attached to a bullet prefab
    //bullets are created from the enemy script
    public float speed = 1f;
    private Vector3 trajectory;

    void Update() {
        //move the bullet every frame (except when the game is paused)
        if (!StaticVariables.pausedFromAchievements) { transform.Translate(speed * trajectory * Time.deltaTime); }
    }

    public void setTrajectory(Vector3 traj) {trajectory = traj;}

    public void OnTriggerEnter(Collider col) {
        //when the bullet hits something, destroy the bullet object
        //if it hits a player or enemy, run the appropriate function
        if (col.GetType() == typeof(BoxCollider)) {
            Destroy(gameObject);
            if (col.gameObject.tag == "Enemy") {col.gameObject.GetComponent<Enemy>().hasBeenHit();}
            if (col.gameObject.tag == "Player") { col.gameObject.GetComponent<Player>().hasBeenHit(); }
        }
    }
}
