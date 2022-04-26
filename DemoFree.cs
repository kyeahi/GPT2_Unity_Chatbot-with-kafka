using UnityEngine;
using System;


public class DemoFree : MonoBehaviour
{

    private readonly string[] m_animations = { "Pickup", "Wave" };
    private Animator m_animators=null;
    public GameObject DisconnectedPanel;

    private void Start()
    {
        
        
    }

    private void Update()
    {
        if(m_animators==null)
        {
            try
            {
                m_animators=GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
            }
            catch(NullReferenceException)
            {
  
            }
        }

    }

    private void OnGUI()
    {
        if(DisconnectedPanel.activeSelf==false)
        {
            GUILayout.BeginVertical(GUILayout.Width(Screen.width));
            for (int i = 0; i < m_animations.Length; i++)
        {
            if (i == 0) { GUILayout.BeginHorizontal(); }

            if (GUILayout.Button(m_animations[i]))
            {

                m_animators.SetTrigger(m_animations[i]);
                
            }

            if (i == m_animations.Length - 1) { GUILayout.EndHorizontal(); }
            else if (i == (m_animations.Length / 2)) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
        }
            GUILayout.EndVertical();


        }


    }
}
