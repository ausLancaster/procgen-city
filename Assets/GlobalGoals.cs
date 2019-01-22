using System.Collections.Generic;
using UnityEngine;

public class GlobalGoals : MonoBehaviour {

    SegmentFactory segFactory;

    private void Start()
    {
        segFactory = GetComponent<SegmentFactory>();
    }

    public List<Road> Generate(Road prevSegment)
    {
        var newRoads = new List<Road>();

        Road continueStraight = segFactory.Create(
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
            float newAngle = prevSegment.transform.localRotation.eulerAngles.y + CityConfig.RandomStraightAngle();
            print(newAngle);
            Road randomStraight = segFactory.Create(
                prevSegment.end,
                Quaternion.Euler(0, newAngle, 0),
                prevSegment.length,
                0,
                prevSegment.type
                );

            float randomPop = Heatmap.Value(randomStraight.end);
            float roadPop;
            if (randomPop > straightPop)
            {
                newRoads.Add(randomStraight);
                roadPop = randomPop;
                DestroyImmediate(continueStraight.gameObject);
            } else
            {
                newRoads.Add(continueStraight);
                roadPop = straightPop;
                DestroyImmediate(randomStraight.gameObject);
            }

            if (roadPop > CityConfig.HIGHWAY_BRANCH_POPULATION_THRESHOLD)
            {
                if (Random.value < CityConfig.HIGHWAY_BRANCH_PROBABILITY)
                {
                    float leftAngle = prevSegment.transform.localRotation.eulerAngles.y - 90 + CityConfig.RnadomBranchAngle();
                    Road leftRoad = segFactory.Create(
                        prevSegment.end,
                        Quaternion.Euler(0, leftAngle, 0),
                        prevSegment.length,
                        0,
                        prevSegment.type
                        );

                    newRoads.Add(leftRoad);
                }
                else if (Random.value < CityConfig.HIGHWAY_BRANCH_PROBABILITY)
                {
                    float rightAngle = prevSegment.transform.localRotation.eulerAngles.y + 90 + CityConfig.RnadomBranchAngle();
                    Road rightRoad = segFactory.Create(
                        prevSegment.end,
                        Quaternion.Euler(0, rightAngle, 0),
                        prevSegment.length,
                        0,
                        prevSegment.type
                        );

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

        return newRoads;
    }
}
