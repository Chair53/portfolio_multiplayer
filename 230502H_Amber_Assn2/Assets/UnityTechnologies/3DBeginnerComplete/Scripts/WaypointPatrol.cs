using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public List<Transform> waypoints;
    int m_CurrentWaypointIndex;

    void Update ()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if(navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Count;
            navMeshAgent.SetDestination (waypoints[m_CurrentWaypointIndex].position);
        }
    }

    public void StartAI()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        navMeshAgent.SetDestination(waypoints[0].position);
    }
}
