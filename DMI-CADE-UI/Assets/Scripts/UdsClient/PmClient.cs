using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

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

    class PmClient
    {
        private class StartSendEventArgs : EventArgs { public string Msg { get; set; } }
        
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<MessageSendEventArgs> MessageSend;
        private event EventHandler<StartSendEventArgs> _StartSend;

        private ManualResetEvent clientDone = new ManualResetEvent(false);
        public bool isConnected { get; private set; } = false;
        private string _socketPath = "";
        
        public PmClient( string socketPath )
        {
            this._socketPath = socketPath;
        }

        public void Run(int timeout, int connectingRetryDelayMs = 1000)
        {
            clientDone.Reset();

            // var endPoint = new UnixDomainSocketEndPoint(this._socketPath);
            var endPoint = new UnixEndPoint(this._socketPath);

            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            bool wasConnected = false;

            while (!wasConnected)
            {
                Console.WriteLine("Connecting...");

                try
                {
                    using (Socket client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                    {

                        client.Connect(endPoint);
                        Console.WriteLine("Connected to: " + this._socketPath);

                        wasConnected = true;
                        OnConnected();

                        // Start asyc receive callbacks.
                        StartAsyncReceive(client);

                        // Add anonymous function to internal event handler.
                        _StartSend += (object source, StartSendEventArgs args) => { StartAsyncSend(source, args, client); };

                        clientDone.WaitOne();

                        // Clear internal event handler.
                        foreach(Delegate d in _StartSend.GetInvocationList()) {
                            _StartSend -= (EventHandler<StartSendEventArgs>) d;
                        }

                        OnDisconnected();
                    }
                }
                catch (SocketException exception)
                {
                    Console.WriteLine(exception.Message);
                }

                if(sWatch.Elapsed.Seconds > timeout) 
                {
                    Console.WriteLine("Connection Timeout.");
                    break;
                }

                if(!wasConnected)
                {
                    //System.Console.WriteLine("Waiting " + connectingRetryDelayMs + "ms... " + sWatch.Elapsed);
                    Thread.Sleep(connectingRetryDelayMs);
                }
            }

            sWatch.Stop();
            //Console.WriteLine("[CLIENT] Run end.");
        }

        private void StartAsyncReceive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.socket = client;
                //System.Console.WriteLine("[RECEIVE] Beginn Receive...");
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
            //System.Console.WriteLine("[RECEIVE CALLBACK] Start...");
            try
            {
                // Retrieve state from asynchronous state object.
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.socket;

                // Read data.
                int bytesRead = client.EndReceive(ar);

                if(bytesRead > 0){
                    //System.Console.WriteLine("[RECEIVE CALLBACK] Received bytes...");
                    String msg = Encoding.ASCII.GetString(state.byteBuffer, 0, bytesRead);
                    //System.Console.WriteLine($"[RECEIVE CALLBACK] Msg received: {msg}");
                    OnMessageReceived(msg);

                    client.BeginReceive(state.byteBuffer, 0, StateObject.BufferSize, 0, 
                        new AsyncCallback(ReceiveCallback), state);

                } else {
                    //System.Console.WriteLine("[RECEIVE CALLBACK] Received no bytes.");
                    clientDone.Set();
                }

            }
            catch (Exception)
            {
                //Console.WriteLine("Caught: " + e.ToString());
            }
            //System.Console.WriteLine("[RECEIVE CALLBACK] Done.");
        }

        public void Send(string message)
        {
            OnStartSend(message);
            OnMessageSend(message);
        }

        public void Disconnect()
        {
            clientDone.Set();
        }

        private void StartAsyncSend(object source, StartSendEventArgs args, Socket client)
        {
            //System.Console.WriteLine("[SEND] Sending Msg...");

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

                //System.Console.WriteLine($"[SEND CALLBACK] Done sending {bytesSend} bytes.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected virtual void OnConnected()
        {
            //System.Console.WriteLine("Invoke 'OnConnect'...");
            isConnected = true;
            Connected?.Invoke(this, new ConnectedEventArgs());
        }

        protected virtual void OnDisconnected()
        {
            //System.Console.WriteLine("Invoke 'OnDisconnect'...");
            isConnected = false;
            Disconnected?.Invoke(this, new DisconnectedEventArgs());
        }

        protected virtual void OnMessageReceived(string message)
        {
            //System.Console.WriteLine("Invoke 'OnMessageReceived'...");
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(){ Msg = message });
        }

        protected virtual void OnMessageSend(string message)
        {
            //System.Console.WriteLine("Invoke 'OnMessageSend'...");
            MessageSend?.Invoke(this, new MessageSendEventArgs(){ Msg = message });
        }
        protected virtual void OnStartSend(string message)
        {
            //System.Console.WriteLine("Invoke 'OnStartSend'...");
            _StartSend?.Invoke(this, new StartSendEventArgs(){ Msg = message });
        }

        private void PrintSocketState(Socket socket)
        {
            System.Console.WriteLine($"socket not null: {socket != null}");
            System.Console.WriteLine($"socket connected: {socket.Connected}");
        }
    }
}