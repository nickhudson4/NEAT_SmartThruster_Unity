using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    NeatManager networkManager;

    private float force_mult = 8.0f; //7 is good
    private float speed = 50; //10 for frameInd, else 0.2
    private float rot_speed = 120.0f; //0.2 for build, 5.0 for editor, 20 for frameInd

    void Awake(){
        rb = GetComponent<Rigidbody>();
        networkManager = GameObject.Find("NeatManager").GetComponent<NeatManager>();
    }

    void Update(){
        // handleMovement();

        // Debug.Log("Mag: " + rb.velocity.magnitude + " || "  + gameObject.name, gameObject);
    }

    private void handleMovement(){
         if (Input.GetKey(KeyCode.A)){
             applyForceOnSide(2, 1);
         }
         if (Input.GetKey(KeyCode.D)){
             applyForceOnSide(3, 1);
         }
         if (Input.GetKey(KeyCode.W)){
             applyForceOnSide(0, 1);
         }
         if (Input.GetKey(KeyCode.S)){
             applyForceOnSide(1, 1);
         }
    }

    public void applyHorizontalForce(float val1, float val2){

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

    public void applyStrictForce(Matrix outputs){
        if (outputs.get(0) > outputs.get(1)){

            rb.AddForce((Vector3.forward * 1.0f) * force_mult * Time.deltaTime, ForceMode.Impulse);
        }
        else {
            rb.AddForce((Vector3.forward * -1.0f) * force_mult * Time.deltaTime, ForceMode.Impulse);

        }
        if (outputs.get(2) > outputs.get(3)){
            rb.AddForce((Vector3.right * 1.0f) * force_mult * Time.deltaTime, ForceMode.Impulse);
        }
        else {
            rb.AddForce((Vector3.right * -1.0f) * force_mult * Time.deltaTime, ForceMode.Impulse);

        }
    }

    public void applyTranslateOnAxis(float horiz, float vert, float speed2){
        Vector3 dir = new Vector3(horiz, 0, vert);
        rb.MovePosition(transform.position + (dir * (speed * .02f)));
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
