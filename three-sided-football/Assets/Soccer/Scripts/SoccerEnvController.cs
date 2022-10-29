using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }


    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;
    private SimpleMultiAgentGroup m_GreenAgentGroup;

    private int m_ResetTimer;

    bool m_ChangableAgentCount;

    EnvironmentParameters m_ResetParams;

    void Start()
    {

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        m_ChangableAgentCount = Academy.Instance.EnvironmentParameters.GetWithDefault("changable_agent_count", 0) > 0;
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            // Logging for Evaluation
            GetComponent<EvalLogging>().LogWithoutGoal(m_ResetTimer, m_BlueAgentGroup.GetRegisteredAgents().Count);

            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            m_GreenAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    void ResetAgentGroup(bool changable=false)
    {
        if (m_BlueAgentGroup == null)
        {
            m_BlueAgentGroup = new SimpleMultiAgentGroup();
        }
        if (m_PurpleAgentGroup == null)
        {
            m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        }
        if (m_GreenAgentGroup == null)
        {
            m_GreenAgentGroup = new SimpleMultiAgentGroup();
        }

        m_BlueAgentGroup.Dispose();
        m_PurpleAgentGroup.Dispose();
        m_GreenAgentGroup.Dispose();

        int agentCount = AgentsList.Count;
        int teamCount = GetTeamCount();
        int nAgentsPerGroup = changable ? Random.Range(2, agentCount / teamCount + 1) : agentCount / teamCount;

        //Debug.Log("teamCnAgentsPerGroupount is " + nAgentsPerGroup);

        foreach (var item in AgentsList)
        {
            Vector3 initPos = item.Agent.gameObject.GetComponent<AgentSoccer>().initialPos;
            float rotSign = item.Agent.gameObject.GetComponent<AgentSoccer>().rotSign;
            item.Agent.gameObject.transform.SetPositionAndRotation(initPos, Quaternion.Euler(0, rotSign, 0));
            item.Agent.gameObject.SetActive(true);

            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue && m_BlueAgentGroup.GetRegisteredAgents().Count < nAgentsPerGroup)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else if (item.Agent.team == Team.Purple && m_PurpleAgentGroup.GetRegisteredAgents().Count < nAgentsPerGroup)
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
            else if (item.Agent.team == Team.Green && m_GreenAgentGroup.GetRegisteredAgents().Count < nAgentsPerGroup)
            {
                m_GreenAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                item.Agent.gameObject.SetActive(false);
            }
        }
    }

    int GetTeamCount()
    {
        List<Team> lTeam = new List<Team>();
        foreach (var item in AgentsList)
        {
            var team = item.Agent.team;
            if (!lTeam.Contains(team))
            {
                lTeam.Add(team);
            }
        }
        return lTeam.Count;
    }

    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void GoalTouched(Team scoredTeam, Team concededTeam)
    {
        List<SimpleMultiAgentGroup> agentGroups = new List<SimpleMultiAgentGroup>() { m_BlueAgentGroup, m_PurpleAgentGroup, m_GreenAgentGroup };

        if (scoredTeam == concededTeam)
        {
            agentGroups[(int)concededTeam].AddGroupReward(-1);
        }
        else
        {
            agentGroups[(int)scoredTeam].AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            agentGroups[(int)concededTeam].AddGroupReward(-1);
        }

        // Logging for Evaluation
        GetComponent<EvalLogging>().LogWithGoal(m_ResetTimer, m_BlueAgentGroup.GetRegisteredAgents().Count, scoredTeam, concededTeam);

        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        m_GreenAgentGroup.EndGroupEpisode();
        ResetScene();
    }

    public void ResetScene()
    {
        ResetAgentGroup(changable: m_ChangableAgentCount);

        m_ResetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-2f, 2f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            if (item.Agent.position == AgentSoccer.Position.Goalie)
            {
                newStartPos = item.Agent.initialPos;
                newRot = Quaternion.Euler(0, item.Agent.rotSign, 0);
            }
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        ResetBall();
    }
}
