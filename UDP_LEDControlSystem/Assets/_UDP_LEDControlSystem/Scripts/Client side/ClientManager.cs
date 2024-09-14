using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

namespace GAG.UDPLEDControlSystem
{
    public class ClientManager : MonoBehaviour
    {
        [SerializeField] ClientUIManager _clientUIManager;

        NetworkDriver _driver;
        NetworkConnection _connection;

        void Start()
        {
            _clientUIManager.PrintConsole("I'm Client");

            _driver = NetworkDriver.Create();

            var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
            _connection = _driver.Connect(endpoint);
        }

        void OnDestroy()
        {
            _driver.Dispose();
        }

        void Update()
        {
            _driver.ScheduleUpdate().Complete();

            if (!_connection.IsCreated)
            {
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    _clientUIManager.PrintConsole("We are now connected to the server.");
                    //Debug.Log("We are now connected to the server.");

                    uint value = 1;
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteUInt(value);
                    _driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint value = stream.ReadUInt();
                    _clientUIManager.PrintConsole($"Got the value {value} back from the server.");
                    //Debug.Log($"Got the value {value} back from the server.");

                    _connection.Disconnect(_driver);
                    _connection = default;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _clientUIManager.PrintConsole("Client got disconnected from server.");
                    //Debug.Log("Client got disconnected from server.");
                    _connection = default;
                }
            }
        }
    }
}