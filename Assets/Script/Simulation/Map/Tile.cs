using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeadReckoning.WorldGeneration;
using DeadReckoning.Constructs;
using System.Linq;
using DeadReckoning.Sim;

namespace DeadReckoning.Map
{
    public class TileMap
    {
        public List<Tile> tiles;
        public List<Tile> land;
        public List<Tile> ocean;

        public List<TectonicPlate> plates;

        public WorldbuildingSettings settings;

        public void Generate(HexSphereGenerator hGen)
        {
            // We're going to move wind and current simulation to another object that tiles will reference.

            foreach (Tile t in tiles)
            {
                t.DetermineWinds(hGen);
                t.DetermineSubmerged(hGen);
            }

            Debug.Log($"Land Tiles: {land.Count} ({ (land.Count / (float)(ocean.Count + land.Count)) * 100 }%) // Sea Tiles: {ocean.Count} ({ (ocean.Count / (float)(ocean.Count + land.Count)) * 100 }%).");

            foreach (Tile t in tiles)
            {
                t.DetermineCurrents(hGen);

                if (!t.submerged && !t.polarCap)
                {
                    t.DetermineBiomes(hGen);
                    t.DetermineResources();
                }
            }
            foreach (Tile t in tiles)
            {
                t.ColorCurrents();
            }
        }

