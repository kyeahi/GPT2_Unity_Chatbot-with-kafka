using UnityEngine;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;


class KafkaCconsumer : MonoBehaviour
{
    [System.Serializable]
    public class threadHandle
    {
        ConsumerConfig config;

        public readonly ConcurrentQueue<string> _queue=new ConcurrentQueue<string>();
        public void StartKafkaListener()
        {
            Debug.Log("Kafka - Starting Thread..");
            try 
            {
                config = new ConsumerConfig{
                          
                        BootstrapServers = "172.30.1.174:9092",
                        GroupId ="foo",// unique group, so each listener gets all messages

                        // MaxPollIntervalMs=10
                    };

                Debug.Log("Kafka - Created config");

                using (var c = new ConsumerBuilder<Ignore, byte[]>(config).Build()) 
                        {
                            c.Subscribe("simba2");
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
                                        string message = Encoding.UTF8.GetString(cr.Value);
                                        Debug.Log(message);
                                        _queue.Enqueue(message);
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
            string message;
            while (_handle._queue.TryDequeue(out message))
            {
                Debug.Log(message);
            }
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

