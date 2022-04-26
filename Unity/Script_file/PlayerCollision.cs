using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    
    public GameObject webportal;
    public StartWebView swv;
    void Awake()
    {
        webportal=GameObject.Find("Canvas").transform.GetChild(5).gameObject;
        swv=webportal.GetComponent<StartWebView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision) 
    {
        if(collision.gameObject.CompareTag("portal"))
        {
            // collision.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            webportal.SetActive(true);
            // Debug.Log(collision.gameObject.name);
            if (collision.gameObject.name=="theater")
            {
                swv.i=1;

            }
            else
            {
                swv.i=0;
            }

        }
    }

    private void OnTriggerExit(Collider collision) 
    {
        if(collision.gameObject.CompareTag("portal"))
        {
            // collision.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            webportal.SetActive(false);

        }
        else
        {
            swv.i=-1;

        }
    }
}
