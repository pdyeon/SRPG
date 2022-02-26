using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;

public class HexMap : MonoBehaviour, IQPathWorld
{
    void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(units != null)
            {
                foreach(Unit u in units)
                {
                    u.DoTurn();
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            if (units != null)
            {
                foreach (Unit u in units)
                {
                    u.DUMMY_PATHING_FUNCTION();
                }
            }
        }
    }

    public GameObject HexPrefab;

    public GameObject ForestPrefab;
    public GameObject JunglePrefab;

    public Material MatOcean;
    public Material MatPlains;
    public Material MatGrassLand;
    public Material MatMountains;
    public Material MatDesert;

    public GameObject PlayerPrefab;

    [System.NonSerialized] public float HeightMountain = 1f;
    [System.NonSerialized] public float HeightHill = 0.6f;
    [System.NonSerialized] public float HeightFlat = 0.0f;

    [System.NonSerialized] public float MoistureJungle = 1f;
    [System.NonSerialized] public float MoistureForest = 0.5f;
    [System.NonSerialized] public float MoistureGrasslands = 0f;
    [System.NonSerialized] public float MoisturePlains = -0.5f;

    [System.NonSerialized] public readonly int NumRows = 30;
    [System.NonSerialized] public readonly int NumColumns = 60;

    [System.NonSerialized] public bool AllowWrapEastWest = true;
    [System.NonSerialized] public bool AllowWrapNorthSouth = true;

    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;

    public Hex GetHexAt(int x, int y)
    {
        if(hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated");
            return null;
        }

        if (AllowWrapEastWest)
        {
            x = x % NumColumns;
            if(x < 0)
            {
                x += NumColumns;
            }
        }
        if (AllowWrapNorthSouth)
        {
            y = y % NumRows;
            if (y < 0)
            {
                y += NumRows;
            }
        }
            

        try { return hexes[x, y]; }
        catch
        {
            Debug.LogError("GetHexAt:" + x + "," + y);
            return null;
        }
        
    }

    public Hex GetHexFromGameObject(GameObject HexGo)
    {
        if (gameObjectToHexMap.ContainsKey(HexGo))
        {
            return gameObjectToHexMap[HexGo];
        }

        return null;
    }

    public GameObject GetHexGO(Hex h)
    {
        if(hexToGameObjectMap.ContainsKey(h))
        {
            return hexToGameObjectMap[h];
        }

        return null;
    }

    public Vector3 GetHexPosition(int q, int r)
    {
        Hex hex = GetHexAt(q, r);

        return GetHexPosition(hex);
    }

    public Vector3 GetHexPosition(Hex hex)
    {
        return hex.PositionFromCamera(Camera.main.transform.position, NumRows, NumColumns);
    }

    virtual public void GenerateMap()
    {
        hexes = new Hex[NumColumns, NumRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        for (int column = 0; column < NumColumns; column++)
        {
            for(int row = 0; row < NumRows; row++)
            {
                Hex h = new Hex(this, column, row);
                h.Elevation = -0.5f;

                hexes[column, row] = h;

                Vector3 pos = h.PositionFromCamera(
                    Camera.main.transform.position,
                    NumRows,
                    NumColumns
                    );

                GameObject hexGo = (GameObject)Instantiate(
                    HexPrefab,
                    pos,
                    Quaternion.identity,
                    this.transform
                    );

                hexToGameObjectMap[h] = hexGo;
                gameObjectToHexMap[hexGo] = h;

                h.TerrainType = Hex.TERRAIN_TYPE.OCEAN;
                h.ElevationType = Hex.ELEVATION_TYPE.WATER;

                hexGo.name = string.Format("HEX: {0},{1}", column, row);
                hexGo.GetComponent<HexComponent>().Hex = h;
                hexGo.GetComponent<HexComponent>().HexMap = this;

                //MeshRenderer mr = hexGo.GetComponentInChildren<MeshRenderer>();
                //mr.material = MatOcean;
            }
        }

        UpdateHexVisuals();

        //StaticBatchingUtility.Combine(this.gameObject);
    }

    public void UpdateHexVisuals()
    {
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex h = hexes[column, row];
                GameObject hexGo = hexToGameObjectMap[h];

                MeshRenderer mr = hexGo.GetComponentInChildren<MeshRenderer>();

                if (h.Elevation >= HeightFlat && h.Elevation < HeightMountain)
                {
                    if (h.Moisture >= MoistureJungle)
                    {
                        mr.material = MatGrassLand;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                        h.FeatureType = Hex.FEATURE_TYPE.RAINFOREST;

                        Vector3 p = hexGo.transform.position;

                        GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGo.transform);
                    }
                    else if (h.Moisture >= MoistureForest)
                    {
                        mr.material = MatGrassLand;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                        h.FeatureType = Hex.FEATURE_TYPE.FOREST;

                        Vector3 p = hexGo.transform.position;

                        GameObject.Instantiate(ForestPrefab, p, Quaternion.identity, hexGo.transform);
                    }
                    else if (h.Moisture >= MoistureGrasslands)
                    {
                        mr.material = MatGrassLand;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;

                        Vector3 p = hexGo.transform.position;
                        GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGo.transform);
                    }
                    else if (h.Moisture >= MoisturePlains)
                    {
                        mr.material = MatPlains;
                        h.TerrainType = Hex.TERRAIN_TYPE.PLAINS;
                    }
                    else
                    {
                        mr.material = MatDesert;
                        h.TerrainType = Hex.TERRAIN_TYPE.DESERT;
                    }
                }

                if (h.Elevation >= HeightMountain)
                {
                    mr.material = MatMountains;
                    h.ElevationType =  Hex.ELEVATION_TYPE.MOUNTAIN;
                }
                else if (h.Elevation >= HeightHill)
                {
                    h.ElevationType = Hex.ELEVATION_TYPE.HILL;
                } // mr.material = MatGrassLand;
                else if (h.Elevation >= HeightFlat)
                {
                    h.ElevationType = Hex.ELEVATION_TYPE.FLAT;
                } // mr.material = MatPlains;
                else
                {
                    h.ElevationType = Hex.ELEVATION_TYPE.WATER;
                    mr.material = MatOcean;
                }

                hexGo.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}\n{2}", column, row, h.BaseMovementCost(false, false, false));

            }
        }
    }

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range)
    {
        List<Hex> results = new List<Hex>();

        for(int dx = -range; dx < range-1; dx++)
        {
            for(int dy = Mathf.Max(-range+1, -dx-range); dy < Mathf.Min(range, -dx+range-1); dy++)
            {
                results.Add(GetHexAt(centerHex.Q + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }

    public void SpawnUnitAt( Unit unit, GameObject prefab, int q, int r )
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }

        Hex myHex = GetHexAt(q, r);
        GameObject myHexGo = hexToGameObjectMap[myHex];
        unit.SetHex(myHex);

        GameObject unitGo = (GameObject)Instantiate(prefab, myHexGo.transform.position, Quaternion.identity, myHexGo.transform);
        unit.OnUnitMoved += unitGo.GetComponent<UnitView>().OnUnitMoved;

        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitGo);
    }
}
