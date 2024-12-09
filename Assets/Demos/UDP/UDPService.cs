using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class UDPService : MonoBehaviour
{
    UdpClient udp;
    IPEndPoint localEP;

    public delegate void UDPMessageReceive(string message, IPEndPoint sender);
    public event UDPMessageReceive OnMessageReceived;

    public bool Listen(int port)
    {
        if (udp != null)
        {
            Debug.LogWarning("[UDPService] Socket already initialized! Close it first.");
            return false;
        }

        try
        {
            localEP = new IPEndPoint(IPAddress.Any, port);
            udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.ExclusiveAddressUse = false;
            udp.Client.Bind(localEP);

            Debug.Log("[UDPService] Server listening on port: " + port);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UDPService] Error creating UDP listener on port {port}: {ex.Message}");
            CloseUDP();
            return false;
        }
    }

    public bool InitClient()
    {
        if (udp != null)
        {
            Debug.LogWarning("[UDPService] Socket already initialized! Close it first.");
            return false;
        }

        try
        {
            udp = new UdpClient();
            localEP = new IPEndPoint(IPAddress.Any, 0);
            udp.Client.Bind(localEP);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UDPService] Error creating UDP client: {ex.Message}");
            CloseUDP();
            return false;
        }
        return true;
    }

    private void CloseUDP()
    {
        if (udp != null)
        {
            udp.Close();
            udp = null;
        }
    }

    void Update()
    {
        ReceiveUDP();
    }

    private void ReceiveUDP()
    {
        if (udp == null) return;

        while (udp.Available > 0)
        {
            IPEndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udp.Receive(ref sourceEP);

            try
            {
                ParseString(data, sourceEP);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[UDPService] Error receiving UDP message: " + ex.Message);
            }
        }
    }

    private void ParseString(byte[] bytes, IPEndPoint sender)
    {
        if (OnMessageReceived == null)
        {
            Debug.LogWarning("[UDPService] No subscribers for OnMessageReceived event.");
            return;
        }

        string message = System.Text.Encoding.UTF8.GetString(bytes);
        OnMessageReceived.Invoke(message, sender);
    }

    public void SendUDPMessage(string message, IPEndPoint destination)
    {
        if (udp == null)
        {
            Debug.LogWarning("[UDPService] Trying to send a message on a socket that is not yet open.");
            return;
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

        try
        {
            udp.Send(bytes, bytes.Length, destination);
        }
        catch (SocketException e)
        {
            Debug.LogWarning($"[UDPService] Error sending UDP message: {e.Message}");
        }
    }
}
