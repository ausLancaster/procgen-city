
using UnityEngine;

public static class CityConfig {

    // road generation

    public const float WIDTH = 4000f;
    public const float HEIGHT = 4000f;
    public const float X_START = -2000f;
    public const float Y_START = -2000f; // -2000f
    public const int MAX_ROADS = 7500;
    public const float STREET_SEGMENT_LENGTH = 30f; //30f
    public const float HIGHWAY_SEGMENT_LENGTH = 40f; //40f
    public const float STREET_SEGMENT_WIDTH = 4f;
    public const float HIGHWAY_SEGMENT_WIDTH = 8f;
    public const float MAX_STRAIGHT_ANGLE = 30f; // 30f
    public const float MAX_BRANCH_ANGLE = 15f; //15f
    public const float HIGHWAY_BRANCH_POPULATION_THRESHOLD = 0.3f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD = 0.6f;
    public const float HIGHWAY_BRANCH_PROBABILITY = 0.05f;
    public const float STREET_BRANCH_PROBABILITY = 0.4f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD_BRANCH_PROBABILITY = 0.4f;
    public const int STREET_FROM_HIGHWAY_DELAY = 100;
    public const int QUADTREE_MAX_OBJECTS = 10;
    public const float QUADTREE_MIN_SIZE = 32f;
    public const float MIN_INTERSECTION_ANGLE = 30f;
    public const float ROAD_SNAP_DISTANCE = 100f;
    public const float JUNCTION_SIZE = 8f;
    public const bool SHOW_JUNCTIONS = true;

    public static float RandomStraightAngle()
    {
        return RandomAngle(MAX_STRAIGHT_ANGLE);
    }

    public static float RandomBranchAngle()
    {
        return RandomAngle(MAX_BRANCH_ANGLE);
    }

    public static float RandomAngle(float limit)
    {
        return Random.Range(-limit, limit);
    }

    // building generation
}
