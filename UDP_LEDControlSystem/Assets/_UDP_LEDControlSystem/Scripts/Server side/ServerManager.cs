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

        void Start()
        {
            StartServer();
        }

        void StartServer()
        {
            _serverUIManager.PrintConsole("I'm Server");

            _driver = NetworkDriver.Create();
            _connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
            if (_driver.Bind(endpoint) != 0)
            {
                _serverUIManager.PrintConsole("Failed to bind to port 7777.");
                Debug.LogError("Failed to bind to port 7777.");
                return;
            }
            _driver.Listen();
        }

        void OnDestroy()
        {
            if (_driver.IsCreated)
            {
                _driver.Dispose();
                _connections.Dispose();
            }
        }

        public void SendClient()
        {
            _driver.ScheduleUpdate().Complete();

            // Clean up connections.
            for (int i = 0; i < _connections.Length; i++)
            {
                _serverUIManager.PrintConsole("1st part" );

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
                _serverUIManager.PrintConsole("2nd part");

                _connections.Add(c);
                _serverUIManager.PrintConsole("Accepted a connection.");
                Debug.Log("Accepted a connection.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                _serverUIManager.PrintConsole("3rd part");

                DataStreamReader stream;
                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
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