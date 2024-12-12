using System;
using System.Net;
using UnityEngine;
using System.Globalization;

public class PaddleSyncClient : MonoBehaviour
{
    public enum PaddleSide { LEFT, RIGHT }
    public PaddleSide paddleSide; 
    private float lastPositionY;
    private const float positionThreshold = 0.01f; 

    private ClientManager clientManager;

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false;
            return;
        }

        clientManager = FindObjectOfType<ClientManager>();
        clientManager.OnPaddlePositionUpdated += UpdatePaddlePosition;
    }

    private void UpdatePaddlePosition(PaddleSide side, float newPositionY)
    {
        if (side != paddleSide) return;

        Vector3 newPosition = transform.position;
        newPosition.y = newPositionY;
        transform.position = newPosition;
    }

    void Update()
    {
        float currentPositionY = transform.position.y;
        if (Mathf.Abs(currentPositionY - lastPositionY) > positionThreshold)
        {
            clientManager.SendPaddleUpdate(paddleSide, currentPositionY);
            lastPositionY = currentPositionY;
        }
    }
}