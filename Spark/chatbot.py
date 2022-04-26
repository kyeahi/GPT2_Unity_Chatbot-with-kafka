mport threading
import pandas as pd
import torch
from transformers import PreTrainedTokenizerFast
import os
from kafka import KafkaConsumer
from kafka import KafkaProducer
from hdfs import InsecureClient
import datetime
from datetime import datetime
from queue import Queue
import sys
from io import BytesIO
import joblib

consumer = KafkaConsumer(
    'bot_question',
    bootstrap_servers=['172.30.1.174:9092'])

producerA = KafkaProducer(
    bootstrap_servers=['172.30.1.174:9092'])
producerB = KafkaProducer(
    bootstrap_servers=['172.30.1.174:9092'])
producerC = KafkaProducer(
    bootstrap_servers=['172.30.1.174:9092'])


client_hdfs = InsecureClient('http://172.30.1.167' + ':9870')

Q_TKN = "<usr>"
A_TKN = "<sys>"
BOS = '</s>'
EOS = '</s>'
MASK = '<unused0>'
SENT = '<unused1>'
PAD = '<pad>'

koGPT2_TOKENIZER = PreTrainedTokenizerFast.from_pretrained("skt/kogpt2-base-v2",
                                                           bos_token=BOS, eos_token=EOS, unk_token='<unk>',
                                                           pad_token=PAD, mask_token=MASK)
botA_message_que = Queue()
botB_message_que = Queue()
botC_message_que = Queue()




# while true하지 않아도 계속 반복됨
def consume_bot_question(consumer):
    for message in consumer:
        question=eval(message.value.decode('utf-8').strip())
        to=str(question['to'])
        if to == "ChatbotA":
            botA_message_que.put(question)
        elif to == "ChatbotB":
            botB_message_que.put(question)
        elif to == "ChatbotC":
            botC_message_que.put(question)


def createDir(pt): #경로 있는지 확인하는 함수, pt에 생성하고자하는 경로를 넣어>줌
    originPath = "/".join(pt.split('/')[:-1])
    targetPath = pt.split('/')[-1]
    fileList = client_hdfs.list(originPath)
    if targetPath not in fileList :
        client_hdfs.makedirs(pt, permission=None)


# path를 설정해주기 위한 변수

i = 1

def consumer_test(path, bot_queue, producer):
    global i
    data_que={'Q':[], 'A':[]}
    #message_que=Queue()
    real_path="/Chatbot/retrained_pt_file/"+path+"/"+str(i)+".pt"
    print(real_path)
    with client_hdfs.read(real_path) as rawdata:
        data = rawdata.read()
        #rawdata.close()
        #model = joblib.load(BytesIO(data))
        model=torch.load(BytesIO(data))
        model.eval()
        i += 1



    # GPT2 모델 테스트
    print('[begin] get consumer list')
    with torch.no_grad():
        try:
            while 1:
                question=bot_queue.get()
                print(path, " > ", question)
                q = question['message']
                a = ""

                while 1:
                    input_ids = torch.LongTensor(koGPT2_TOKENIZER.encode(Q_TKN + q + SENT + A_TKN + a)).unsqueeze(dim=0)
                    pred = model(input_ids)
                    pred = pred.logits
                    gen = koGPT2_TOKENIZER.convert_ids_to_tokens(torch.argmax(pred, dim=-1).squeeze().numpy().tolist())[-1]

                    if gen == EOS:
                        break
                    a += gen.replace("▁", " ")



                if q != 'quit':
                    #message_que.put(question)
                    data_que['Q'].append(q)
                    data_que['A'].append(a)
                    producer.send(topic=question['to'], key=question['from'].encode('utf-8'),  # topic=path
                                  value=a.strip().encode('utf-8'))  # 값이 넘어
가는 부분
                    print("Chatbot > {}".format(a.strip()))
                else:
                    break
        except:
            pass

        filename = datetime.today().strftime('%m%d') + '/' + path + '.csv'

        if question['to'] == path:
            dataframe=pd.DataFrame(data_que)
            #dataframe = pd.DataFrame(list(message_que.queue))
            createDir('/Chatbot/dailydata/'+datetime.today().strftime('%m%d')) #경로가 없으면 생성하는 함수
            with client_hdfs.write('/Chatbot/dailydata/'+filename, overwrite=True, encoding='utf-8') as writer:
                dataframe.to_csv(writer)
            print("finish!")

        #message_que.clear() #queue의 내용을 전체 삭제




# 들어오는 값 계속 받아줘서 ABC분배
thread_kafka_consume=threading.Thread(target=consume_bot_question,args=(consumer,))
thread_kafka_consume.start()

th_chatbotA=threading.Thread(target=consumer_test,args=('ChatbotA',botA_message_que,producerA,))
th_chatbotB=threading.Thread(target=consumer_test,args=('ChatbotB',botB_message_que,producerB,))
th_chatbotC=threading.Thread(target=consumer_test,args=('ChatbotC',botC_message_que,producerC,))
th_chatbotA.start()
th_chatbotB.start()
th_chatbotC.start()

print("start")
