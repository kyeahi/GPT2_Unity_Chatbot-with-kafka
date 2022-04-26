using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("DisconnectPanel")]
    public InputField NickNameInput;

    [Header("LobbyPanel")]
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public GameObject DisconnectedPanel;
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public PhotonView PV;
    
    public GameObject Player_list;
    public RectTransform movingPanel;
    private bool player_list_on=false;
    private PhotonView player_PV;

    public Transform startPoint;
    private List<Transform> targets;
    [SerializeField] private FreeCameraLogic m_cameraLogic;

    public Player targetPlayer=null;
    private bool chaton=true;

    roompanel_Kafka_send rks;
    Bot_kafka_send bks=null;
    


    void Awake() 
    {
        RoomPanel.SetActive(false);
        Player_list.SetActive(false);
        targets=m_cameraLogic.m_targets;
        Screen.SetResolution(960,540,false);
        PhotonNetwork.LocalPlayer.NickName =null;
        rks=GameObject.Find("Canvas").GetComponent<roompanel_Kafka_send>();
    }


    #region 플레이어 리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void PlayerListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else 
        {

        }
        PV.RPC("PlayerListRenewal",RpcTarget.All);
    }


    public void PlayerListOpenClick()
    {
        if(player_list_on)
        {
            movingPanel.anchoredPosition=new Vector3(728,0,0);
            player_list_on=false;
        }
        else
        {
            movingPanel.anchoredPosition=new Vector3(61,0,0);
            player_list_on=true;
        }
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        // PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        // NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    [PunRPC]
    void PlayerListRenewal()
    {
        
        string[] nickname_list=new string[20];
        int n_int=0;
        int npc_num=0;
        foreach(GameObject c in GameObject.FindGameObjectsWithTag("NPC"))
        {
            nickname_list[n_int]=c.name;
            n_int+=1;
        }
        npc_num=n_int;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            nickname_list[n_int]=p.NickName;
            n_int+=1;
        }

        // 최대페이지
        maxPage = ((PhotonNetwork.PlayerList.Length+npc_num) % CellBtn.Length == 0) ? (PhotonNetwork.PlayerList.Length+npc_num) / CellBtn.Length : (PhotonNetwork.PlayerList.Length+npc_num) / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;



        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < (PhotonNetwork.PlayerList.Length+npc_num)) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < (PhotonNetwork.PlayerList.Length+npc_num)) ? nickname_list[multiple + i] : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        PV.RPC("PlayerListRenewal",RpcTarget.All);    }
    #endregion


    #region 사용자 닉네임으로 접속

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        // WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        PhotonNetwork.JoinOrCreateRoom("Room",new RoomOptions{MaxPlayers=20},null);

    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    // public override void OnDisconnected(DisconnectCause cause)
    // {
    //     RoomPanel.SetActive(false);
    // }

    #endregion


    #region 방

    public void LeaveRoom()
    {
        targetPlayer=null;
        PhotonNetwork.LeaveRoom();
    }
    public override async void OnJoinedRoom()
    {
        
        PhotonNetwork.Instantiate("MaleFreeSimpleMovement1",startPoint.position,startPoint.rotation);
        
        GameObject[] go=GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject temp_go in go)
        {
            if (temp_go.GetComponent<PhotonView>().IsMine)
            {
                player_PV=temp_go.GetComponent<PhotonView>();
                targets.Add(temp_go.transform);
                m_cameraLogic.PreviousTarget();
                player_PV.name=NickNameInput.text;
            }
        }

        // PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        
        DisconnectedPanel.SetActive(false);
        RoomPanel.SetActive(true);
        Player_list.SetActive(true);

        RoomRenewal();
        PV.RPC("PlayerListRenewal",RpcTarget.All);
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";

    }
    #region 채팅
    
    
    public void Send()
    {

        if(bks!=null)
        {
            bks.ConveyedChat=ChatInput.text;
            bks.ConveyedPlayer=bks.gameObject.name;
            Debug.Log(bks.gameObject.name+"에게 '"+ChatInput.text+"'가 전달됨");
            PV.RPC("ChatRPC", PhotonNetwork.LocalPlayer, PhotonNetwork.NickName + " : " + ChatInput.text);
            ChatInput.text = "";
        }
        else if (targetPlayer==null)
        {
            PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
            ChatInput.text = "";
        }
        else
        {
            PV.RPC("ChatRPC", targetPlayer, PhotonNetwork.NickName + " : " + ChatInput.text);
            PV.RPC("ChatRPC", PhotonNetwork.LocalPlayer, "<color=blue>"+PhotonNetwork.NickName + " : " + ChatInput.text+"</color>");
            // if(rks.ConveyedChat!=null)
            // {
            //     PV.RPC("ChatRPC", PhotonNetwork.LocalPlayer, "<color=yellow>Please wait...</color>");

            //     while(true)
            //     {
            //         if(rks.ConveyedChat==null)
            //         {
            //             break;
            //         }
            //     }
            // }
            rks.ConveyedChat=ChatInput.text;
            rks.ConveyedPlayer=targetPlayer.NickName;
            // Debug.Log("'"+ChatInput.text+"'가 전달됨");
            ChatInput.text = "";
        }

    }
    public void TargetClick(int i)
    {
        RoomPanel.SetActive(false);
        string Nickname=CellBtn[i].transform.GetChild(0).GetComponent<Text>().text;
        GameObject[] chatbots=GameObject.FindGameObjectsWithTag("NPC");

        foreach(GameObject chat_obj in chatbots)
        {
            if (chat_obj.name==Nickname)
            {
                Debug.Log(Nickname+" 이 선택되었습니다");
                bks=chat_obj.GetComponent<Bot_kafka_send>();
            }
        }
        if(bks==null)
        {
            foreach(Player p in PhotonNetwork.PlayerList)
                {
                    if (p.NickName==Nickname)
                    {
                        targetPlayer=p;
                    }
                }
        }
        
        RoomPanel.SetActive(true);

    }

    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            // Debug.Log("return");
            if(!chaton)
            {
                RoomPanel.SetActive(true);
                chaton=true;
            }
            else
            {
                RoomPanel.SetActive(false);
                chaton=false;
                bks=null;
                targetPlayer=null;
            }
            
        }
    }


    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 입장하셨습니다</color>");
        PV.RPC("PlayerListRenewal",RpcTarget.All);
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        PV.RPC("PlayerListRenewal",RpcTarget.All);
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }
    #endregion

    void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }


    [ContextMenu("정보")]
    void info()
    {
        if(PhotonNetwork.InRoom)
        {
            string playerstr="방에 있는 플레이어 목록 : ";
            for (int i=0; i< PhotonNetwork.PlayerList.Length;i++) playerstr+=PhotonNetwork.PlayerList[i].NickName+",";
            Debug.Log(playerstr);
        }
    }



}
