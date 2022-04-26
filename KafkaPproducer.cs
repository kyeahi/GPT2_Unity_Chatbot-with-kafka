using UnityEngine;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;


class KafkaPproducer : MonoBehaviour
{
    [System.Serializable]
    public class threadHandle
    {
        ProducerConfig config;

        public async void StartKafkaListener()
        {
            Debug.Log("Kafka - Starting Thread..");
            try 
            {
                config = new ProducerConfig{
                          
                        BootstrapServers = "172.30.1.174:9092",
                    };

                Debug.Log("Kafka - Created config");

                using (var p = new ProducerBuilder<Null, string>(config).Build()) 
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            Console.CancelKeyPress += (_, e) => {
                                e.Cancel = true; // prevent the process from terminating.
                                cts.Cancel();
                            };

                            try
                            {
                                var dr=await p.ProduceAsync("test-topic",new Message<Null,string>{Value="test"});
                                Debug.Log($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                                
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
