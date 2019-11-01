using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    NeatManager networkManager;

    private float force_mult = 7.0f; //7 is good
    private float speed = 10.0f;
    private float rot_speed = 100.0f;

    void Awake(){
        rb = GetComponent<Rigidbody>();
        networkManager = GameObject.Find("NeatManager").GetComponent<NeatManager>();
    }

    void Update(){
        handleMovement();

        // Debug.Log("Mag: " + rb.velocity.magnitude + " || "  + gameObject.name, gameObject);
    }

    private void handleMovement(){
         if (Input.GetKey(KeyCode.A)){
            //  applyForceOnSide(2);
         }
         if (Input.GetKey(KeyCode.D)){
            //  applyForceOnSide(3);
         }
         if (Input.GetKey(KeyCode.W)){
            //  applyForceOnSide(0);
         }
         if (Input.GetKey(KeyCode.S)){
            //  applyForceOnSide(1);
         }
    }

    public void applyForceOnSide(int side, float amount){
        if (side == 0){
            rb.AddForce(Vector3.forward * (force_mult * amount));
        }
        else if (side == 1){
            rb.AddForce(Vector3.back * (force_mult * amount));

        }
        else if (side == 2){
            rb.AddForce(Vector3.left * (force_mult * amount));

        }
        else if (side == 3){
            rb.AddForce(Vector3.right * (force_mult * amount));

        }
    }

    public void applyForces(Matrix forces){
        rb.AddForce((Vector3.forward * forces.get(0)) * force_mult);
        rb.AddForce((Vector3.right * forces.get(1)) * force_mult);
        rb.AddForce((-Vector3.forward * forces.get(2)) * force_mult);
        rb.AddForce((-Vector3.right * forces.get(3)) * force_mult);
    }

    public void applyForceOnAxis(float horiz, float vert, bool fps_indep){
        // Debug.Log("VERT: " + vert);
        // Debug.Log("HORIZ: " + horiz);
        float fps_mult = fps_indep ? Time.deltaTime : 1.0f;
        force_mult = fps_indep ? force_mult : force_mult / 1.3f;
        rb.AddForce((Vector3.forward * vert) * force_mult * fps_mult, ForceMode.Impulse);
        rb.AddForce((Vector3.right * horiz) * force_mult * fps_mult, ForceMode.Impulse);
    }

    public void moveForwardWithRot(float horiz){
        horiz *= Time.deltaTime * rot_speed;
        transform.eulerAngles += new Vector3(0, horiz, 0);
        rb.MovePosition(transform.position + (transform.forward * speed * Time.deltaTime));
    }


    public void resetPlayer(Vector3 pos){
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        gameObject.SetActive(true);
        rb.isKinematic = false;

    }

    private void OnCollisionEnter(Collision other){
        GameObject otherObject = other.gameObject;
        if (otherObject.tag == "MazeFloor"){ return; }
        networkManager.OnPlayerWallHitHelper(this.gameObject, otherObject);
    }

}
