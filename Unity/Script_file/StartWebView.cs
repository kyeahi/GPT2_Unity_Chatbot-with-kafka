using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartWebView : MonoBehaviour
{
    private string[] URLs;
    public int i=-1;

    void Awake()
    {
        URLs=new string[2]{"http://www.sjb-teamp.kro.kr/main","http://www.gyuho.r-e.kr/"};

    }
   public void WebClick()
   {
       Application.OpenURL(URLs[i]);
   }

}
