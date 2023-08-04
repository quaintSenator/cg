using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using TCCamp;
using UnityEngine;
using UnityEngine.Events;

public class Network : MonoBehaviour {
    
    //将协议号与返回的消息类型对应上
    private readonly Dictionary<int,Type> _responseMsgDic = new Dictionary<int, Type>() {
        {(int)SERVER_CMD.ServerAddRsp,typeof(AddRsp)},
        {(int)SERVER_CMD.ServerLoginRsp,typeof(PlayerLoginRsp)},
        {(int)SERVER_CMD.ServerCreateRsp,typeof(PlayerCreateRsp)},//创建回包 = 1005
        {(int)SERVER_CMD.ShopContentRsp,typeof(ShopContentRsp) },//请求商店内容回包 = 1007
        {(int)SERVER_CMD.BackpackContentRsp,typeof(BackPackContentRsp) },//服务器背包内容回包
         {(int)SERVER_CMD.ServerBuyItemRsp,typeof(BuyItemRsp) }//服务器购买物品回包
    };
    
    public struct NetMsg {
        public int      cmd;
        public IMessage msg;
    }
    
    private const float         DEFINE_RECEIVE_INTERVAL = 0.1f;
    
    public        string        staInfo                 = "NULL";      //状态信息
    public        string        ip                      = "127.0.0.1"; //输入ip地址
    public        int           port                    = 8086;        //输入端口号
    
    private int                 recTimes  = 0;      //接收到信息的次数
    private string              recMes    = "NULL"; //接收到的消息
    private TcpClient           tcpClient = null;
    private bool                clickSend = false; //是否点击发送按钮
    private Queue<NetMsg>       receiveQueue;      //服务器消息接收队列
    private float               timeCal = 0f;
    private byte[]              _headBytes;

    public UnityAction<NetMsg> recvCallback { get; set; } = null;

    private void Start() {
        if (_headBytes == null)
        {
            char[] head = new[] {'T', 'C'};
            _headBytes = Encoding.Default.GetBytes(head); 
        }

        recvCallback = null;
        ConnectToServer();
    }

    private void OnDestroy() {
        if (tcpClient.Connected) {
            tcpClient.Close();
        }
    }

    private void Update() {
        timeCal += Time.deltaTime;
        if (timeCal >= DEFINE_RECEIVE_INTERVAL) {
            timeCal -= DEFINE_RECEIVE_INTERVAL;
            RecvMsg();
        }
        
        while (receiveQueue != null && receiveQueue.Count >0) {
            NetMsg msg = receiveQueue.Dequeue();
            EventModule.Instance.Dispatch(msg.cmd, msg.msg);
        }
    }

    private void ConnectToServer() {
        timeCal = 0f;

        try {
            if (tcpClient == null) tcpClient = new TcpClient();
            IPAddress  ipaddress             = IPAddress.Parse(ip);
            IPEndPoint point                 = new IPEndPoint(ipaddress, port);
            tcpClient.Connect(point); //通过IP和端口号来定位一个所要连接的服务器端
            Debug.Log("连接成功 , " + " ip = " + ip + " port = " + port);
            staInfo = ip + ":" + port + "  连接成功";

            receiveQueue = new Queue<NetMsg>();

            //AddReq req = new AddReq();
            //req.A = 1;
            //req.B = 2;
            //SendMsg((int) CLIENT_CMD.ClientAddReq, req);
        }
        catch (Exception) {
            Debug.Log("IP或者端口号错误......");
            staInfo = "IP或者端口号错误......";
        }
    }

    public void SendMsg(int cmd, IMessage msg) {
        if (tcpClient.Connected == false) {
            Debug.LogError("Send Message Failed! Tcp not connected!");
            return;
        }

        byte[] body = msg.ToByteArray();

        Int16  length     = (Int16)(body.Length + 2);
        byte[] lengthByte = BitConverter.GetBytes(length);
        
        byte[] cmdByte = BitConverter.GetBytes((Int16)cmd);
        
        int    packageLength = 4 + length;
        byte[] package       = new byte[packageLength];
        Buffer.BlockCopy(_headBytes, 0, package, 0, _headBytes.Length);
        Buffer.BlockCopy(lengthByte, 0, package, 2, lengthByte.Length);
        Buffer.BlockCopy(cmdByte,    0, package, 4, cmdByte.Length);
        Buffer.BlockCopy(body,       0, package, 6, body.Length);
        var stream = tcpClient.GetStream();
        stream.Write(package);
        stream.Flush();
        //stream.Close();
    }

    private void RecvMsg() {
        if (tcpClient.Connected == false) {
            //Debug.LogError("Receive Message Failed! Tcp not connected!");
            return;
        }

        int    readSoFar = 0;
        var    stream    = tcpClient.GetStream();
        byte[] buffer    = new byte[1024];
        while (stream.DataAvailable) {
            int readCount = stream.Read(buffer, 0, buffer.Length);
            if (readCount > 0)
            {
                printBytes(buffer);
                EnqueueMsg(buffer);
            }
                
        }
    }

    private void EnqueueMsg(byte[] buffer) {
        byte[] headBytes   = new byte[2];
        byte[] lengthBytes = new byte[2];
        byte[] cmdBytes    = new byte[2];
        
        Buffer.BlockCopy(buffer, 0, headBytes, 0, 2);
        if (headBytes[0] == this._headBytes[0] && headBytes[1] == this._headBytes[1])
        {
            Buffer.BlockCopy(buffer, 2, lengthBytes, 0, 2);
            int length = bytesToInt16(lengthBytes, 0);
            Buffer.BlockCopy(buffer, 4, cmdBytes, 0, 2);
            int cmd = bytesToInt16(cmdBytes, 0);
                
            byte[] body = new byte[length -2];
            Buffer.BlockCopy(buffer, 6, body, 0, body.Length);

            Type tp;
            if (_responseMsgDic.TryGetValue(cmd, out tp))
            {
                IMessage msg = (IMessage)Activator.CreateInstance(tp);
                msg.MergeFrom(body);
                NetMsg netMsg;
                netMsg.cmd = cmd;
                netMsg.msg = msg;
                receiveQueue.Enqueue(netMsg);
            } 
        }
    }

    public static Int16 bytesToInt16(byte[] src, int offset) {  
        Int16 value;    
        value = (Int16) ((src[offset] & 0xFF)   
                         | ((src[offset +1] & 0xFF) <<8));  
        return value;  
    }
    public void printBytes(byte[] bytearray)
    {
        string s = "";
        foreach (byte b in bytearray)
        {
            s += " ";
            s += b.ToString("X");
        }
        Debug.Log(s);
    }
}
