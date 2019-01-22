
using UnityEngine;

public static class CityConfig {

    public const float WIDTH = 4000f;
    public const float HEIGHT = 4000f;
    public const float X_START = -2000f;
    public const float Y_START = -2000f;
    public const int MAX_ROADS = 300;
    public const float ROAD_SEGMENT_LENGTH = 30f;
    public const float HIGHWAY_SEGMENT_LENGTH = 40f;
    public const float STREET_SEGMENT_WIDTH = 9f;
    public const float HIGHWAY_SEGMENT_WIDTH = 16f;
    public const float MAX_STRAIGHT_ANGLE = 15f;
    public const float MAX_BRANCH_ANGLE = 3f;
    public const float HIGHWAY_BRANCH_POPULATION_THRESHOLD = 0.1f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD = 0.1f;
    public const float HIGHWAY_BRANCH_PROBABILITY = 0.05f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD_BRANCH_PROBABILITY = 0.4f;
    public const float JUNCTION_SIZE = 15f;



    public static float RandomStraightAngle()
    {
        return RandomAngle(MAX_STRAIGHT_ANGLE);
    }

    public static float RnadomBranchAngle()
    {
        return RandomAngle(MAX_BRANCH_ANGLE);
    }

    public static float RandomAngle(float limit)
    {
        return Random.Range(-limit, limit);
    }
}
