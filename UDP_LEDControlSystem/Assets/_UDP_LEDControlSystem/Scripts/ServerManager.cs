using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

namespace GAG.UDPLEDControlSystem
{
    public class ServerManager : MonoBehaviour
    {
        NetworkDriver _driver;
        NativeList<NetworkConnection> _connections;

        void Start()
        {
            _driver = NetworkDriver.Create();
            _connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
            if (_driver.Bind(endpoint) != 0)
            {
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

        void Update()
        {
            _driver.ScheduleUpdate().Complete();

            // Clean up connections.
            for (int i = 0; i < _connections.Length; i++)
            {
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
                _connections.Add(c);
                Debug.Log("Accepted a connection.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                DataStreamReader stream;
                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        uint number = stream.ReadUInt();

                        Debug.Log($"Got {number} from a client, adding 2 to it.");
                        number += 2;

                        _driver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                        writer.WriteUInt(number);
                        _driver.EndSend(writer);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from the server.");
                        _connections[i] = default;
                        break;
                    }
                }
            }
        }
    }
}