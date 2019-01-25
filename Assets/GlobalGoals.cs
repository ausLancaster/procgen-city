using System.Collections.Generic;
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
            prevSegment.t + 1,
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
                prevSegment.t + 1,
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

                    Road leftRoad = AddBranch(
                        prevSegment,
                        straight,
                        j,
                        prevSegment.transform.localEulerAngles.y - 90 + CityConfig.RandomBranchAngle(),
                        prevSegment.length,
                        prevSegment.t + 1,
                        prevSegment.type
                        );
                    newRoads.Add(leftRoad);
                }
                if (Random.value < CityConfig.HIGHWAY_BRANCH_PROBABILITY)
                {
                    Road rightRoad = AddBranch(
                        prevSegment,
                        straight,
                        j,
                        prevSegment.transform.localEulerAngles.y + 90 + CityConfig.RandomBranchAngle(),
                        prevSegment.length,
                        prevSegment.t + 1,
                        prevSegment.type
                        );
                    newRoads.Add(rightRoad);
                }


            }
        }
        else if (straightPop > CityConfig.STREET_BRANCH_POPULATION_THRESHOLD)
        {
            // do not delete continueStraight
            newRoads.Add(continueStraight);
        }
        else
        {
            DestroyImmediate(continueStraight.gameObject);
        }

        // street branches off either highway or streets
        if (straightPop > CityConfig.STREET_BRANCH_POPULATION_THRESHOLD)
        {
            int t = (prevSegment.type == RoadType.Highway) ? CityConfig.STREET_FROM_HIGHWAY_DELAY : 1;

            Junction j = null;
            if (Random.value < CityConfig.STREET_BRANCH_PROBABILITY)
            {
                Road leftRoad = AddBranch(
                    prevSegment,
                    continueStraight,
                    j,
                    prevSegment.transform.localEulerAngles.y - 90 + CityConfig.RandomBranchAngle(),
                    CityConfig.STREET_SEGMENT_LENGTH,
                    prevSegment.t + t,
                    RoadType.Street
                    );
                newRoads.Add(leftRoad);
            }
            if (Random.value < CityConfig.STREET_BRANCH_PROBABILITY)
            {
                Road leftRoad = AddBranch(
                    prevSegment,
                    continueStraight,
                    j,
                    prevSegment.transform.localEulerAngles.y + 90 + CityConfig.RandomBranchAngle(),
                    CityConfig.STREET_SEGMENT_LENGTH,
                    prevSegment.t + t,
                    RoadType.Street
                    );
                newRoads.Add(leftRoad);
            }
        }

        // set up links between roads
        foreach (Road r in newRoads)
        {
            r.Parent = prevSegment;
            prevSegment.next.Add(new Road.Neighbour(r, true));
            foreach (Road r_other in newRoads)
            {
                if (r_other.id != r.id)
                {
                    r.prev.Add(new Road.Neighbour(r_other, false));
                }
            }
        }

        return newRoads;
    }

    Road AddBranch(Road prevSegment, Road straight, Junction j, float angle, float length, float t, RoadType type)
    {

        Road branchRoad = segFactory.CreateRoad(
            prevSegment.end,
            Quaternion.Euler(prevSegment.transform.localEulerAngles.x, angle, 0),
            length,
            t,
            type
            );
        if (j == null)
        {
            j = segFactory.CreateJunction(prevSegment.end, Quaternion.identity);
            straight.attachedSegments.Add(j);
            j.outgoing.Add(straight);
        }
        branchRoad.attachedSegments.Add(j);
        j.incoming.Add(prevSegment);
        j.outgoing.Add(branchRoad);

        return branchRoad;
    }
}
