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
            _serverUIManager.PrintConsole("I'm the Server");

            _driver = NetworkDriver.Create();

            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);

            if (_driver.Bind(endpoint) != 0)
            {
                _serverUIManager.PrintConsole("Failed to bind to port 7777.");
                return;
            }
            _driver.Listen();

            _connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        }

        // Shutdown the server
        void ShutdownServer()
        {
            if (_driver.IsCreated)
            {
                _driver.Dispose();
                _connections.Dispose();
            }
        }

        // Update the server
        void UpdateServer()
        {
            if (_isSendData)
            {
                _driver.ScheduleUpdate().Complete();

                CleanupConnections();
                AcceptNewConnections();
                UpdateNetworkEvents();
            }
        }

        // Clean up connections.
        void CleanupConnections()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
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
            }
        }

        // Handle network events
        void UpdateNetworkEvents()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                NetworkEvent.Type cmd;

                while ((cmd = _driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        UpdateData(stream, i);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        _serverUIManager.PrintConsole("Client disconnected from the server.");
                        _connections[i] = default;
                        break;
                    }
                }
            }
        }

        // Update the LED pattern 
        void UpdateData(DataStreamReader stream, int i)
        {
            ulong ulongSendNumber = stream.ReadUInt();
            foreach (GameObject selectedLED in _serverUIManager.SelectedLEDButtons)
            {
                Color32 selectedColor = selectedLED.transform.GetChild(0).GetComponent<Image>().color;
                string r = selectedColor.r.ToString();
                string g = selectedColor.g.ToString();
                string b = selectedColor.b.ToString();

                _serverUIManager.PrintConsole("ID: " + selectedLED.name + " R: " + r + " G: " + g + " B: " + b);

                string lEDNumber = SetLEDID(selectedLED.name);

                r = SetRGBSubtrings(r);
                g = SetRGBSubtrings(g);
                b = SetRGBSubtrings(b);

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

        // Set id length to 3
        string SetLEDID(string id)
        {
            if (id.Length == 1)
            {
                id = "10" + id;
            }
            else if (id.Length == 2)
            {
                id = "1" + id;
            }
            return id;
        }

        // Set RGB value length to 3
        string SetRGBSubtrings(string value)
        {
            if (value.Length == 1)
            {
                value = "99" + value;
            }
            else if (value.Length == 2)
            {
                value = "9" + value;
            }
            return value;
        }

        // Allow to send new LED pattern to the client
        public void SendToClient()
        {
            _isSendData = true;
        }
    }
}