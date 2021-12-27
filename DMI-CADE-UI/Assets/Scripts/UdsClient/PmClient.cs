using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace UdsClient
{    
    class StateObject {
        public Socket socket = null;
        public const int BufferSize = 256;
        public byte[] byteBuffer = new byte[BufferSize];
    }

    public class ConnectedEventArgs : EventArgs {}
    public class DisconnectedEventArgs : EventArgs {}
    public class MessageReceivedEventArgs : EventArgs { public string Msg { get; set; } }
    public class MessageSendEventArgs : EventArgs { public string Msg { get; set; } }

    /// <summary>
    /// Class for connecting to a unix domain socket server from unity.
    /// </summary>
    /// This class is meant to be used on a linux operating system.
    /// <example>
    /// Cache a new instance of <see cref="PmClient"/> somewhere consistently.
    /// Subscribe to necessary events.
    /// In Unity run this function by calling <code>StartCoroutine(nameof(RunClient));</code> where 'RunClient' is a
    /// function like: <code>private IEnumerator RunClient() { yield return _pmClient.Run(); }</code> and '_pmClient'
    /// is the cached instance of <see cref="PmClient"/>.
    /// Starts looking for the server, invoking <see cref="OnConnected"/> when connected.
    /// Invokes <see cref="OnMessageReceived"/> when a message was received.
    /// Use <see cref="Send"/> for sending a message to server.
    /// Disconnects when connection is lost (ended by server) or <see cref="Disconnect"/> is called after being
    /// connected.
    /// </example>
    class PmClient
    {
        private class StartSendEventArgs : EventArgs { public string Msg { get; set; } }
        
        public event EventHandler<ConnectedEventArgs> OnConnected;

        /// Invoked when disconnected.
        public event EventHandler<DisconnectedEventArgs> OnDisconnected;

        /// Invoked when a message was received.
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        /// Invoked when a message was send.
        public event EventHandler<MessageSendEventArgs> OnMessageSend;

        /// Invoked when starting internal asynchronous sending process. Use <see cref="OnMessageSend"/> for
        /// getting notified when the client sent a message.
        private event EventHandler<StartSendEventArgs> _onStartSend;

        public bool isConnected { get; private set; } = false;
        private readonly string _socketPath;
        private Socket _client;
        
        public PmClient( string socketPath )
        {
            this._socketPath = socketPath;
        }

        /// <summary>
        /// The asynchronous function connecting and maintaining the connection to a Unix Domain Socket Server.
        /// </summary>
        /// It is recommended to only run this function in a linux environment.
        /// <example><see cref="PmClient"/></example>
        /// <param name="connectingRetryDelay"></param>
        /// <returns></returns>
        public IEnumerator Run(float connectingRetryDelay = 1f)
        {
            // var endPoint = new UnixDomainSocketEndPoint(this._socketPath);
            var endPoint = new UnixEndPoint(this._socketPath);

            bool wasConnected = false;

            while (!wasConnected)
            {
                // Debug.Log("Connecting...");
                
                _client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                
                try 
                {
                    _client.Connect(endPoint);
                    wasConnected = true;
                }
                catch (SocketException)
                {
                    // Debug.Log("Connecting failed...");
                    // Debug.Log(exception.Message);
                }

                if (!wasConnected) {
                    yield return new WaitForSeconds(connectingRetryDelay);
                    continue;
                }

                Debug.Log("Connected to: " + this._socketPath);

                wasConnected = true;
                Connected();

                // Start asyc receive callbacks.
                StartAsyncReceive(_client);
                
                // Add anonymous function to internal event handler.
                _onStartSend += (object source, StartSendEventArgs args) => { StartAsyncSend(source, args, _client); };

                while (isConnected) {
                    yield return null;
                }
            }

            // Debug.Log("[CLIENT] Run end.");
        }

        private void StartAsyncReceive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.socket = client;
                //Debug.Log("[RECEIVE] Beginn Receive...");
                client.BeginReceive(state.byteBuffer, 0, StateObject.BufferSize, 0, 
                    new AsyncCallback(ReceiveCallback), state);   
            }
            catch (Exception)
            {
                //Console.Write("Caught: " + e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            //Debug.Log("[RECEIVE CALLBACK] Start...");
            try
            {
                // Retrieve state from asynchronous state object.
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.socket;

                // Read data.
                int bytesRead = client.EndReceive(ar);

                if(bytesRead > 0){
                    //Debug.Log("[RECEIVE CALLBACK] Received bytes...");
                    String msg = Encoding.ASCII.GetString(state.byteBuffer, 0, bytesRead);
                    //Debug.Log($"[RECEIVE CALLBACK] Msg received: {msg}");
                    MessageReceived(msg);

                    client.BeginReceive(state.byteBuffer, 0, StateObject.BufferSize, 0, 
                        new AsyncCallback(ReceiveCallback), state);

                } else {
                    //Debug.Log("[RECEIVE CALLBACK] Received no bytes.");
                    Disconnect();
                }

            }
            catch (Exception)
            {
                //Debug.Log("Caught: " + e.ToString());
            }
            //Debug.Log("[RECEIVE CALLBACK] Done.");
        }

        public void Send(string message)
        {
            StartSend(message);
            MessageSend(message);
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        public void Disconnect()
        {
            // Clear internal event handler.
            if (_onStartSend != null) {
                foreach(Delegate d in _onStartSend.GetInvocationList()) {
                    _onStartSend -= (EventHandler<StartSendEventArgs>) d;
                }
            }

            Disconnected();

            if (_client != null)
                _client.Dispose();
        }

        private void StartAsyncSend(object source, StartSendEventArgs args, Socket client)
        {
            //Debug.Log("[SEND] Sending Msg...");

            byte[] byteData = Encoding.ASCII.GetBytes(args.Msg);

            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendAsyncCallback), client);
        }

        private void SendAsyncCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;

                int bytesSend = client.EndSend(ar);

                //Debug.Log($"[SEND CALLBACK] Done sending {bytesSend} bytes.");
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        protected virtual void Connected()
        {
            //Debug.Log("Invoke 'OnConnect'...");
            isConnected = true;
            OnConnected?.Invoke(this, new ConnectedEventArgs());
        }

        protected virtual void Disconnected()
        {
            //Debug.Log("Invoke 'OnDisconnect'...");
            isConnected = false;
            OnDisconnected?.Invoke(this, new DisconnectedEventArgs());
        }

        protected virtual void MessageReceived(string message)
        {
            //Debug.Log($"Invoke 'OnMessageReceived': {message}");
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(){ Msg = message });
        }

        protected virtual void MessageSend(string message)
        {
            //Debug.Log($"Invoke 'OnMessageSend' {message}");
            OnMessageSend?.Invoke(this, new MessageSendEventArgs(){ Msg = message });
        }
        protected virtual void StartSend(string message)
        {
            //Debug.Log("Invoke 'OnStartSend'...");
            _onStartSend?.Invoke(this, new StartSendEventArgs(){ Msg = message });
        }

        private void PrintSocketState(Socket socket)
        {
            Debug.Log($"socket not null: {socket != null}");
            Debug.Log($"socket connected: {socket?.Connected}");
        }
    }
}