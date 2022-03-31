# 메타버스 가상 공간 속, KoGPT2 모델을 활용한 챗봇 (GPT2_Unity_Chatbot-with-kafka)  
✔ Unitiy 가상공간 속에서 KoGPT2 모델 기반의 챗봇을 카프카와 연동시켜 구현하였다.   
✔ KoGPT2 모델이란, 주어진 단어를 통해 다음 단어를 예측하는 학습모델이다. 이는 문장 생성에 최적화되어 있다.

## 사용 프로그램      
   <a href="/README.md#unity--photon"><img src="https://img.shields.io/badge/Unity & Photon-a4c5f3?style=flat-square&logo=Unity&logoColor=black"/></a>
   <a href="/README.md#apache-kafka"><img src="https://img.shields.io/badge/Apache Kafka-ff9b3b?style=flat-square&logo=apache kafka&logoColor=white"/></a>
   <a href="/README.md#python-machine-learning"><img src="https://img.shields.io/badge/Python-52c72e?style=flat-square&logo=python&logoColor=white"/></a>
   <a href="/README.md#apache-hadoop"><img src="https://img.shields.io/badge/Apache Hadoop-ffd966?style=flat-square&logo=apache hadoop&logoColor=black"/></a>
   <a href="/README.md#apache-spark"><img src="https://img.shields.io/badge/Apache Spark-674ea7?style=flat-square&logo=apache spark&logoColor=white"/></a>
   <a href="/README.md#"><img src="https://img.shields.io/badge/Elasticsearch-0762d7?style=flat-square&logo=Elasticsearch&logoColor=white"/></a>
   <a href="/README.md#unity--photon"><img src="https://img.shields.io/badge/Logstash-f85c5c?style=flat-square&logo=logstash&logoColor=white"/></a>
   <a href="/README.md#unity--photon"><img src="https://img.shields.io/badge/Kibana-8fce00?style=flat-square&logo=kibana&logoColor=white"/></a>   
   
   
## 구현 기능  
✔ Unity 가상공간에서    

## 구성도
<img width="65%" src="https://user-images.githubusercontent.com/50973139/160983406-2f64d241-2593-45e1-b540-a86a5a3e6d50.png"/>

     
## 시스템 (환경설정)
|시스템|CPU|메모리|HDD|프로그램 버전|비고|
|------|---|---|------|---|---|
|Kafka broker|1|2.0GB|20GB|2.13|Vmware|
|Kafka Producer 1|2|16.0GB|100GB|2.13|Vmware|
|Kafka Consumer 1|3.60GHz|4.0GB|20GB|2.13|Vmware|
|Kafka Producer 2|1|1GB|20GB|2.13|Vmware|
|Kafka Consumer 2|3.60GHz|1.0GB|20GB|2.13|Vmware|
|Spark master|4|16.0GB|422GB|3.1.3|컴퓨터|
|Spark worker1|2|8.0GB|50GB|3.1.3|Vmware|
|Spark worker2|2|8.0GB|50GB|3.1.3|Vmware|
|Hadoop master|4|16.0GB|421.0GB|3.2.2|컴퓨터|
|Hadoop worker1|2|8.0GB|50.0GB|3.2.2|Vmware|
|Hadoop worker2|2|8.0GB|50.0GB|3.2.2|Vmware|
|Unity|2|16.0GB|100GB|2020.3.30f1|노트북|
|Photon|2|16.0GB|100GB|photon unity networking2|노트북|
|Logstash|4|8.0GB|100GB|7.17.1|노트북|
|Elasticsearch|2|4.0GB|20GB|7.17.1|Vmware|
|Kibana|2|4.0GB|20GB|7.17.1|Vmware|
   
🔹 Producer 1과 Consumer 1은 질문을 받을 때, Producer 2와 Consumer 2는 답변을 보낼 때 사용   


## Unity & Photon
* 어떤 기능을 사용하였는지, 어떤 언어를 사용했는디
* 구현 기능이 무엇인지
* 상세하게 적기   

## Apache Kafka
|Topic|Producer|Consumer|Context|비고|
|------|---|---|------|---|
|Question|Unity Server|Machine learning test|클라이언트 질문|테스트2|
|Answer|Machine learning test|Unity Server|챗봇 답변|테스트2|

## Hadoop & Spark

## Python (Machine Learning)


## 참조
* https://github.com/SKT-AI/KoGPT2
* https://github.com/songys/Chatbot_data
