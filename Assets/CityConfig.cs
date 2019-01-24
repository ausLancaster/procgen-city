﻿
using UnityEngine;

public static class CityConfig {

    public const float WIDTH = 4000f;
    public const float HEIGHT = 4000f;
    public const float X_START = -2000f;
    public const float Y_START = -2000f;
    public const int MAX_ROADS = 4000;
    public const float ROAD_SEGMENT_LENGTH = 30f;
    public const float HIGHWAY_SEGMENT_LENGTH = 40f;
    public const float STREET_SEGMENT_WIDTH = 9f;
    public const float HIGHWAY_SEGMENT_WIDTH = 8f; // 16f
    public const float MAX_STRAIGHT_ANGLE = 20f; // 15f
    public const float MAX_BRANCH_ANGLE = 3f;
    public const float HIGHWAY_BRANCH_POPULATION_THRESHOLD = 0.2f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD = 0.3f;
    public const float HIGHWAY_BRANCH_PROBABILITY = 0.05f;
    public const float STREET_BRANCH_POPULATION_THRESHOLD_BRANCH_PROBABILITY = 0.4f;
    public const float JUNCTION_SIZE = 16f;
    public const int QUADTREE_MAX_OBJECTS = 10;
    public const float QUADTREE_MIN_SIZE = 32f;
    public const float MIN_INTERSECTION_ANGLE = 20f;
    public const float ROAD_SNAP_DISTANCE = 100f;
    public const bool SHOW_JUNCTIONS = true;



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
