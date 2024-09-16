using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System;
using UnityEngine.UI;

namespace GAG.UDPLEDControlSystem
{
    public class ClientManager : MonoBehaviour
    {
        [SerializeField] ClientUIManager _clientUIManager;

        NetworkDriver _driver;
        NetworkConnection _connection;


        void Start()
        {
            InitClient();
        }

        void Update()
        {
            UpdateClient();
        }

        void OnDestroy()
        {
            ShutdownClient();
        }


        // Initiate the networkDriver and connection
        void InitClient()
        {
            _clientUIManager.PrintConsole("I'm a Client");

            _driver = NetworkDriver.Create();
            _connection = default(NetworkConnection);

            NetworkEndpoint endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
            _connection = _driver.Connect(endpoint);
        }

        // Shutdown the client
        void ShutdownClient()
        {
            _driver.Dispose();
        }

        // Update the client
        void UpdateClient()
        {
            _driver.ScheduleUpdate().Complete();

            CheckAlive();
            UpdateNetworkEvents();
        }

        // Check the client connection
        void CheckAlive()
        {
            if(!_connection.IsCreated)
            {
                _clientUIManager.PrintConsole("Somthing went wrong, lost connection to the server.");
            }
        }

        // Handle network events
        void UpdateNetworkEvents()
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    _clientUIManager.PrintConsole("I'm now connected to the server.");

                    ulong value = 1;
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteULong(value);
                    _driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    UpdateData(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _clientUIManager.PrintConsole("Client got disconnected from server.");
                    _connection = default;

                    InitClient();
                }
            }
        }

        // Update the LED pattern 
        void UpdateData(DataStreamReader stream)
        {
            ulong value = stream.ReadULong();
            string stringValue = value.ToString();

            string ledID = stringValue.Substring(0, 3);

            string r = stringValue.Substring(3, 3);
            string g = stringValue.Substring(6, 3);
            string b = stringValue.Substring(9, 3);

            _clientUIManager.PrintConsole("ID: " + ledID + " R: " + r + " G: " + g + " B: " + b);

            ledID = GetLEDID(ledID);
            _clientUIManager.PrintConsole("ID: " + ledID);

            Color32 newColor = Color.black;

            newColor.r = (byte)GetRGBSubtrings(r);
            newColor.g = (byte)GetRGBSubtrings(g);
            newColor.b = (byte)GetRGBSubtrings(b);

            _clientUIManager.PrintConsole(" R: " + newColor.r + " G: " + newColor.g + " B: " + newColor.b);

            foreach (GameObject lEDLight in _clientUIManager.LEDLights)
            {
                if (lEDLight.name == ledID)
                {
                    lEDLight.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = newColor;
                }
            }
        }

        // Get the real id value
        string GetLEDID(string id)
        {
            if (id.Substring(0, 2) == "10")
            {
                id = id.Substring(2);
            }
            else if (id.Substring(0, 1) == "1")
            {
                id = id.Substring(1, 2);
            }
            return id;
        }

        // Get the real RGB values
        int GetRGBSubtrings(string value)
        {
            if (value.Substring(0, 2) == "99")
            {
                value = value.Substring(2);
            }
            else if (value.Substring(0, 1) == "9")
            {
                value = value.Substring(1, 2);
            }
            return Int32.Parse(value);
        }
    }
}