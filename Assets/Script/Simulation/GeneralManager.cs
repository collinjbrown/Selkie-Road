using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;

namespace DeadReckoning.Sim
{
    public class GeneralManager : MonoBehaviour
    {
        public int year;
        public int day;
        public int totalPopulation;
        public int startingStructures = 10;

        public HistoryManager HistoryManager;
        public WorldGeneration.HexSphereGenerator hGen;

        public void PassDay()
        {
            // We'll add more here once we have more stuff.
            HistoryManager.PassDay(hGen);
            totalPopulation = HistoryManager.worldPopulation;
            day = HistoryManager.Day;
            year = day / 365;
        }

        public void PassDays(int days)
        {
            for (int i = 0; i < days; i++)
            {
                PassDay();
            }
        }

        public void TriggerTimelapse(int days)
        {
            StartCoroutine(Timelapse(days));
        }

        IEnumerator Timelapse(int days)
        {
            int target = day + days;
            // float rotateTime = 0.0001f;

            while (day < target)
            {
                PassDay();
                // hGen.transform.parent.Rotate(0, (360 / (rotateTime * 60 * 60)) * Time.deltaTime, 0, Space.Self);
                yield return new WaitForSeconds(0.01f);
            }

            // hGen.transform.rotation = Quaternion.identity;

            yield return null;
        }

        public void Setup(List<Tile> landTiles)
        {
            HistoryManager = new HistoryManager(startingStructures, landTiles);
            totalPopulation = HistoryManager.worldPopulation;
        }
    }
}
