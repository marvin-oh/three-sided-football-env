using UnityEngine;

public class SoccerBallController : MonoBehaviour
{
    public GameObject area;
    [HideInInspector]
    public SoccerEnvController envController;
    public string purpleGoalTag; //will be used to check if collided with purple goal
    public string blueGoalTag; //will be used to check if collided with blue goal
    public string greenGoalTag; //will be used to check if collided with green goal

    public string blueAgentTag;
    public string purpleAgentTag;
    public string greenAgentTag;

    Team lastTouchedTeam;

    public EvalLogging evalLogging;

    void Start()
    {
        envController = area.GetComponent<SoccerEnvController>();
        evalLogging = area.GetComponent<EvalLogging>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(blueAgentTag)) //ball touched Blue Agent
        {
            lastTouchedTeam = Team.Blue;
        }
        if (col.gameObject.CompareTag(purpleAgentTag)) //ball touched Purple Agent
        {
            lastTouchedTeam = Team.Purple;
        }
        if (!string.IsNullOrEmpty(greenAgentTag) && col.gameObject.CompareTag(greenAgentTag)) //ball touched Grren Agent
        {
            lastTouchedTeam = Team.Green;
        }

        // Logging for Evaluation
        if (evalLogging != null)
        {
            EpisodeLog currentEpisodeLog = evalLogging.episodeLog;
            currentEpisodeLog.nBlueTouch += (lastTouchedTeam == Team.Blue) ? 1 : 0;
            currentEpisodeLog.nPurpleTouch += (lastTouchedTeam == Team.Purple) ? 1: 0;
            currentEpisodeLog.nGreenTouch += (lastTouchedTeam == Team.Green) ? 1 : 0;
        }

        if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
        {
            envController.GoalTouched(lastTouchedTeam, Team.Blue);
        }
        if (col.gameObject.CompareTag(purpleGoalTag)) //ball touched purple goal
        {
            envController.GoalTouched(lastTouchedTeam, Team.Purple);
        }
        if (!string.IsNullOrEmpty(greenGoalTag) && col.gameObject.CompareTag(greenGoalTag)) //ball touched green goal
        {
            envController.GoalTouched(lastTouchedTeam, Team.Green);
        }
    }
}
