using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Detection
{
    public class RiskScoring
    {
        public ThreatLevel ScoreToThreatLevel(int score)
        {
            if (score >= 90)
            {
                return ThreatLevel.DarkRed;
            }
            if (score >= 70)
            {
                return ThreatLevel.Red;
            }
            if (score >= 45)
            {
                return ThreatLevel.Orange;
            }
            if (score >= 20)
            {
                return ThreatLevel.Yellow;
            }
            if (score >= 5)
            {
                return ThreatLevel.Blue;
            }

            return ThreatLevel.Green;
        }

        public int ComputeScore(ThreatInfo? knownThreat, ThreatInfo? heuristic)
        {
            var score = 0;
            if (knownThreat != null)
            {
                score += 90;
            }

            if (heuristic != null)
            {
                score += 35;
            }

            return score;
        }
    }
}
