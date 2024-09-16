using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine.UI;
using System;

namespace GAG.UDPLEDControlSystem
{
    public class ServerManager : MonoBehaviour
    {
        [SerializeField] ServerUIManager _serverUIManager;

        NetworkDriver _driver;
        NativeList<NetworkConnection> _connections;

        bool _isSendData = false;

        void Start()
        {
            InitServer();
        }

        void Update()
        {
            UpdateServer();
        }

        void OnDestroy()
        {
            ShutdownServer();
        }


        // Initiate the networkDriver and connection list
        void InitServer()
        {
            _serverUIManager.PrintConsole("I'm Server");

            _driver = NetworkDriver.Create();

            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);

            if (_driver.Bind(endpoint) != 0)
            {
                _serverUIManager.PrintConsole("Failed to bind to port 7777.");
                Debug.LogError("Failed to bind to port 7777.");
                return;
            }
            _driver.Listen();

            _connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        }

        void ShutdownServer()
        {
            if (_driver.IsCreated)
            {
                _driver.Dispose();
                _connections.Dispose();
            }
        }

        void UpdateServer()
        {
            if (_isSendData)
            {
                _driver.ScheduleUpdate().Complete();

                CleanupConnections();
                AcceptNewConnections();
                UpdateMessages();
            }
        }

        // Clean up connections.
        void CleanupConnections()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
               // _serverUIManager.PrintConsole("Cleanup Connections");

                if (!_connections[i].IsCreated)
                {
                    _connections.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }

