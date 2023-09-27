using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Unity;
public class SimpleCharacterController : MonoBehaviour
{

    public float speed = 5.0f;
    public float rayDistance = 100.0f;

    private UdpClient udpClient;
    private string serverIP = "127.0.0.1"; // IP of the Python server
    private int port = 12345; // Port number
    public Transform dest;
    public int counter;
    void Start()
    {
        udpClient = new UdpClient();
        counter = 0; 
    }

    void Update()
    {
        SendEnvInfoToServer();
        ReceiveMovementFromServer();
    }

    [System.Serializable]
    public class MovementCommands
    {
        public int[] commands;
    }
    [System.Serializable]
    public class EnvInfo
    {
        public int stepCounter;
        public List<InfoData> data = new List<InfoData>();
    }

    [System.Serializable]
    public class InfoData
    {
        public string id;
        public float value;
    }
    void SendEnvInfoToServer()
    {
        EnvInfo envInfo = GetEnvInfo();
        string message = JsonUtility.ToJson(envInfo);
        byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
        udpClient.Send(bytesToSend, bytesToSend.Length, serverIP, port);
    }

    void ReceiveMovementFromServer()
    {
        if (udpClient.Available > 0)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
            byte[] data = udpClient.Receive(ref remoteEP);
            string message = Encoding.UTF8.GetString(data);
            MovementCommands moveCommands = JsonUtility.FromJson<MovementCommands>(message);
            MoveCharacter(moveCommands.commands);
        }
    }
    void MoveCharacter(int[] commands)
    {
        Debug.Log("Move X : " + commands[0] + " , Move Y : " + commands[1]);
        Vector3 movement = new Vector3(commands[0], 0.0f, commands[1]);
        transform.Translate(movement * speed * Time.deltaTime);
        //transform.Rotate(0, commands[1], 0);
        
    }


    public EnvInfo GetEnvInfo()
    {

        EnvInfo envInfo = new EnvInfo();

        for (int i = 0; i < 360; i += 30)
        {
            InfoData info = new InfoData();
            info.id = i.ToString();
            info.value = RaycastAtAngle(i);
            envInfo.data.Add(info);
        }

        envInfo.data.Add(new InfoData() { id = "DestX", value = dest.position.x });
        envInfo.data.Add(new InfoData() { id = "DestZ", value = dest.position.z });
        envInfo.data.Add(new InfoData() { id = "SelfX", value = transform.position.x });
        envInfo.data.Add(new InfoData() { id = "SelfZ", value = transform.position.z });
        envInfo.stepCounter = counter;
        counter++;
        return envInfo;

      
    }

    float RaycastAtAngle(float angle)
    {
        Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, rayDistance))
        {
            if(dir == transform.forward)
            {
                Debug.DrawRay(transform.position, dir * hit.distance, Color.yellow, 0.5f);
            }
                
            else
            {
                Debug.DrawRay(transform.position, dir * hit.distance, Color.red, 0.5f);
            }
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(transform.position, dir * rayDistance, Color.red, 0.5f);
            return rayDistance;
        }
    }
}