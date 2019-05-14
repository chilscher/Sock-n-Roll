using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour{

    public float speed = 1f;

    private Vector3 trajectory;

    void Update(){transform.Translate(speed * trajectory * Time.deltaTime);}

    public void setTrajectory(Vector3 traj) {trajectory = traj;}

    public void OnTriggerEnter(Collider col) {
        if (col.GetType() == typeof(BoxCollider)) {
            Destroy(gameObject);
            if (col.gameObject.tag == "Enemy") {col.gameObject.GetComponent<Enemy>().hasBeenHit();}
            if (col.gameObject.tag == "Player") { col.gameObject.GetComponent<Player>().hasBeenHit(); }
        }
    }
}
