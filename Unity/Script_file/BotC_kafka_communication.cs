using UnityEngine;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class BotC_kafka_communication : MonoBehaviourPunCallbacks
{

    public static string name=null;

    public PhotonView PV;

    public Text[] ChatText;

    void Awake()
    {  
        name=this.gameObject.name;
        Debug.Log(name+"이 존재");

    }

    [System.Serializable]
    public class threadHandle
    {
        ConsumerConfig config;

        public readonly ConcurrentQueue<string[]> _queue=new ConcurrentQueue<string[]>();
        public void StartKafkaListener()
        {
            Debug.Log("Kafka - Starting Thread..");
            try 
            {
                config = new ConsumerConfig{
                          
                        BootstrapServers = "172.30.1.174:9092",
                        GroupId =name,// unique group, so each listener gets all messages

                        // MaxPollIntervalMs=10
                    };

                Debug.Log("Kafka - Created config");

                using (var c = new ConsumerBuilder<string, string>(config).Build()) 
                        {
                            c.Subscribe(name);
                            Debug.Log("Kafka - Subscribed");

                            CancellationTokenSource cts = new CancellationTokenSource();
                            Console.CancelKeyPress += (_, e) => {
                                e.Cancel = true; // prevent the process from terminating.
                                cts.Cancel();
                            };

                            try
                            {
                                // consumer는 데이터가 들어올 때마다 입력받아야 하니깐 무한반복
                                while (true)
                                {
                                    try
                                    {
                                        Debug.Log("trying.."); 
                                        // Waiting for message
                                        var cr = c.Consume(cts.Token);
                                        // Got message! Decode and put on queue
                                        
                                        string target=cr.Key;
                                        string message = cr.Value;
                                        string[] queue_message=new string[2]{target,message}; 
                                        Debug.Log(target+" 에게 '"+message+"' 전달");
                                        _queue.Enqueue(queue_message);
                                    }
                                    catch (ConsumeException e)
                                    {
                                        Debug.Log("Kafka - Error occured: " + e.Error.Reason);
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Debug.Log("Kafka - Canceled..");
                                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                                c.Close();
                            }
                        }             
            }
            catch (Exception ex) {
                Debug.Log("Kafka - Received Expection: " + ex.Message + " trace: " + ex.StackTrace);
            }
        }
    }
    bool kafkaStarted = false;
    Thread kafkaThread;
    threadHandle _handle;

    void Start()
    {
        StartKafkaThread();
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Cancelling Kafka!");
            StopKafkaThread();
        }

        ProcessKafkaMessage();
    }

    void OnDisable()
    {
        StopKafkaThread();        
    }
    void OnApplicationQuit()
    {
        StopKafkaThread();
    }

    public void StartKafkaThread()
    {
        if(kafkaStarted) return;

        _handle = new threadHandle();
        kafkaThread = new Thread(_handle.StartKafkaListener);

        kafkaThread.Start();
        kafkaStarted = true;
        // StartKafkaListener(config);
    }
    private void ProcessKafkaMessage()
    {
        if (kafkaStarted)
        {
            string[] message;
            while (_handle._queue.TryDequeue(out message))
            {
                // Debug.Log(message);
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (p.NickName==message[0])
                    {
                        Debug.Log(message[1]);
                        PV.RPC("ChatRPC", p, "<color=blue> "+ name+" : " + message[1]+"</color>");
                    }
                }
                

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

    void StopKafkaThread()
    {
        if(kafkaStarted)
        {
            kafkaThread.Abort();
            kafkaThread.Join();
            kafkaStarted = false;
        }
    }
}
