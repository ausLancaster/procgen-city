using CSharpQuadTree;
using UnityEngine;

public class RoadMap : MonoBehaviour {

    QuadTree<Road> quadTree;
    public QuadTree<Road> QuadTree {
        get
        {
            if (quadTree == null) Initialize();
            return quadTree;
        }
    }

    private void Initialize()
    {
        quadTree = new QuadTree<Road>(
            new Size(CityConfig.QUADTREE_MIN_SIZE, CityConfig.QUADTREE_MIN_SIZE),
            CityConfig.QUADTREE_MAX_OBJECTS
        );
    }

    public void AddRoad(Road road)
    {
        if (quadTree == null) Initialize();
        road.enabled = true;
        quadTree.Insert(road);
        
    }

}
