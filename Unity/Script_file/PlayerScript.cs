using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Confluent.Kafka;
using System.Threading.Tasks;
using System;
public class PlayerScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public float moveSpeed=10;
    float hzInput, vInput;
    Vector3 lookDirection;
    public GameObject RoomPanel;

    void Start() 
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(PV.IsMine)
        {
            // float axis=Input.GetAxisRaw("Horizontal");
            // float axis2=Input.GetAxisRaw("Vertical");
            // transform.Translate(new Vector3(axis*Time.deltaTime*7,0,axis2*Time.deltaTime*7));
            if(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow))
            {GetDirectionAndMove();}
        }
        
    }

    void GetDirectionAndMove()
    {
        hzInput=-Input.GetAxis("Horizontal");
        vInput=Input.GetAxis("Vertical");
        lookDirection=hzInput*Vector3.forward+vInput*Vector3.right;

        this.transform.Translate(Vector3.right*moveSpeed*Time.deltaTime);
        this.transform.rotation=Quaternion.LookRotation(lookDirection);

    }
}
