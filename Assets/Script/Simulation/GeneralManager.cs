using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.Map;
using DeadReckoning.WorldGeneration;

namespace DeadReckoning.Sim
{
    public class GeneralManager : MonoBehaviour
    {
        public int year;
        public int day;
        public int totalPopulation;
        public int startingCivilizations = 50;
        public int startingCountySize = 5;

        public HistoryManager HistoryManager;
        public WorldGeneration.HexSphereGenerator hGen;
        public GameObject hostPrefab;

        public void PassDay()
        {
            HistoryManager.PassDay(hGen);

            totalPopulation = HistoryManager.worldPopulation;

            day = HistoryManager.Day;
            year = day / 365;
        }

        #region Creating Objects
        public void GenerateHost(Civilization civ, int hostSize, Host.Purpose purpose, Hex start, Hex end)
        {
            GameObject g = Instantiate(hostPrefab, start.center.pos, Quaternion.identity, this.transform);
            g.transform.up = start.center.pos.normalized;

            g.GetComponent<Host>().Initiate(HistoryManager, civ, hostSize, purpose, start, end);
            HistoryManager.hosts.Add(g.GetComponent<Host>());
        }
        #endregion

        #region Passage of Time
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
            HistoryManager = new HistoryManager(startingCivilizations, startingCountySize, landTiles);
            totalPopulation = HistoryManager.worldPopulation;
        }
        #endregion

        #region Instance
        public static GeneralManager instance;
        public void Awake() { instance = this; }
        #endregion
    }
}
