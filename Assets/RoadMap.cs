using CSharpQuadTree;
using UnityEngine;

public class RoadMap : MonoBehaviour {

    QuadTree<Segment> quadTree;
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
