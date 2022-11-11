using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EpisodeLog
{
    public int episodeLength;

    public int nAgentsPerGroup;

    public int nBlueTouch;
    public int nPurpleTouch;
    public int nGreenTouch;

    public bool isBlueGoal;
    public bool isPurpleGoal;
    public bool isGreenGoal;

    public bool isBlueConcede;
    public bool isPurpleConcede;
    public bool isGreenConcede;

    public void Reset()
    {
        episodeLength = 0;

        nAgentsPerGroup = 0;

        nBlueTouch = 0;
        nPurpleTouch = 0;
        nGreenTouch = 0;

        isBlueGoal = false;
        isPurpleGoal = false;
        isGreenGoal = false;

        isBlueConcede = false;
        isPurpleConcede = false;
        isGreenConcede = false;
    }
}

public class EvalLogging : MonoBehaviour
{
    public EpisodeLog episodeLog;
    public bool doLogging = true;
    public string experimentName;

    // Start is called before the first frame update
    void Start()
    {
        episodeLog = new EpisodeLog();

        Time.timeScale = 15f;
    }

    public void LogWithGoal(int step, int _nAgentsPerGroup, Team scoredTeam, Team concededTeam)
    {
        episodeLog.episodeLength = step;
        episodeLog.nAgentsPerGroup = _nAgentsPerGroup;

        episodeLog.isBlueGoal = (scoredTeam == Team.Blue);
        episodeLog.isPurpleGoal = (scoredTeam == Team.Purple);
        episodeLog.isGreenGoal = (scoredTeam == Team.Green);

        episodeLog.isBlueConcede = (concededTeam == Team.Blue);
        episodeLog.isPurpleConcede = (concededTeam == Team.Purple);
        episodeLog.isGreenConcede = (concededTeam == Team.Green);

        if (doLogging)
        {
            SaveLog();
            episodeLog.Reset();
        }
    }

    public void LogWithoutGoal(int step, int _nAgentsPerGroup)
    {
        episodeLog.episodeLength = step;
        episodeLog.nAgentsPerGroup = _nAgentsPerGroup;

        if (doLogging)
        {
            SaveLog();
            episodeLog.Reset();
        }
    }

    private void SaveLog()
    {
        using (var writer = new StreamWriter("../experiment/log_" + experimentName + ".csv", append: true))
        {
            string line = "";
            line += episodeLog.episodeLength + ",";
            line += episodeLog.nAgentsPerGroup + ",";
            line += episodeLog.nBlueTouch + ",";
            line += episodeLog.nPurpleTouch + ",";
            line += episodeLog.nGreenTouch + ",";
            line += episodeLog.isBlueGoal + ",";
            line += episodeLog.isPurpleGoal + ",";
            line += episodeLog.isGreenGoal + ",";
            line += episodeLog.isBlueConcede + ",";
            line += episodeLog.isPurpleConcede + ",";
            line += episodeLog.isGreenConcede + ",";
            writer.WriteLine(line);
        }
    }
}
