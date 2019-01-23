using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CSharpQuadTree
{
    public class QuadTree<T> where T : class, IQuadObject
    {
        private readonly bool sort;
        private readonly Size minLeafSize;
        private readonly int maxObjectsPerLeaf;
        private QuadNode root = null;
        private Dictionary<T, QuadNode> objectToNodeLookup = new Dictionary<T, QuadNode>();
        private Dictionary<T, int> objectSortOrder = new Dictionary<T, int>();
        public QuadNode Root { get { return root; } }
        private object syncLock = new object();
        private int objectSortId = 0;

        public QuadTree(Size minLeafSize, int maxObjectsPerLeaf)
        {
            this.minLeafSize = minLeafSize;
            this.maxObjectsPerLeaf = maxObjectsPerLeaf;
        }

        public int GetSortOrder(T quadObject)
        {
            lock (objectSortOrder)
            {
                if (!objectSortOrder.ContainsKey(quadObject))
                    return -1;
                else
                {
                    return objectSortOrder[quadObject];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLeafSize">The smallest size a leaf will split into</param>
        /// <param name="maxObjectsPerLeaf">Maximum number of objects per leaf before it forces a split into sub quadrants</param>
        /// <param name="sort">Whether or not queries will return objects in the order in which they were added</param>
        public QuadTree(Size minLeafSize, int maxObjectsPerLeaf, bool sort)
            : this(minLeafSize, maxObjectsPerLeaf)
        {
            this.sort = sort;
        }

        public void Insert(T quadObject)
        {
            lock (syncLock)
            {
                if (sort & !objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Add(quadObject, objectSortId++);
                }

                Rect bounds = quadObject.Bounds;
                if (root == null)
                {
                    var rootSize = new Vector2(Mathf.Ceil(bounds.width / minLeafSize.Width),
                                            Mathf.Ceil(bounds.height / minLeafSize.Height));
                    float multiplier = Mathf.Max(rootSize.x, rootSize.y);
                    rootSize = new Vector2(minLeafSize.Width * multiplier, minLeafSize.Height * multiplier);
                    var center = new Vector2(bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
                    var rootOrigin = new Vector2(center.x - rootSize.x / 2, center.y - rootSize.y / 2);
                    root = new QuadNode(new Rect(rootOrigin, rootSize));
                }
                while (!Contains(root.Bounds, bounds))
                {
                    ExpandRoot(bounds);
                }

                InsertNodeObject(root, quadObject);
            }
        }

        public List<T> Query(Rect bounds)
        {
            lock (syncLock)
            {
                List<T> results = new List<T>();
                if (root != null)
                    Query(bounds, root, results);
                if (sort)
                    results.Sort((a, b) => { return objectSortOrder[a].CompareTo(objectSortOrder[b]); });
                return results;
            }
        }

        private void Query(Rect bounds, QuadNode node, List<T> results)
        {
            lock (syncLock)
            {
                if (node == null) return;

                if (bounds.Overlaps(node.Bounds))
                {
                    foreach (T quadObject in node.Objects)
                    {
                        if (bounds.Overlaps(quadObject.Bounds))
                            results.Add(quadObject);
                    }

                    foreach (QuadNode childNode in node.Nodes)
                    {
                        Query(bounds, childNode, results);
                    }
                }
            }
        }

        private void ExpandRoot(Rect newChildBounds)
        {
            lock (syncLock)
            {
                bool isNorth = root.Bounds.y < newChildBounds.y;
                bool isWest = root.Bounds.x < newChildBounds.x;

                Direction rootDirection;
                if (isNorth)
                {
                    rootDirection = isWest ? Direction.NW : Direction.NE;
                }
                else
                {
                    rootDirection = isWest ? Direction.SW : Direction.SE;
                }

                float newX = (rootDirection == Direction.NW || rootDirection == Direction.SW)
                                  ? root.Bounds.x
                                  : root.Bounds.x - root.Bounds.width;
                float newY = (rootDirection == Direction.NW || rootDirection == Direction.NE)
                                  ? root.Bounds.y
                                  : root.Bounds.y - root.Bounds.height;
                Rect newRootBounds = new Rect(newX, newY, root.Bounds.width * 2, root.Bounds.height * 2);
                QuadNode newRoot = new QuadNode(newRootBounds);
                SetupChildNodes(newRoot);
                newRoot[rootDirection] = root;
                root = newRoot;
            }
        }

        private void InsertNodeObject(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                if (!Contains(node.Bounds, quadObject.Bounds))
                    throw new Exception("This should not happen, child does not fit within node bounds");

                if (!node.HasChildNodes() && node.Objects.Count + 1 > maxObjectsPerLeaf)
                {
                    SetupChildNodes(node);

                    List<T> childObjects = new List<T>(node.Objects);
                    List<T> childrenToRelocate = new List<T>();

                    foreach (T childObject in childObjects)
                    {
                        foreach (QuadNode childNode in node.Nodes)
                        {
                            if (childNode == null)
                                continue;

                            if (Contains(childNode.Bounds, childObject.Bounds))
                            {
                                childrenToRelocate.Add(childObject);
                            }
                        }
                    }

                    foreach (T childObject in childrenToRelocate)
                    {
                        RemoveQuadObjectFromNode(childObject);
                        InsertNodeObject(node, childObject);
                    }
                }

                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode != null)
                    {
                        if (Contains(childNode.Bounds, quadObject.Bounds))
                        {
                            InsertNodeObject(childNode, quadObject);
                            return;
                        }
                    }
                }

                AddQuadObjectToNode(node, quadObject);
            }
        }

        private void ClearQuadObjectsFromNode(QuadNode node)
        {
            lock (syncLock)
            {
                List<T> quadObjects = new List<T>(node.Objects);
                foreach (T quadObject in quadObjects)
                {
                    RemoveQuadObjectFromNode(quadObject);
                }
            }
        }

        private void RemoveQuadObjectFromNode(T quadObject)
        {
            lock (syncLock)
            {
                QuadNode node = objectToNodeLookup[quadObject];
                node.quadObjects.Remove(quadObject);
                objectToNodeLookup.Remove(quadObject);
                quadObject.BoundsChanged -= new EventHandler(quadObject_BoundsChanged);
            }
        }

        private void AddQuadObjectToNode(QuadNode node, T quadObject)
        {
            lock (syncLock)
            {
                node.quadObjects.Add(quadObject);
                objectToNodeLookup.Add(quadObject, node);
                quadObject.BoundsChanged += new EventHandler(quadObject_BoundsChanged);
            }
        }

        void quadObject_BoundsChanged(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                T quadObject = sender as T;
                if (quadObject != null)
                {
                    QuadNode node = objectToNodeLookup[quadObject];
                    if (!Contains(node.Bounds, quadObject.Bounds) || node.HasChildNodes())
                    {
                        RemoveQuadObjectFromNode(quadObject);
                        Insert(quadObject);
                        if (node.Parent != null)
                        {
                            CheckChildNodes(node.Parent);
                        }
                    }
                }
            }
        }

        private void SetupChildNodes(QuadNode node)
        {
            lock (syncLock)
            {
                if (minLeafSize.Width <= node.Bounds.width / 2 && minLeafSize.Height <= node.Bounds.height / 2)
                {
                    node[Direction.NW] = new QuadNode(node.Bounds.x, node.Bounds.y, node.Bounds.width / 2,
                                                      node.Bounds.height / 2);
                    node[Direction.NE] = new QuadNode(node.Bounds.x + node.Bounds.width / 2, node.Bounds.y,
                                                      node.Bounds.width / 2,
                                                      node.Bounds.height / 2);
                    node[Direction.SW] = new QuadNode(node.Bounds.x, node.Bounds.y + node.Bounds.height / 2,
                                                      node.Bounds.width / 2,
                                                      node.Bounds.height / 2);
                    node[Direction.SE] = new QuadNode(node.Bounds.x + node.Bounds.width / 2,
                                                      node.Bounds.y + node.Bounds.height / 2,
                                                      node.Bounds.width / 2, node.Bounds.height / 2);

                }
            }
        }

        public void Remove(T quadObject)
        {
            lock (syncLock)
            {
                if (sort && objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Remove(quadObject);
                }

                if (!objectToNodeLookup.ContainsKey(quadObject))
                    throw new KeyNotFoundException("QuadObject not found in dictionary for removal");

                QuadNode containingNode = objectToNodeLookup[quadObject];
                RemoveQuadObjectFromNode(quadObject);

                if (containingNode.Parent != null)
                    CheckChildNodes(containingNode.Parent);
            }
        }



        private void CheckChildNodes(QuadNode node)
        {
            lock (syncLock)
            {
                if (GetQuadObjectCount(node) <= maxObjectsPerLeaf)
                {
                    // Move child objects into this node, and delete sub nodes
                    List<T> subChildObjects = GetChildObjects(node);
                    foreach (T childObject in subChildObjects)
                    {
                        if (!node.Objects.Contains(childObject))
                        {
                            RemoveQuadObjectFromNode(childObject);
                            AddQuadObjectToNode(node, childObject);
                        }
                    }
                    if (node[Direction.NW] != null)
                    {
                        node[Direction.NW].Parent = null;
                        node[Direction.NW] = null;
                    }
                    if (node[Direction.NE] != null)
                    {
                        node[Direction.NE].Parent = null;
                        node[Direction.NE] = null;
                    }
                    if (node[Direction.SW] != null)
                    {
                        node[Direction.SW].Parent = null;
                        node[Direction.SW] = null;
                    }
                    if (node[Direction.SE] != null)
                    {
                        node[Direction.SE].Parent = null;
                        node[Direction.SE] = null;
                    }

                    if (node.Parent != null)
                        CheckChildNodes(node.Parent);
                    else
                    {
                        // Its the root node, see if we're down to one quadrant, with none in local storage - if so, ditch the other three
                        int numQuadrantsWithObjects = 0;
                        QuadNode nodeWithObjects = null;
                        foreach (QuadNode childNode in node.Nodes)
                        {
                            if (childNode != null && GetQuadObjectCount(childNode) > 0)
                            {
                                numQuadrantsWithObjects++;
                                nodeWithObjects = childNode;
                                if (numQuadrantsWithObjects > 1) break;
                            }
                        }
                        if (numQuadrantsWithObjects == 1)
                        {
                            foreach (QuadNode childNode in node.Nodes)
                            {
                                if (childNode != nodeWithObjects)
                                    childNode.Parent = null;
                            }
                            root = nodeWithObjects;
                        }
                    }
                }
            }
        }


        private List<T> GetChildObjects(QuadNode node)
        {
            lock (syncLock)
            {
                List<T> results = new List<T>();
                results.AddRange(node.quadObjects);
                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode != null)
                        results.AddRange(GetChildObjects(childNode));
                }
                return results;
            }
        }

        public int GetQuadObjectCount()
        {
            lock (syncLock)
            {
                if (root == null)
                    return 0;
                int count = GetQuadObjectCount(root);
                return count;
            }
        }

        private int GetQuadObjectCount(QuadNode node)
        {
            lock (syncLock)
            {
                int count = node.Objects.Count;
                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode != null)
                    {
                        count += GetQuadObjectCount(childNode);
                    }
                }
                return count;
            }
        }

        public int GetQuadNodeCount()
        {
            lock (syncLock)
            {
                if (root == null)
                    return 0;
                int count = GetQuadNodeCount(root, 1);
                return count;
            }
        }

        private int GetQuadNodeCount(QuadNode node, int count)
        {
            lock (syncLock)
            {
                if (node == null) return count;

                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode != null)
                        count++;
                }
                return count;
            }
        }

        public List<QuadNode> GetAllNodes()
        {
            lock (syncLock)
            {
                List<QuadNode> results = new List<QuadNode>();
                if (root != null)
                {
                    results.Add(root);
                    GetChildNodes(root, results);
                }
                return results;
            }
        }

        private void GetChildNodes(QuadNode node, ICollection<QuadNode> results)
        {
            lock (syncLock)
            {
                foreach (QuadNode childNode in node.Nodes)
                {
                    if (childNode != null)
                    {
                        results.Add(childNode);
                        GetChildNodes(childNode, results);
                    }
                }
            }
        }

        private bool Contains(Rect a, Rect b)
        {
            return a.Contains(new Vector2(b.xMin, b.yMin)) && a.Contains(new Vector2(b.xMax, b.yMax));
        }

        public class QuadNode
        {
            private static int _id = 0;
            public readonly int ID = _id++;

            public QuadNode Parent { get; internal set; }

            private QuadNode[] _nodes = new QuadNode[4];
            public QuadNode this[Direction direction]
            {
                get
                {
                    switch (direction)
                    {
                        case Direction.NW:
                            return _nodes[0];
                        case Direction.NE:
                            return _nodes[1];
                        case Direction.SW:
                            return _nodes[2];
                        case Direction.SE:
                            return _nodes[3];
                        default:
                            return null;
                    }
                }
                set
                {
                    switch (direction)
                    {
                        case Direction.NW:
                            _nodes[0] = value;
                            break;
                        case Direction.NE:
                            _nodes[1] = value;
                            break;
                        case Direction.SW:
                            _nodes[2] = value;
                            break;
                        case Direction.SE:
                            _nodes[3] = value;
                            break;
                    }
                    if (value != null)
                        value.Parent = this;
                }
            }

            public ReadOnlyCollection<QuadNode> Nodes;

            internal List<T> quadObjects = new List<T>();
            public ReadOnlyCollection<T> Objects;

            public Rect Bounds { get; internal set; }

            public bool HasChildNodes()
            {
                return _nodes[0] != null;
            }

            public QuadNode(Rect bounds)
            {
                Bounds = bounds;
                Nodes = new ReadOnlyCollection<QuadNode>(_nodes);
                Objects = new ReadOnlyCollection<T>(quadObjects);
            }

            public QuadNode(float x, float y, float width, float height)
                : this(new Rect(x, y, width, height))
            {

            }
        }
    }

    public enum Direction : int
    {
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }
}