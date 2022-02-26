using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QPath;

public class Unit : IQPathUnit
{
    public string Name = "Unnamed";
    public int HitPoints = 100;
    public int Strenth = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

    public Hex Hex { get; protected set; }

    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    // List of hexes to walk through (from pathfinder)
    // NOTE: First item is always the hex we are standing in

    Queue<Hex> hexPath;

    const bool MOVEMENT_RULES_LIIKE_CIV6 = false;

    public void SetHex(Hex newHex)
    {
        Hex oldHex = Hex;
        if(Hex != null)
        {
            Hex.RemoveUnit(this);
        }

        Hex = newHex;

        Hex.AddUnit(this);

        if(OnUnitMoved != null)
        {
            OnUnitMoved(oldHex, newHex);
        }
    }
    
    public void DUMMY_PATHING_FUNCTION()
    {
        //QPath.CostEstimateDelegate ced = (IQPathTile a, IQPathTile b) => (

        //Hex.Distance(a, b)
        //);

        Hex[] pathHexes = QPath.QPath.FindPath<Hex>(
            Hex.HexMap,
            this,
            Hex,
            Hex.HexMap.GetHexAt(Hex.Q + 6, Hex.R),
            Hex.CostEstimate
            );

        //Hex[] pathHexes = System.Array.ConvertAll(pathTiles, a => (Hex)a);

        SetHexPath(pathHexes);

    }

    public void ClearHexPath()
    {
        this.hexPath = new Queue<Hex>();
    }

    public void SetHexPath(Hex[] hexArray)
    {
        this.hexPath = new Queue<Hex>(hexArray);
    }

    public Hex[] GetHexPath()
    {
        return (this.hexPath == null) ? null : this.hexPath.ToArray();
    }

    public void DoTurn()
    {
        // Do queued move

        if(hexPath == null || hexPath.Count == 0)
        {
            return;
        }

        // Grab the first hex from our queue
        /*Hex hexWeAreLeaving =*/ hexPath.Dequeue();
        Hex newHex = hexPath.Peek();

        if(hexPath.Count == 1)
        {
            // The only hex left in the list, is the one we are moving to now,
            // therefore we have no more path to follow, so let's just clear
            // the queue completely to avoid confusion.
            hexPath = null;
        }

        // Move to the new Hex
        SetHex(newHex);
    }

    public int MovementCostToEnterHex(Hex hex)
    {
        // TODO: Implement different movement traints

        return hex.BaseMovementCost(false, false, false);
    }

    public float AggregateTurnsToEnterHex(Hex hex, float turnsToDate)
    {
        // The issue at hand is that if you are trying to enter a tile
        // with a movement cost greater than your current remaining movement
        // points, this will either result in a cheaper-than expected
        // turn cost (Civ5) or a more-expensive-than expected turn cost (Civ6)

        float baseTrunToEnterHex = MovementCostToEnterHex(hex) / Movement;

        if(baseTrunToEnterHex < 0)
        {
            //Impassible terrain
            //Debug.Log("Impassible terrain");
            return -99999;
        }

        if(baseTrunToEnterHex > 1)
        {
            baseTrunToEnterHex = 1;
        }

        float turnRemaining = MovementRemaining / Movement;

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;

        if( (turnsToDateFraction > 0 && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f)
        {
            if (turnsToDateFraction < 0.01f) turnsToDateFraction = 0;
            if(turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }
        }

        float turnsUsedAfterThismove = turnsToDateFraction + baseTrunToEnterHex;

        if(turnsUsedAfterThismove > 1)
        {
            if(MOVEMENT_RULES_LIIKE_CIV6)
            {
                if (turnsToDateFraction == 0)
                {

                }
                else
                {
                    turnsToDateWhole += 1;
                    turnsToDateFraction = 0;
                }
                turnsUsedAfterThismove = baseTrunToEnterHex;
            }
            else
            {
                turnsUsedAfterThismove = 1;
            }
        }

        return turnsToDateWhole + turnsUsedAfterThismove;

    }

    public float CostToEnterHex(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        return 1;
    }

}
