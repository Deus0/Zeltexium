using UnityEngine;
using System.Collections;

namespace Zeltex.Voxels
{
    /// <summary>
    /// used by blocks with the /Summoner command
    /// </summary>
    public class VoxelSummoner : MonoBehaviour
    {
        // every 30 seconds, if the summoned object is gone, summon a new one
        // or summon one 30+-(10) seconds after it dies
        float WaitTime = 30;
        float Variance = 10;
    
        public void OnMonstersDeath()
        {
            StartCoroutine(SummonNewBeing());
        }
        IEnumerator SummonNewBeing()
        {
            yield return new WaitForSeconds(WaitTime + Random.Range(-Variance, Variance));
            // summon new thingo here - use summoner class rather then something new

            // Add Event to character stats on death's event handler

        }
    }
}