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
            _clientUIManager.PrintConsole("I'm Client");

            _driver = NetworkDriver.Create();
            _connection = default(NetworkConnection);

            NetworkEndpoint endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
            _connection = _driver.Connect(endpoint);
        }

        void ShutdownClient()
        {
            _driver.Dispose();
        }

        void UpdateClient()
        {
            _driver.ScheduleUpdate().Complete();

            CheckAlive();
            UpdateMessages();
        }
        
        void CheckAlive()
        {
            if(!_connection.IsCreated)
            {
                _clientUIManager.PrintConsole("Somthing went wrong, lost connection to the server.");

            }
        }

        void UpdateMessages()
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    _clientUIManager.PrintConsole("We are now connected to the server.");
                    Debug.Log("We are now connected to the server.");

                    ulong value = 1;
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteULong(value);
                    _driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    ulong value = stream.ReadULong();
                    //print(value);

                    string tmp = value.ToString();
                    print(tmp);
                    _clientUIManager.PrintConsole(tmp);

                    string ledID = value.ToString().Substring(0, 3);
                    print(ledID);
                    _clientUIManager.PrintConsole(ledID);

                    string r = value.ToString().Substring(3,3);
                    string g = value.ToString().Substring(6,3);
                    string b = value.ToString().Substring(9,3);

                    print("ID: " + ledID + " R: " + r + " G: " + g + " B: " + b);


                    ///string colorToHex = value.ToString().Substring(4, tmp.Length - 5);
                    //print(colorToHex);
                    //_clientUIManager.PrintConsole(colorToHex);

                    ///string hexString = Int64.Parse(colorToHex).ToString("X");
                    ///hexString = '#' + hexString;
                    //string hexString = value.ToString("X");
                    //print(hexString);
                    //_clientUIManager.PrintConsole(hexString);

                    //if (ledID.Substring(0, 2) == "10")
                    //{
                    //    //_clientUIManager.PrintConsole(ledID);
                    //    ledID = ledID.Substring(2);

                    //   // _clientUIManager.PrintConsole(ledID);

                    //}
                    //else if (ledID.Substring(0, 1) == "1")
                    //{
                    //    //_clientUIManager.PrintConsole(ledID);

                    //    ledID = ledID.Substring(1, 2);

                    //    //_clientUIManager.PrintConsole(ledID);

                    //}

                    ledID = GetLEDID(ledID);
                    _clientUIManager.PrintConsole("ID: " + ledID);

                    Color32 newColor = Color.black;

                    newColor.r = (byte)GetRGBSubtrings(r);
                    newColor.g = (byte)GetRGBSubtrings(g);
                    newColor.b = (byte)GetRGBSubtrings(b);

                    print(" R: " + newColor.r + " G: " + newColor.g + " B: " + newColor.b);

                    //Color myColor;
                    //if (ColorUtility.TryParseHtmlString(hexString, out myColor))
                    //{
                    //    newColor = myColor;
                    //}
                    //else
                    //{
                    //    newColor = Color.red;
                    //}

                    foreach (GameObject lEDLight in _clientUIManager.LEDLights)
                    {

                        if (lEDLight.name == ledID)
                        {
                            //_clientUIManager.PrintConsole("lEDLight.name == ledID");

                            lEDLight.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = newColor;
                        }
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _clientUIManager.PrintConsole("Client got disconnected from server.");
                    _connection = default;

                    InitClient();
                }
            }
        }
        
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

        public void SendToServer(uint msg)
        {
            DataStreamWriter writer;
            _driver.BeginSend(_connection, out writer);
            writer.WriteUInt(msg);
            _driver.EndSend(writer);
        }

        //void UpdatetClient()
        //{
        //    _driver.ScheduleUpdate().Complete();

        //    if (!_connection.IsCreated)
        //    {
        //        return;
        //    }

        //    DataStreamReader stream;
        //    NetworkEvent.Type cmd;

        //    while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
        //    {
        //        if (cmd == NetworkEvent.Type.Connect)
        //        {
        //            _clientUIManager.PrintConsole("We are now connected to the server.");
        //            Debug.Log("We are now connected to the server.");

        //            uint value = 1;
        //            _driver.BeginSend(_connection, out var writer);
        //            writer.WriteUInt(value);
        //            _driver.EndSend(writer);
        //        }
        //        else if (cmd == NetworkEvent.Type.Data)
        //        {
        //            ulong value = stream.ReadULong();
        //            //print(value);

        //            string tmp = value.ToString();
        //            //print(tmp);

        //            string ledID = value.ToString().Substring(0, 3);
        //            print(ledID);
        //            //_clientUIManager.PrintConsole(ledID);

        //            string colorToHex = value.ToString().Substring(3, tmp.Length - 3);
        //            print(colorToHex);
        //            //_clientUIManager.PrintConsole(colorToHex);

        //            string hexString = Int32.Parse(colorToHex).ToString("X");
        //            hexString = '#' + hexString;
        //            //string hexString = value.ToString("X");
        //            print(hexString);
        //            //_clientUIManager.PrintConsole(hexString);

        //            if (ledID.Substring(0, 2) == "10")
        //            {
        //                //_clientUIManager.PrintConsole(ledID);
        //                ledID = ledID.Substring(2);

        //                _clientUIManager.PrintConsole(ledID);

        //            }
        //            else if (ledID.Substring(0, 1) == "1")
        //            {
        //                //_clientUIManager.PrintConsole(ledID);

        //                ledID = ledID.Substring(1, 2);

        //                _clientUIManager.PrintConsole(ledID);

        //            }

        //            Color newColor = Color.black;
        //            Color myColor;
        //            if (ColorUtility.TryParseHtmlString(hexString, out myColor)){
        //                newColor = myColor;
        //            }
        //            else
        //            {
        //                newColor = Color.red;
        //            }

        //            foreach (GameObject lEDLight in _clientUIManager.LEDLights)
        //            {

        //                if (lEDLight.name == ledID)
        //                {
        //                    //_clientUIManager.PrintConsole("lEDLight.name == ledID");

        //                    lEDLight.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = newColor;
        //                }
        //            }

        //            Debug.Log($"Got the value {value} back from the server.");

        //             _connection.Disconnect(_driver);
        //             _connection = default;
        //             //StartClient();

        //        }
        //        else if (cmd == NetworkEvent.Type.Disconnect)
        //        {
        //            _clientUIManager.PrintConsole("Client got disconnected from server.");
        //            Debug.Log("Client got disconnected from server.");
        //            _connection = default;

        //            //StartClient();
        //        }
        //    }
        //}

    }
}