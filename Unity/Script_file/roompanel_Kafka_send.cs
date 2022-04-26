using UnityEngine;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Text;

public class roompanel_Kafka_send : MonoBehaviourPunCallbacks
{
    public InputField ChatInput;
    public string ConveyedChat=null;
    public string ConveyedPlayer=null;
    public readonly static ConcurrentQueue<string> _queue=new ConcurrentQueue<string>();

    [System.Serializable]
    public class threadHandle
    {
        ProducerConfig config;

        public async void StartKafkaListener(ConcurrentQueue<string> _queue)
        {
            Debug.Log("Kafka - Starting Thread..");
            try 
            {
                config = new ProducerConfig{
                          
                        BootstrapServers = "172.30.1.174:9092", //broker ubuntu ip
                    };

                Debug.Log("Kafka - Created config"); //여기까지는 옴

                using (var p = new ProducerBuilder<Null, string>(config).Build()) 
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            Console.CancelKeyPress += (_, e) => {
                                e.Cancel = true; // prevent the process from terminating.
                                cts.Cancel();
                            };

                            try
                            {
                                while(true)          
                                {    
                                    string message;
                                    // Debug.Log("Dequeue전");
                                    
                                    while(_queue.TryDequeue(out message))
                                    {
                                        // Debug.Log("Dequeue중");
                                        try
                                        {   
                                            var dr=await p.ProduceAsync("question", new Message <Null,string>{Value=message});
                                            Debug.Log($"Delivered '{dr.Value}' to '{dr.Partition}'");
                                        }
                                        catch(ProduceException<string,string> e)
                                        {
                                            Debug.Log("Kafka - Error occured: "+e.Error.Reason);
                                            
                                        }
                                    }
                                    // Debug.Log("Dequeue후");
                                }
                                
                                
                            }
                            catch (ProduceException<Null,string> e)
                            {
                                Debug.Log($"Delivery failed: {e.Error.Reason}");
                            }
                        }             
            }
            catch (Exception ex) {
                Debug.Log("Kafka - Received Expection: " + ex.Message + " trace: " + ex.StackTrace);
            }
        }
    }


    // public void btn_click()
    // {  
        // string[] message={PhotonNetwork.NickName, ChatInput.text};
        // _queue.Enqueue(message);

    // }

    bool kafkaStarted = false;
    Thread kafkaThread;
    threadHandle _handle;
    void Awake()
    {
        ChatInput.text="";
    }
    void Start()
    {
        StartKafkaThread(_queue);
    }
    void Update()
    {
        if(ConveyedChat!=null)
        {
            string message="{'from' : '"+PhotonNetwork.NickName+"', 'to' : '"+ConveyedPlayer+"', 'message' : '"+ConveyedChat+"'}";
            
            if (ConveyedChat==null || ConveyedChat==""){ }
            else
            {
                _queue.Enqueue(message);
            }
            
            ConveyedChat=null;
        }

        
        if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Cancelling Kafka!");
            StopKafkaThread();
        }

    }


    public void StartKafkaThread(ConcurrentQueue<string> _queue)
    {
        if(kafkaStarted) return;

        _handle = new threadHandle();
        // kafkaThread=new Thread(new ParameterizedThreadStart(_handle.StartKafkaListener));
        kafkaThread=new Thread(()=> _handle.StartKafkaListener(_queue));

        kafkaThread.Start();
        kafkaStarted = true;
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

    void OnDisable()
    {
        StopKafkaThread();        
    }
    void OnApplicationQuit()
    {
        StopKafkaThread();
    }
}