        public void DetermineContinents(HexSphereGenerator hGen)
        {
            // This needs to be called before we add noise...
            // so that we can add mountains when we do that.

            List<Tile> uncheckedTiles = tiles;

            for (int i = 0; i < settings.tectonicPlates; i++)
            {
                Tile randomTile = uncheckedTiles[Random.Range(0, uncheckedTiles.Count)];
                uncheckedTiles.Remove(randomTile);

                TectonicPlate plate = new TectonicPlate(randomTile);
                plate.color = Random.ColorHSV();

                plates.Add(plate);
            }

            Debug.Log($"{plates.Count} tectonic plates generated.");

            foreach (Tile t in tiles)
            {
                TectonicPlate closestPlate = null;
                float closestDistance = Mathf.Infinity;

                foreach (TectonicPlate p in plates)
                {
                    Vector3 p1 = p.tiles[0].hex.center.pos;
                    Vector3 p2 = t.hex.center.pos;

                    if (Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2) + Mathf.Pow(p1.z - p2.z, 2)) < closestDistance) // idk
                    {
                        closestDistance = Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2) + Mathf.Pow(p1.z - p2.z, 2));
                        closestPlate = p;
                    }
                }

                closestPlate.tiles.Add(t);
                t.plate = closestPlate;
            }

            foreach (Tile t in tiles)
            {
                t.map = this;

                foreach (Hex h in t.hex.neighbors)
                {
                    if (h.tile.plate != t.plate)
                    {
                        t.fault = true;

                        foreach (Hex h2 in t.hex.neighbors) // This is kinda gross (x3).
                        {
                            h2.tile.faultAdjacent = true;
                        }
                    }
                }

                settings = hGen.worldSettings;
                t.DeterminePolarCaps(hGen);
            }
        }

        public TileMap (List<Tile> t, WorldbuildingSettings sett)
        {
            tiles = t;
            land = new List<Tile>();
            ocean = new List<Tile>();
            plates = new List<TectonicPlate>();
            settings = sett;
        }
    }

    public class Tile
    {
        // 1 Tile = 10 KM in diameter
        // 1 Tile = ~64.95 KM^2 = ~40 mils^2
        public const int tileArea = 40;

        // Which means max population density should be...
        // somewhere around ~5000.
        // Max Pop = area * fertility.

        public bool submerged; // Underwater, duh.
        public bool shore; // Edge of landmass.
        public bool fault; // Edge of continent.
        public bool faultAdjacent; // What is says on the tin.
        public bool polarCap;

        public int Fertility { get { return GetFertility(); } }
        public float PopulationLimit { get { return GetPopulationLimit(); } }

        public TileMap map;
        public TectonicPlate plate;
        public County county;

        public Dictionary<Resource.Type, Resource> resources;

        public List<Structure> structures;

        public Gradient precipitation;
        public Gradient temperature;
        public Biome biome;

        public float windMagnitude;
        public FlowDirection windDirection;
        public WindType windType;

        public FlowDirection currentDirection;
        public CurrentHeat currentHeat;

        public bool grass;

        public Hex hex;

        public Tile(Hex h)
        {
            resources = new Dictionary<Resource.Type, Resource>();
            structures = new List<Structure>();
            hex = h;
        }

        public enum Precipitation { veryHigh, high, mid, low, veryLow }
        public enum Gradient { veryHigh, high, mid, low, veryLow }
        public enum Biome { none, tropicalMonsoon, hotSteppe, savanna, tropicalRainforest, hotDesert, coldDesert, mountain, highlands, mediterranean, humidSubtropic, oceanic, humidContinental, subarctic, prairie, icecap }
        // Respective Food :: 0,         20,          10,       10,           20,              0,         5,        N/A,       20,          40,             30,         25,          20,             5,       30,      0
        // Other Resources :: 0,
        public enum WindType { trade, westerly, easterly }
        public enum CurrentHeat { none, lukewarm, mixed, warm, cold }
        public enum FlowDirection { none, east, west, north, south }

        #region Resources
        public float GetPopulationLimit()
        {
            // Maximum population should essentially be somewhere between abysmally low and 5000.
            // 5,000 comes from 125 people per square mile times the tile area (~40 square miles).
            // This (kinda absurdly) high density should only ever occur in really fertile places...
            // like Mediterranean climates. Average density should fall somewhere closer to the 30 ~ 60 range.
            // We'll also weight things like heat and precipitation; both through absolute values.

            float maxPop = (Fertility * 2.5f * tileArea) - Mathf.Abs((int)precipitation * 100) - Mathf.Abs((int)temperature * 100);

            if (maxPop < 0)
            {
                maxPop = 0;
            }

            return maxPop;
        }
        public int GetFertility()
        {
            int total = 0;

            foreach (KeyValuePair<Resource.Type, Resource> r in resources)
            {
                if (r.Value.edible)
                {
                    total += r.Value.yield;
                }
            }

            return total;
        }
        public int GetLivestockCount()
        {
            return (from KeyValuePair<Resource.Type, Resource> r in resources where r.Value.animal select r).ToList().Count;
        }
        public void DetermineResources()
        {
            #region Rainwater
            if (precipitation == Gradient.veryLow)
            {
                AddResources(Resource.Type.water, 0, false, false, true, false);
            }
            else if (precipitation == Gradient.low)
            {
                AddResources(Resource.Type.water, 10, false, false, true, false);
            }
            else if (precipitation == Gradient.mid)
            {
                AddResources(Resource.Type.water, 20, false, false, true, false);
            }
            else if (precipitation == Gradient.high)
            {
                AddResources(Resource.Type.water, 30, false, false, true, false);
            }
            else if (precipitation == Gradient.veryHigh)
            {
                AddResources(Resource.Type.water, 40, false, false, true, false);
            }
            #endregion

            int dumbLuck = Random.Range(0, 11);

            #region Coasts & Faults
            if (shore)
            {
                AddResources(Resource.Type.fish, 20, true, true, false, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.fish, 10, true, true, false, false);
                }
            }

            if (faultAdjacent)
            {
                AddResources(Resource.Type.stone, 20, false, false, false, true);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.stone, 10, false, false, false, true);
                }
            }
            #endregion

            #region Biomes
            if (biome == Biome.tropicalMonsoon)
            {
                AddResources(Resource.Type.rice, 20, false, true, false, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.rice, 10, false, true, false, false);
                }
            }
            else if (biome == Biome.hotSteppe)
            {
                AddResources(Resource.Type.barley, 5, false, true, false, false);
                AddResources(Resource.Type.goat, 5, true, true, false, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.goat, 10, true, true, false, false);
                }
            }
            else if (biome == Biome.savanna)
            {
                AddResources(Resource.Type.maize, 5, false, true, false, false);
                AddResources(Resource.Type.millet, 5, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.goat, 10, true, true, false, false);
                }
            }
            else if (biome == Biome.tropicalRainforest)
            {
                AddResources(Resource.Type.sugarCane, 5, false, true, false, false);
                AddResources(Resource.Type.coffee, 5, false, true, false, false);
                AddResources(Resource.Type.banana, 5, false, true, false, false);
                AddResources(Resource.Type.chocolate, 5, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.pork, 10, true, true, false, false);
                }
            }
            else if (biome == Biome.coldDesert)
            {
                AddResources(Resource.Type.goat, 5, true, true, false, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.goat, 5, true, true, false, false);
                }
            }
            else if (biome == Biome.highlands)
            {
                AddResources(Resource.Type.sheep, 5, true, true, false, false);
                AddResources(Resource.Type.barley, 10, false, true, false, false);
                AddResources(Resource.Type.wheat, 5, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.goat, 10, true, true, false, false);
                }
            }
            else if (biome == Biome.mediterranean)
            {
                AddResources(Resource.Type.wheat, 20, false, true, false, false);
                AddResources(Resource.Type.olives, 10, false, true, false, false);
                AddResources(Resource.Type.grapes, 10, false, true, false, false);
                AddResources(Resource.Type.beef, 10, true, true, false, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.grapes, 10, false, true, false, false);
                    AddResources(Resource.Type.olives, 10, false, true, false, false);
                }
            }
            else if (biome == Biome.humidSubtropic)
            {
                AddResources(Resource.Type.rice, 10, false, true, false, false);
                AddResources(Resource.Type.cotton, 10, false, true, false, false);
                AddResources(Resource.Type.maize, 10, false, true, false, false);
                AddResources(Resource.Type.tea, 10, false, false, true, false);

                if (dumbLuck == 10)
                {
                    AddResources(Resource.Type.potatoes, 10, false, true, false, false);
                    AddResources(Resource.Type.tea, 10, false, false, true, false);
                }
            }
            else if (biome == Biome.oceanic)
            {
                AddResources(Resource.Type.potatoes, 10, false, true, false, false);
                AddResources(Resource.Type.sheep, 10, true, true, false, false);
                AddResources(Resource.Type.beef, 10, true, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.potatoes, 10, false, true, false, false);
                }
            }
            else if (biome == Biome.humidContinental)
            {
                AddResources(Resource.Type.wheat, 10, false, true, false, false);
                AddResources(Resource.Type.barley, 10, false, true, false, false);
                AddResources(Resource.Type.maize, 5, false, true, false, false);
                AddResources(Resource.Type.potatoes, 5, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.poultry, 5, true, true, false, false);
                    AddResources(Resource.Type.beef, 5, true, true, false, false);
                }
            }
            else if (biome == Biome.subarctic)
            {
                AddResources(Resource.Type.wheat, 5, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.goat, 5, true, true, false, false);
                }
            }
            else if (biome == Biome.prairie)
            {
                AddResources(Resource.Type.beef, 10, true, true, false, false);
                AddResources(Resource.Type.wheat, 20, false, true, false, false);

                if (dumbLuck >= 9)
                {
                    AddResources(Resource.Type.poultry, 10, true, true, false, false);
                }
            }
            #endregion

            #region Random
            dumbLuck = Random.Range(0, 101);

            if (dumbLuck >= 98)
            {
                AddResources(Resource.Type.tin, 10, false, false, false, true);
            }
            else if (dumbLuck >= 94)
            {
                AddResources(Resource.Type.copper, 20, false, false, false, true);
            }
            else if (dumbLuck >= 90)
            {
                AddResources(Resource.Type.iron, 20, false, false, false, true);
            }
            #endregion
        }
        public void AddResources(Resource.Type type, int yield, bool animal, bool edible, bool drinkable, bool buildable)
        {
            if (!resources.ContainsKey(type))
            {
                Resource r = new Resource(type, yield, animal, edible, drinkable, buildable);
                resources.Add(type, r);
            }
            else
            {
                Resource r = resources[type];
                r.yield += yield;
            }
        }
        #endregion

        #region Worldbuilding

        public void DeterminePolarCaps(HexSphereGenerator hGen)
        {
            Vector3 pos = hex.center.pos;
            Vector3 worldCenter = hGen.transform.position;
            float worldRadius = hGen.worldRadius;

            if (Mathf.Abs(pos.y - worldCenter.y) > worldRadius * (map.settings.polarCapCutoff))
            {
                polarCap = true;
                SetBiome(Biome.icecap, true, Gradient.veryLow, true, Gradient.veryLow, Color.cyan, false);
            }
            else if (Mathf.Abs(pos.y - worldCenter.y) > worldRadius * (map.settings.polarCapCutoff - Random.Range(0, map.settings.polarVariance)))
            {
                polarCap = true;
                SetBiome(Biome.icecap, true, Gradient.veryLow, true, Gradient.veryLow, Color.cyan, false);
            }
        }

        public bool SearchForMountains(FlowDirection neighborDirection)
        {
            int searchDistance = Mathf.RoundToInt(windMagnitude * 3);

            int[] neighborInds = new int[] { 1, 2, 3 };

            if (neighborDirection == FlowDirection.east)
            {
                neighborInds = new int[] { 0, 5, 4 };

                if (hex.pent)
                {
                    neighborInds = new int[] { 0, 4 };
                }
            }
            else if (neighborDirection == FlowDirection.north)
            {
                neighborInds = new int[] { 1, 0 };
            }
            else if (neighborDirection == FlowDirection.south)
            {
                neighborInds = new int[] { 3, 4 };
            }

            List<Tile> searchTiles = new List<Tile>();

            for (int n = 0; n < neighborInds.Length; n++)
            {
                Tile t = hex.neighbors[neighborInds[n]].tile;
                searchTiles.Add(t);
            }

            for (int i = 0; i < searchDistance; i++)
            {
                List<Tile> tempSearches = new List<Tile>();

                foreach (Tile t in searchTiles)
                {
                    for (int n = 0; n < neighborInds.Length; n++)
                    {
                        if (t.hex.pent && neighborInds[n] == 5)
                        {
                            continue;
                        }

                        Tile neighbor = t.hex.neighbors[neighborInds[n]].tile;

                        if (neighbor.faultAdjacent || neighbor.fault)
                        {
                            return true;
                        }

                        tempSearches.Add(neighbor);
                    }
                }

                searchTiles = tempSearches;
            }

            return false;
        }

        public void DetermineWinds(HexSphereGenerator hGen)
        {
            // We might consider tapering winds into the other cells...
            // so that there isn't such a hard cutoff between them.

            float hadleyCutoff = map.settings.hadleyCellCutoff;
            float ferrelCutoff = map.settings.ferrelCellCutoff;

            float worldRadius = hGen.worldRadius;
            Vector3 worldCenter = hGen.transform.position;

            Vector3 pos = this.hex.center.pos;
            // Vector3[] relativeAxes = HexChunk.FindRelativeAxes(this.hex.center);


            if (Mathf.Abs(pos.y - worldCenter.y) <= worldRadius * hadleyCutoff)
            {
                // Trade Winds
                this.windMagnitude = Mathf.Abs(1 - ((Mathf.Abs(pos.y - worldCenter.y) / worldRadius) / hadleyCutoff));

                this.hex.windColor = Color.Lerp(Color.white, Color.blue, windMagnitude);
                this.windDirection = FlowDirection.west;
                windType = WindType.trade;
            }
            else if (Mathf.Abs(pos.y - worldCenter.y) <= worldRadius * ferrelCutoff)
            {
                // Westerlies
                this.windMagnitude = Mathf.Abs(1 - (((Mathf.Abs(pos.y - worldCenter.y) / worldRadius) - hadleyCutoff) / hadleyCutoff));

                this.hex.windColor = Color.Lerp(Color.white, Color.red, windMagnitude);
                this.windDirection = FlowDirection.east;
                windType = WindType.westerly;
            }
            else
            {
                // Polar Easterlies
                this.windMagnitude = Mathf.Abs(1 - (((Mathf.Abs(pos.y - worldCenter.y) / worldRadius) - ferrelCutoff) / hadleyCutoff));

                this.hex.windColor = Color.Lerp(Color.white, Color.cyan, windMagnitude);
                this.windDirection = FlowDirection.west;
                windType = WindType.easterly;
            }
        }

        public void DetermineSubmerged(HexSphereGenerator hGen)
        {
            if (hex.center.pos.magnitude < hGen.oceanRadius)
            {
                submerged = true;
                map.ocean.Add(this);
            }
            else
            {
                map.land.Add(this);
            }
        }

        public void DetermineCurrents(HexSphereGenerator hGen)
        {
            float windCurrentCutoff = map.settings.windCurrentCutoff;
            float equatorY = hGen.transform.position.y;

            if (!submerged && !hex.pent)
            {
                if (windType == WindType.trade || windType == WindType.easterly)
                {
                    // Warm on right, cold on left.
                    // This is overly simplistic, but we'll flesh this out later.

                    if (hex.center.pos.y >= equatorY)
                    {
                        SetNeighborCurrents(hex, FlowDirection.west, CurrentHeat.cold, FlowDirection.south);
                        SetNeighborCurrents(hex, FlowDirection.east, CurrentHeat.warm, FlowDirection.north);
                    }
                    else
                    {
                        SetNeighborCurrents(hex, FlowDirection.west, CurrentHeat.cold, FlowDirection.north);
                        SetNeighborCurrents(hex, FlowDirection.east, CurrentHeat.warm, FlowDirection.south);
                    }
                }
                else
                {
                    if (hex.center.pos.y >= equatorY)
                    {
                        SetNeighborCurrents(hex, FlowDirection.west, CurrentHeat.warm, FlowDirection.north);
                        SetNeighborCurrents(hex, FlowDirection.east, CurrentHeat.cold, FlowDirection.south);
                    }
                    else
                    {
                        SetNeighborCurrents(hex, FlowDirection.west, CurrentHeat.warm, FlowDirection.south);
                        SetNeighborCurrents(hex, FlowDirection.east, CurrentHeat.cold, FlowDirection.north);
                    }
                }
            }
            else if (!hex.pent)
            {
                if (windMagnitude > windCurrentCutoff)
                {
                    currentHeat = CurrentHeat.lukewarm;
                    currentDirection = windDirection;
                }
            }
        }

        public void ColorCurrents()
        {
            if (currentHeat == CurrentHeat.warm && submerged)
            {
                hex.currentColor = Color.red;
            }
            else if (currentHeat == CurrentHeat.cold && submerged)
            {
                hex.currentColor = Color.cyan;
            }
            else if (currentHeat == CurrentHeat.mixed && submerged)
            {
                hex.currentColor = Color.magenta;
            }
            else if (currentHeat == CurrentHeat.lukewarm && submerged)
            {
                hex.currentColor = Color.green;
            }
            else
            {
                hex.currentColor = Color.white;
            }
        }

        void SetNeighborCurrents(Hex h, FlowDirection neighborDirection, CurrentHeat heat, FlowDirection direction)
        {
            // Come up with a way to better map the currents on the coast.

            int[] neighborInds = new int[] { 1, 2, 3 };

            if (neighborDirection == FlowDirection.east)
            {
                neighborInds = new int[] { 0, 5, 4 };
            }
            else if (neighborDirection == FlowDirection.north)
            {
                neighborInds = new int[] { 2, 1, 0, 5 };
            }
            else if (neighborDirection == FlowDirection.south)
            {
                neighborInds = new int[] { 2, 3, 4, 5 };
            }

            for (int i = 0; i < neighborInds.Length; i++)
            {
                if (h.neighbors[neighborInds[i]].tile.submerged)
                {
                    if (!h.tile.submerged)
                    {
                        h.tile.shore = true; // Used to determine precipitation.
                        currentHeat = heat; // Used to determine biomes.
                        currentDirection = neighborDirection; // Used to find onshore / offshore winds.
                    }

                    if (h.neighbors[neighborInds[i]].tile.currentHeat == CurrentHeat.none)
                    {
                        h.neighbors[neighborInds[i]].tile.currentHeat = heat;
                    }
                    else if (h.neighbors[neighborInds[i]].tile.currentHeat == CurrentHeat.warm && heat == CurrentHeat.cold
                          || h.neighbors[neighborInds[i]].tile.currentHeat == CurrentHeat.cold && heat == CurrentHeat.warm)
                    {
                        h.neighbors[neighborInds[i]].tile.currentHeat = CurrentHeat.mixed;
                    }

                    h.neighbors[neighborInds[i]].tile.currentDirection = direction;
                }
            }
        }

        public void SetBiome(Biome bio, bool setTemp, Gradient temp, bool setPrecip, Gradient precip, Color c, bool grass)
        {
            biome = bio;

            if (setTemp)
            {
                temperature = temp;
            }

            if (setPrecip)
            {
                precipitation = precip;
            }

            this.grass = grass;
            hex.biomeColor = Color.Lerp(c, Color.white, Random.Range(0f, 0.5f));
            hex.uv = new Vector2((int)bio, 0);
        }

        public void DetermineBiomes(HexSphereGenerator hGen)
        {
            Vector3 pos = hex.center.pos;
            Vector3 worldCenter = hGen.transform.position;
            float worldRadius = hGen.worldRadius;

            hex.biomeColor = Color.white;

            precipitation = EstimatePrecipitation();
            temperature = EstimateTemperature(hGen);

            float r = Random.Range(-hGen.worldSettings.biomeVariance, hGen.worldSettings.biomeVariance);

            // Deserts (in rain shadows)
            if (SearchForMountains(windDirection) && !fault && !faultAdjacent) // Deserts (in mountain rainshadows).
            {
                if (Mathf.Abs(pos.y - worldCenter.y) <= worldRadius * map.settings.coldHotDesertSplit)
                {
                    SetBiome(Biome.hotDesert, true, Gradient.veryHigh, true, Gradient.veryLow, Color.red, false);
                }
                else
                {
                    SetBiome(Biome.coldDesert, true, Gradient.veryLow, true, Gradient.veryLow, Color.blue, false);
                }
            }
            else
            {
                FlowDirection oppositeWind;

                if (windDirection == FlowDirection.east)
                {
                    oppositeWind = FlowDirection.west;
                }
                else if (windDirection == FlowDirection.west)
                {
                    oppositeWind = FlowDirection.east;
                }
                else if (windDirection == FlowDirection.north)
                {
                    oppositeWind = FlowDirection.south;
                }
                else
                {
                    oppositeWind = FlowDirection.north;
                }

                if (SearchForMountains(oppositeWind) && biome != Biome.hotDesert)
                {
                    precipitation = Gradient.veryHigh;
                }
            }

            // Rainforests (near equator)
            if (Mathf.Abs(pos.y - worldCenter.y) + r <= worldRadius * map.settings.rainForestCutoff)
            {
                //if (temperature == Gradient.high && precipitation == Gradient.high
                //    || temperature == Gradient.high && precipitation == Gradient.veryHigh
                //    || temperature == Gradient.veryHigh && precipitation == Gradient.high
                //    || temperature == Gradient.veryHigh && precipitation == Gradient.veryHigh)
                //{
                SetBiome(Biome.tropicalRainforest, true, Gradient.high, true, Gradient.veryHigh, Color.blue, false);
                // }
            }

            // Savannahs (near equator, further out)
            if (Mathf.Abs(pos.y - worldCenter.y) + r <= worldRadius * map.settings.savannahCutoff)
            {
                if (temperature == Gradient.high && precipitation == Gradient.low
                    || temperature == Gradient.high && precipitation == Gradient.veryLow
                    || temperature == Gradient.veryHigh && precipitation == Gradient.low
                    || temperature == Gradient.veryHigh && precipitation == Gradient.veryLow)
                {
                    SetBiome(Biome.savanna, false, Gradient.mid, false, Gradient.mid, Color.yellow, true);
                }
            }

            // Hot steppe (near equator, around savannahs and deserts).
            if (Mathf.Abs(pos.y - worldCenter.y) + r <= worldRadius * map.settings.hotSteppeCutoff && biome == Biome.none)
            {
                SetBiome(Biome.hotSteppe, true, Gradient.high, true, Gradient.low, Color.Lerp(Color.red, Color.yellow, 0.25f), true);
            }

            // Monsoon (where onshore winds and warm currents meet (kinda rare).
            if (shore && windDirection != currentDirection && currentHeat == CurrentHeat.warm
                && Mathf.Abs(pos.y - worldCenter.y) + r <= worldRadius * map.settings.monsoonCutoff)
            {
                SetBiome(Biome.tropicalMonsoon, true, Gradient.high, true, Gradient.high, Color.magenta, false);
            }

            // Mediterranean (cold currents)
            if (Mathf.Abs(pos.y - worldCenter.y) + r > worldRadius * map.settings.mediterraneanCutoff
                && currentHeat == CurrentHeat.cold)
            {
                SetBiome(Biome.mediterranean, true, Gradient.mid, true, Gradient.mid, Color.yellow, true);
            }

            // Oceanic (warm currents)
            if (Mathf.Abs(pos.y - worldCenter.y) + r > worldRadius * map.settings.mediterraneanCutoff
                && currentHeat == CurrentHeat.warm)
            {
                SetBiome(Biome.oceanic, true, Gradient.mid, true, Gradient.veryHigh, Color.green, true);
            }

            // Humid subtropics: wet, dense forests, usually interior.
            if (Mathf.Abs(pos.y - worldCenter.y) + r > worldRadius * map.settings.humidSubtropicCutoff)
            {
                if (temperature == Gradient.high && precipitation == Gradient.high
                    || temperature == Gradient.high && precipitation == Gradient.veryHigh
                    || temperature == Gradient.veryHigh && precipitation == Gradient.high
                    || temperature == Gradient.veryHigh && precipitation == Gradient.veryHigh)
                {
                    SetBiome(Biome.humidSubtropic, true, Gradient.mid, true, Gradient.veryHigh, Color.Lerp(Color.green, Color.blue, 0.25f), false);
                }
            }

            // Forests and prairies.
            if (Mathf.Abs(pos.y - worldCenter.y) + r > worldRadius * map.settings.humidContinentalCutoff
                && biome == Biome.none)
            {
                if (precipitation == Gradient.low || precipitation == Gradient.veryLow)
                {
                    SetBiome(Biome.prairie, true, Gradient.mid, false, Gradient.mid, Color.cyan, true);
                }
                else
                {
                    SetBiome(Biome.humidContinental, true, Gradient.mid, false, Gradient.mid, Color.cyan, false);
                }
            }

            // Subarctic (think Siberia)
            if (Mathf.Abs(pos.y - worldCenter.y) + r > worldRadius * map.settings.subarcticCutoff)
            {
                SetBiome(Biome.subarctic, true, Gradient.veryLow, true, Gradient.low, Color.Lerp(Color.cyan, Color.black, 0.7f), false);
            }

            // Mountain & Highlands
            if (fault && !shore)
            {
                SetBiome(Biome.mountain, true, Gradient.veryLow, false, Gradient.mid, Color.white, false);
            }
            else if (faultAdjacent || fault && shore) // Highlands
            {
                SetBiome(Biome.highlands, true, Gradient.low, false, Gradient.mid, Color.Lerp(Color.green, Color.white, 0.75f), true);
            }

            if (biome == Biome.none)
            {
                if (shore)
                {
                    SetBiome(Biome.humidSubtropic, true, Gradient.mid, true, Gradient.veryHigh, Color.Lerp(Color.green, Color.blue, 0.25f), false);
                }
                else
                {
                    SetBiome(Biome.coldDesert, true, Gradient.veryLow, true, Gradient.veryLow, Color.blue, false);
                }
            }

            if (submerged || biome == Biome.mountain)
            {
                hex.isWalkable = false;
            }
        }
        public Gradient EstimateTemperature(HexSphereGenerator hGen)
        {
            float posY = hex.center.pos.y;
            float equatorY = hGen.transform.position.y;
            float worldRadius = hGen.worldRadius;

            int temperatureEstimate;

            float midPoint;

            if (posY >= equatorY)
            {
                midPoint = worldRadius * 0.5f;

                temperatureEstimate = Mathf.RoundToInt(1 - ((posY - equatorY) / midPoint));
            }
            else
            {
                midPoint = -worldRadius * 0.5f;

                temperatureEstimate = Mathf.RoundToInt(1 - ((posY - equatorY) / midPoint));
            }

            if (!shore)
            {
                temperatureEstimate++;
            }
            else
            {
                temperatureEstimate--;
            }

            temperatureEstimate += Random.Range(-1, 2);

            if (temperatureEstimate >= map.settings.veryHighTemperatureCutoff)
            {
                hex.temperatureColor = Color.red;
                return Gradient.veryHigh;
            }
            else if (temperatureEstimate >= map.settings.highTemperatureCutoff)
            {
                hex.temperatureColor = Color.yellow;
                return Gradient.high;
            }
            else if (temperatureEstimate <= map.settings.lowTemperatureCutoff)
            {
                hex.temperatureColor = Color.cyan;
                return Gradient.low;
            }
            else if (temperatureEstimate <= map.settings.veryLowTemperatureCutoff)
            {
                hex.temperatureColor = Color.blue;
                return Gradient.veryLow;
            }
            else
            {
                hex.temperatureColor = Color.green;
                return Gradient.mid;
            }
        }
        public Gradient EstimatePrecipitation()
        {
            float windPressureCutoff = 0.75f;
            int precipitationEstimate = 0;

            if (shore && currentHeat == CurrentHeat.warm)
            {
                precipitationEstimate++;
            }
            else if (shore && currentHeat == CurrentHeat.cold)
            {
                precipitationEstimate--;
            }

            if (shore && windDirection != currentDirection) // Onshore winds.
            {
                precipitationEstimate++;
            }
            if (shore && windDirection == currentDirection) // Offshore winds.
            {
                precipitationEstimate--;
            }

            if (windType == WindType.trade && windMagnitude >= windPressureCutoff
                || windType == WindType.easterly && windMagnitude >= windPressureCutoff) // Low pressure areas (equator and polar fronts)
            {
                precipitationEstimate++;
            }
            else if (windType == WindType.westerly && windMagnitude >= windPressureCutoff) // High pressure areas (subtropical ridges)
            {
                precipitationEstimate--;
            }

            precipitationEstimate += Random.Range(-1, 2);

            if (precipitationEstimate >= map.settings.veryHighPrecipitationCutoff)
            {
                hex.precipitationColor = Color.blue;
                return Gradient.veryHigh;
            }
            else if (precipitationEstimate >= map.settings.highPrecipitationCutoff)
            {
                hex.precipitationColor = Color.cyan;
                return Gradient.high;
            }
            else if (precipitationEstimate <= map.settings.lowPrecipitationCutoff)
            {
                hex.precipitationColor = Color.yellow;
                return Gradient.low;
            }
            else if (precipitationEstimate <= map.settings.veryLowPrecipitationCutoff)
            {
                hex.precipitationColor = Color.red;
                return Gradient.veryLow;
            }
            else
            {
                hex.precipitationColor = Color.green;
                return Gradient.mid;
            }
        }

        #endregion
    }

    public class TectonicPlate
    {
        public List<Tile> tiles;

        public Color color;

        public TectonicPlate(Tile t)
        {
            tiles = new List<Tile>();

            tiles.Add(t);

            t.plate = this;
        }
    }

    public class Resource
    {
        public int yield; // How much of it one gets per turn.
        public bool animal; // Determines disease and ethics.
        public bool edible; // Can be eaten.
        public bool drinkable; // Can be drunk (drinken sounds better).
        public bool buildable; // Can be used to make structures.
        public Type type; // What sort of thing it is.

        public enum Type { water, tea, wheat, barley, rice, millet, maize, potatoes, sugarCane, banana, coffee, chocolate, grapes, olives, cotton, pork, poultry, beef, goat, sheep, fish, lumber, tin, iron, copper, stone } // These are just placeholders.

        public Resource(Resource.Type type, int yield, bool animal, bool edible, bool drinkable, bool buildable)
        {
            this.type = type;
            this.yield = yield;
            this.animal = animal;
            this.edible = edible;
            this.drinkable = drinkable;
            this.buildable = buildable;
        }
    }
}