        // Accept new connections.
        void AcceptNewConnections()
        {
            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                _connections.Add(c);
               // _serverUIManager.PrintConsole("Accepted a connection.");
                Debug.Log("Accepted a connection.");
            }
        }

        public void UpdateMessages()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                //_serverUIManager.PrintConsole("3rd part");

                //DataStreamReader stream;
                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        ulong ulongSendNumber = stream.ReadUInt();
                        foreach (GameObject selectedLED in _serverUIManager.SelectedLEDButtons)
                        {
                            Color32 selectedColor = selectedLED.transform.GetChild(0).GetComponent<Image>().color;
                            string r = selectedColor.r.ToString();
                            string g = selectedColor.g.ToString();
                            string b = selectedColor.b.ToString();
                            //string stringHexColor = ColorUtility.ToHtmlStringRGBA(selectedColor);
                            ///string stringHexColor = ColorUtility.ToHtmlStringRGB(selectedColor);

                            //print("ID: " + selectedLED.name + " Color: " + stringHexColor);
                            print("ID: " + selectedLED.name + " R: " + r + " G: " + g + " B: " + b);
                            //_serverUIManager.PrintConsole("ID: " + selectedLED.name + " Color: " + stringHexColor);

                            ///ulong ulongHexColor = Convert.ToUInt64(stringHexColor, 16);

                            string lEDNumber = selectedLED.name;
                            if (lEDNumber.Length == 1)
                            {
                                lEDNumber = "10" + lEDNumber;
                            }
                            else if (lEDNumber.Length == 2)
                            {
                                lEDNumber = "1" + lEDNumber;
                            }

                            if (r.Length == 1)
                            {
                                r = "99" + r;
                            }
                            else if (r.Length == 2)
                            {
                                r = "9" + r;
                            }


                            if (g.Length == 1)
                            {
                                g = "99" + g;
                            }
                            else if (g.Length == 2)
                            {
                                g = "9" + g;
                            }


                            if (b.Length == 1)
                            {
                                b = "99" + b;
                            }

                            else if (b.Length == 2)
                            {
                                b = "9" + b;
                            }


                            //string bindedName = lEDNumber + ulongHexColor;
                            string bindedName = lEDNumber + r + g + b;
                            _serverUIManager.PrintConsole(bindedName.ToString());

                            ulongSendNumber = Convert.ToUInt64(bindedName);
                            _serverUIManager.PrintConsole(ulongSendNumber.ToString());
                            _driver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                            writer.WriteULong(ulongSendNumber);
                            _driver.EndSend(writer);
                        }
                        _connections[i].Disconnect(_driver);
                        _driver.ScheduleFlushSend(default).Complete();
                        _isSendData = false;

                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        _serverUIManager.PrintConsole("Client disconnected from the server.");
                        Debug.Log("Client disconnected from the server.");
                        _connections[i] = default;
                        break;
                    }
                }
            }
        }

        public void OnData(DataStreamReader stream, int i)
        {
            //ulong ulongSendNumber = stream.ReadUInt();
            foreach (GameObject selectedLED in _serverUIManager.SelectedLEDButtons)
            {
                ulong ulongSendNumber = stream.ReadULong();

                Color selectedColor = selectedLED.transform.GetChild(0).GetComponent<Image>().color;
                string stringHexColor = ColorUtility.ToHtmlStringRGB(selectedColor);

                //print("ID: " + selectedLED.name + " Color: " + stringHexColor);
                //_serverUIManager.PrintConsole("ID: " + selectedLED.name + " Color: " + stringHexColor);

                //uint number0 = Convert.ToUInt32(stringHexColor, 16);
                ulong ulongHexColor = Convert.ToUInt64(stringHexColor, 16);

                string lEDNumber = selectedLED.name;
                if (lEDNumber.Length == 1)
                {
                    lEDNumber = "10" + lEDNumber;
                }
                else if (lEDNumber.Length == 2)
                {
                    lEDNumber = "1" + lEDNumber;
                }

                string bindedName = lEDNumber + ulongHexColor;

                ulongSendNumber = Convert.ToUInt64(bindedName);
                //_serverUIManager.PrintConsole(ulongSendNumber.ToString());
                _driver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                writer.WriteULong(ulongSendNumber);
                _driver.EndSend(writer);
            }
        }

        //public void SendToClient(NetworkConnection connection, uint msg)
        public void SendToClient()
        {
            _isSendData = true;
            //DataStreamWriter writer;
            //_driver.BeginSend(connection, out writer);
            //writer.WriteUInt(msg);
            //_driver.EndSend(writer);
        }

        public void SendClient()
        {
            _driver.ScheduleUpdate().Complete();

            // Clean up connections.
            for (int i = 0; i < _connections.Length; i++)
            {
                //_serverUIManager.PrintConsole("1st part");

                if (!_connections[i].IsCreated)
                {
                    _connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Accept new connections.
            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                //_serverUIManager.PrintConsole("2nd part");

                _connections.Add(c);
                _serverUIManager.PrintConsole("Accepted a connection.");
                Debug.Log("Accepted a connection.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                //_serverUIManager.PrintConsole("3rd part");

                //DataStreamReader stream;
                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        ulong ulongSendNumber = stream.ReadUInt();
                        foreach (GameObject selectedLED in _serverUIManager.SelectedLEDButtons)
                        {
                            //ulong ulongSendNumber = stream.ReadULong();

                            Color selectedColor = selectedLED.transform.GetChild(0).GetComponent<Image>().color;
                            //string stringHexColor = ColorUtility.ToHtmlStringRGBA(selectedColor);
                            string stringHexColor = ColorUtility.ToHtmlStringRGB(selectedColor);

                            print("ID: " + selectedLED.name + " Color: " + stringHexColor);
                            //_serverUIManager.PrintConsole("ID: " + selectedLED.name + " Color: " + stringHexColor);

                            //uint number0 = Convert.ToUInt32(stringHexColor, 16);
                            ulong ulongHexColor = Convert.ToUInt64(stringHexColor, 16);

                            string lEDNumber = selectedLED.name;
                            if (lEDNumber.Length == 1)
                            {
                                lEDNumber = "10" + lEDNumber;
                            }
                            else if (lEDNumber.Length == 2)
                            {
                                lEDNumber = "1" + lEDNumber;
                            }

                            string bindedName = lEDNumber + ulongHexColor;

                            ulongSendNumber = Convert.ToUInt64(bindedName);
                            _serverUIManager.PrintConsole(ulongSendNumber.ToString());
                            _driver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                            writer.WriteULong(ulongSendNumber);
                            _driver.EndSend(writer);
                        }
                        _connections[i].Disconnect(_driver);
                        _driver.ScheduleFlushSend(default).Complete();
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        _serverUIManager.PrintConsole("Client disconnected from the server.");
                        Debug.Log("Client disconnected from the server.");
                        _connections[i] = default;
                        break;
                    }
                }
            }
        }

    }
}