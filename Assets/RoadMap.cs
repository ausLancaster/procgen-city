using CSharpQuadTree;
using System.Collections.Generic;
using UnityEngine;

public class RoadMap : MonoBehaviour {

    QuadTree<Segment> quadTree;
    public List<Road> allRoads { get; set; }
    public QuadTree<Segment> QuadTree {
        get
        {
            if (quadTree == null) Initialize();
            return quadTree;
        }
    }

    private void Initialize()
    {
        quadTree = new QuadTree<Segment>(
            new Size(CityConfig.QUADTREE_MIN_SIZE, CityConfig.QUADTREE_MIN_SIZE),
            CityConfig.QUADTREE_MAX_OBJECTS
        );
    }

    public void AddRoad(Road road)
    {
        if (quadTree == null) Initialize();
        quadTree.Insert(road);
        if (allRoads == null) allRoads = new List<Road>();
        allRoads.Add(road);
        
    }

    public void AddJunction(Junction junction)
    {
        if (!junction.added)
        {
            if (quadTree == null) Initialize();
            quadTree.Insert(junction);
            junction.added = true;
        }
    }

}
