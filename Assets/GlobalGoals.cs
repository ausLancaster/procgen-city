﻿using System.Collections.Generic;
using UnityEngine;

public class GlobalGoals : MonoBehaviour {

    SegmentFactory segFactory;

    private void Initialize()
    {
        segFactory = GetComponent<SegmentFactory>();
    }

    public List<Road> Generate(Road prevSegment, RoadMap roadMap)
    {
        var newRoads = new List<Road>();

        if (prevSegment.severed) return newRoads;

        if (segFactory == null) Initialize();
        Road continueStraight = segFactory.CreateRoad(
            prevSegment.end,
            prevSegment.transform.localRotation,
            prevSegment.length,
            0,
            prevSegment.type
            );
        float straightPop = Heatmap.Value(continueStraight.end);

        if (prevSegment.type == RoadType.Highway)
        {
            // generate a random deviation and compare to going straight, 
            // chosing the road that leads to the highest population
            float newAngle = prevSegment.transform.localEulerAngles.y + CityConfig.RandomStraightAngle();

            Road randomStraight = segFactory.CreateRoad(
                prevSegment.end,
                Quaternion.Euler(prevSegment.transform.localEulerAngles.x, newAngle, 0),
                prevSegment.length,
                0,
                prevSegment.type
                );

            float randomPop = Heatmap.Value(randomStraight.end);
            float roadPop;
            Road straight;
            if (randomPop > straightPop)
            {
                newRoads.Add(randomStraight);
                roadPop = randomPop;
                straight = randomStraight;
                DestroyImmediate(continueStraight.gameObject);
            } else
            {
                newRoads.Add(continueStraight);
                roadPop = straightPop;
                straight = continueStraight;
                DestroyImmediate(randomStraight.gameObject);
            }

            // highway branches off highway
            if (roadPop > CityConfig.HIGHWAY_BRANCH_POPULATION_THRESHOLD)
            {
                Junction j = null;
                if (Random.value < CityConfig.HIGHWAY_BRANCH_PROBABILITY)
                {
                    float leftAngle = prevSegment.transform.localEulerAngles.y - 90 + CityConfig.RnadomBranchAngle();
                    Road leftRoad = segFactory.CreateRoad(
                        prevSegment.end,
                        Quaternion.Euler(prevSegment.transform.localEulerAngles.x, leftAngle, 0),
                        prevSegment.length,
                        0,
                        prevSegment.type
                        );
                    j = segFactory.CreateJunction(prevSegment.end, Quaternion.identity);
                    leftRoad.attachedSegments.Add(j);
                    straight.attachedSegments.Add(j);
                    roadMap.AddJunction(j);
                    newRoads.Add(leftRoad);
                }
                if (Random.value < CityConfig.HIGHWAY_BRANCH_PROBABILITY)
                {
                    float rightAngle = prevSegment.transform.localEulerAngles.y + 90 + CityConfig.RnadomBranchAngle();
                    Road rightRoad = segFactory.CreateRoad(
                        prevSegment.end,
                        Quaternion.Euler(prevSegment.transform.localEulerAngles.x, rightAngle, 0),
                        prevSegment.length,
                        0,
                        prevSegment.type
                        );
                    if (j == null)
                    {
                        j = segFactory.CreateJunction(prevSegment.end, Quaternion.identity);
                        roadMap.AddJunction(j);
                    }
                    rightRoad.attachedSegments.Add(j);
                    straight.attachedSegments.Add(j);
                    newRoads.Add(rightRoad);
                }


            }
        }
        /*
        else if (straightPop > CityConfig.ROAD_BRANCH_POPULATION_THRESHOLD)
        {
            // do not delete continueStraight
            //newRoads.Add(continueStraight);
            DestroyImmediate(continueStraight);// remove this line
        }
        else
        {
            DestroyImmediate(continueStraight);
        }*/

        // set up links between roads
        foreach (Road r in newRoads)
        {
            r.prev.Add(prevSegment);
            prevSegment.next.Add(r);
        }

        return newRoads;
    }
}
