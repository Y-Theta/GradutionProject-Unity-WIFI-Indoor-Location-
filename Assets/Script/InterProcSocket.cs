using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// <summary>
/// 收信回调
/// </summary>
public delegate void MessageReceivedEventHandle(string msg);

public class InterProcSocket {

    public static Lazy<InterProcSocket> Singleton = new Lazy<InterProcSocket>(() => new InterProcSocket());

    /// <summary>
    /// 连接状态字
    /// </summary>
    public bool Connected {
        get {
            return _client == null ? false : _client.Connected;
        }
    }

    /// <summary>
    /// 用于维持通信的客户端
    /// </summary>
    private Socket _client;

    /// <summary>
    /// 用于收发的端口号
    /// </summary>
    private const int _port = 20191;

    /// <summary>
    /// 
    /// </summary>
    private const int _timeout = 5000;

    /// <summary>
    /// 接收缓存
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    /// 客户端主动断开时发送报文
    /// </summary>
    private const string _disconnectack = "--DISCONNECT--";
    public static string DISCONNECT {
        get { return _disconnectack; }
    }

    /// <summary>
    /// 心跳信息
    /// </summary>
    private const string _hbpara = "--WAKE--";
    public static string HERTBEAT {
        get { return _disconnectack; }
    }
    /// <summary>
    /// 关联此事件以处理收到的TCP报文
    /// </summary>
    public event MessageReceivedEventHandle MessageReceived;

    /// <summary>
    /// 发送数据
    /// </summary>
    public void PostData(string data) {
        _client.Send(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init() {
        _client = null;
        _buffer = null;
        _buffer = new byte[4096];
        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _client.ReceiveTimeout = _timeout;
        _client.SendTimeout = _timeout;
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public bool Connect() {
        lock (_client) {
            Init();

            _client.Connect(new IPAddress(new byte[] { 219, 216, 64, 135 }), _port);
            _client.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(OnMessageReceived), null);
            return _client.Connected;
        }
    }


    /// <summary>
    /// 连接指定地址服务器
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public bool Connect(string addr) {
        lock (_client) {
            Init();

            _client.Connect(IPAddress.Parse(addr), _port);
            _client.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(OnMessageReceived), null);
            return _client.Connected;
        }
    }

    /// <summary>
    /// 处理服务端消息
    /// </summary>
    private void OnMessageReceived(IAsyncResult e) {
        try {
            int REnd = _client.EndReceive(e);
            if (REnd > 0) {
                byte[] data = new byte[REnd];
                Array.Copy(_buffer, 0, data, 0, REnd);
                string rec = Encoding.ASCII.GetString(data);
                //在此次可以对data进行按需处理
                MessageReceived?.Invoke(rec);
                _client.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(OnMessageReceived), null);
            }
        }
        catch (SocketException) {
            _client.Close();
            _client = null;
        }
    }

    /// <summary>
    /// 程序结束时向服务器发送离线消息
    /// </summary>
    ~InterProcSocket() {
        PostData(_disconnectack);
        _client.Close();
    }

    /// <summary>
    /// 
    /// </summary>
    public InterProcSocket() { Init(); }
}
