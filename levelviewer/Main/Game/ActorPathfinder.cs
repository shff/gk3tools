using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class ActorPathfinder
    {
        private byte[] _weightMap;
        private int _width, _height;
        private Math.Vector2 _scaledSize;
        private Math.Vector2 _offset;

        private int[] _whichList;
        private int[] _parents;
        private float[] _fCost;
        private float[] _gCost;
        private float[] _hCost;
        private int _whichListCounter;

        public ActorPathfinder(string pathMapName, Math.Vector2 size, Math.Vector2 offset)
        {
            if (pathMapName == null)
                throw new ArgumentNullException("pathMapName");

            _scaledSize = size;
            _offset = offset;

            if (pathMapName.EndsWith(".BMP", StringComparison.OrdinalIgnoreCase) == false)
                pathMapName += ".BMP";

            Graphics.BitmapSurface map;
            using (System.IO.Stream stream = FileSystem.Open(pathMapName))
            {
                // apparently the color doesn't matter, the weights are
                // stored as indices. So we don't want to convert!
                map = new Gk3Main.Graphics.BitmapSurface(stream, false);
            }

            // temporary
            //Graphics.Utils.WriteTga("out.tga", map.Pixels, map.Width, map.Height);

            // read the pixels and convert them into weights
            _weightMap = new byte[map.Width * map.Height];
            _width = map.Width;
            _height = map.Height;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Graphics.Color color = map.ReadColorAt(x, y);

                    _weightMap[y * _width + x] = color.R;
                }
            }

            // create the working arrays
            _whichList = new int[_width * _height];
            _parents = new int[_width * _height];
            _fCost = new float[_width * _height];
            _gCost = new float[_width * _height];
            _hCost = new float[_width * _height];
        }

        public Math.Vector2 ScaledSize
        {
            get { return _scaledSize; }
        }

        public Math.Vector2 Offset
        {
            get { return _offset; }
        }

        private class MapNode : IComparable<MapNode>
        {
            public int X, Y;
            public ActorPathfinder Parent;

            public MapNode(ActorPathfinder parent, int x, int y)
            {
                X = x;
                Y = y;
                Parent = parent;
            }

            public int CompareTo(MapNode node)
            {
                float fcost1 = Parent._fCost[Y * Parent._width + X];
                float fcost2 = Parent._fCost[node.Y * Parent._width + node.X];

                return fcost1.CompareTo(fcost2);
            }
        }

        public Math.Vector2[] CalculatePath(Math.Vector2 start, Math.Vector2 end)
        {
            /*// convert the coordinates into local map coordinates
            int goalx = (int)((start.X - _offset.X) / _scaledSize.X * _width);
            int goaly = (int)((start.Y - _offset.Y) / _scaledSize.Y * _height);
            */

            int goalx = (int)start.X;
            int goaly = (int)start.Y;

            if (goalx < 0 || goaly < 0 ||
                goalx >= _width || goaly >= _height)
                return null;
            /*
            int startx = (int)((end.X - _offset.X) / _scaledSize.X * _width);
            int starty = (int)((end.Y - _offset.Y) / _scaledSize.Y * _height);
            */
            int startx = (int)end.X;
            int starty = (int)end.Y;

            if (startx < 0 || starty < 0 ||
                startx >= _width || starty >= _height)
                return null;

            _whichListCounter += 2;

            PriorityQueue<MapNode> open = new PriorityQueue<MapNode>();
            addToOpen(new MapNode(this, startx, starty), open);
            _parents[starty * _width + startx] = 0;

            while(true)
            {
                MapNode lowest = removeFirstFromOpen(open);
                addToClosed(lowest);

                if (lowest.X == goalx && lowest.Y == goaly)
                {
                    // we just found the path!
                    return generatePath(goalx, goaly);
                }

                // check north
                check(lowest.X, lowest.Y, lowest.X, lowest.Y - 1, goalx, goaly, open);

                // check south
                check(lowest.X, lowest.Y, lowest.X, lowest.Y + 1, goalx, goaly, open);

                // check east
                check(lowest.X, lowest.Y, lowest.X + 1, lowest.Y, goalx, goaly, open);

                // check west
                check(lowest.X, lowest.Y, lowest.X - 1, lowest.Y, goalx, goaly, open);

                if (open.Count == 0)
                {
                    // we failed to find a path!
                    return null;
                }
            }
        }

        public void PrintPathToLogger(Math.Vector2[] path)
        {
            foreach (Math.Vector2 node in path)
            {
                Logger.WriteInfo(node.ToString());
            }
        }

        private void check(int parentX, int parentY, int x, int y, int goalx, int goaly, PriorityQueue<MapNode> open)
        {
            if (y >= 0 && x >= 0)
            {
                float weight = _weightMap[y * _width + x];
                if (weight < 255)
                {
                    int whichList = _whichList[y * _width + x];

                    if (whichList == _whichListCounter)
                    {
                        // it's on the open list
                        float gCost = _gCost[parentY * _width + parentX] + weight;
                        if (gCost < _gCost[y * _width + x])
                        {
                            _parents[y * _width + x] = parentY * _width + parentX;
                            _gCost[parentY * _width + parentX] = gCost;
                            _fCost[parentY * _width + parentX] = gCost + _hCost[parentY * _width + parentX];
                        }
                    }
                    else if (whichList < _whichListCounter)
                    {
                        // not on the open list (or the closed)
                        _hCost[y * _width + x] = calcHCost(x, y, goalx, goaly);
                        _gCost[y * _width + x] = _gCost[parentY * _width + parentX] + weight;
                        _fCost[y * _width + x] = _hCost[y * _width + x] + _gCost[y * _width + x];

                        MapNode node = new MapNode(this, x, y);
                        _parents[y * _width + x] = parentY * _width + parentX;

                        addToOpen(node, open);
                    }
                }
            }
        }

        private Math.Vector2[] generatePath(int startX, int startY)
        {
            List<Math.Vector2> path = new List<Math.Vector2>();

            int currentX = startX;
            int currentY = startY;

            path.Add(new Gk3Main.Math.Vector2(startX, startY));

            while (true)
            {
                int parent = _parents[currentY * _width + currentX];
                if (parent == 0)
                    break;

                currentX = parent % _width;
                currentY = parent / _width;

                path.Add(new Math.Vector2(currentX, currentY));
            }

            return path.ToArray();
        }

        private void addToOpen(MapNode node, PriorityQueue<MapNode> open)
        {
            int whichList = _whichList[node.Y * _width + node.X];

            if (whichList == _whichListCounter)
                throw new InvalidOperationException("The node is already in the open list");
            else if (whichList == _whichListCounter + 1)
                throw new InvalidOperationException("Nodes should never move from the closed list to the open list");

            open.Enqueue(node);
            _whichList[node.Y * _width + node.X] = _whichListCounter;
        }

        private MapNode removeFirstFromOpen(PriorityQueue<MapNode> open)
        {
            MapNode result = open.Dequeue();
            _whichList[result.Y * _width + result.X] = _whichListCounter - 1;

            return result;
        }

        private void addToClosed(MapNode node)
        {
            int whichList = _whichList[node.Y * _width + node.X];

            if (whichList == _whichListCounter)
                throw new InvalidOperationException("Node must be removed from the open list before adding to the closed list");
            if (whichList == _whichListCounter + 1)
                throw new InvalidOperationException("Node is already on the closed list");
            _whichList[node.Y * _width + node.X] = _whichListCounter + 1;
        }

        private float calcHCost(int startx, int starty, int goalx, int goaly)
        {
            return (float)System.Math.Sqrt((startx - goalx) * (startx - goalx) + (starty - goaly) * (starty - goaly));
        }
    }

    //====================================================
    //| Downloaded From                                  |
    //| Visual C# Kicks - http://www.vcskicks.com/       |
    //| License - http://www.vcskicks.com/license.html   |
    //====================================================
    /// <summary>
    /// Priority Queue data structure
    /// </summary>
    class PriorityQueue<T>
        where T : IComparable<T>
    {
        protected List<T> storedValues;

        public PriorityQueue()
        {
            //Initialize the array that will hold the values
            storedValues = new List<T>();

            //Fill the first cell in the array with an empty value
            storedValues.Add(default(T));
        }

        /// <summary>
        /// Gets the number of values stored within the Priority Queue
        /// </summary>
        public virtual int Count
        {
            get { return storedValues.Count - 1; }
        }

        /// <summary>
        /// Returns the value at the head of the Priority Queue without removing it.
        /// </summary>
        public virtual T Peek()
        {
            if (this.Count == 0)
                return default(T); //Priority Queue empty
            else
                return storedValues[1]; //head of the queue
        }

        /// <summary>
        /// Adds a value to the Priority Queue
        /// </summary>
        public virtual void Enqueue(T value)
        {
            //Add the value to the internal array
            storedValues.Add(value);

            //Bubble up to preserve the heap property,
            //starting at the inserted value
            this.BubbleUp(storedValues.Count - 1);
        }

        /// <summary>
        /// Returns the minimum value inside the Priority Queue
        /// </summary>
        public virtual T Dequeue()
        {
            if (this.Count == 0)
                return default(T); //queue is empty
            else
            {
                //The smallest value in the Priority Queue is the first item in the array
                T minValue = this.storedValues[1];

                //If there's more than one item, replace the first item in the array with the last one
                if (this.storedValues.Count > 2)
                {
                    T lastValue = this.storedValues[storedValues.Count - 1];

                    //Move last node to the head
                    this.storedValues.RemoveAt(storedValues.Count - 1);
                    this.storedValues[1] = lastValue;

                    //Bubble down
                    this.BubbleDown(1);
                }
                else
                {
                    //Remove the only value stored in the queue
                    storedValues.RemoveAt(1);
                }

                return minValue;
            }
        }

        /// <summary>
        /// Restores the heap-order property between child and parent values going up towards the head
        /// </summary>
        protected virtual void BubbleUp(int startCell)
        {
            int cell = startCell;

            //Bubble up as long as the parent is greater
            while (this.IsParentBigger(cell))
            {
                //Get values of parent and child
                T parentValue = this.storedValues[cell / 2];
                T childValue = this.storedValues[cell];

                //Swap the values
                this.storedValues[cell / 2] = childValue;
                this.storedValues[cell] = parentValue;

                cell /= 2; //go up parents
            }
        }

        /// <summary>
        /// Restores the heap-order property between child and parent values going down towards the bottom
        /// </summary>
        protected virtual void BubbleDown(int startCell)
        {
            int cell = startCell;

            //Bubble down as long as either child is smaller
            while (this.IsLeftChildSmaller(cell) || this.IsRightChildSmaller(cell))
            {
                int child = this.CompareChild(cell);

                if (child == -1) //Left Child
                {
                    //Swap values
                    T parentValue = storedValues[cell];
                    T leftChildValue = storedValues[2 * cell];

                    storedValues[cell] = leftChildValue;
                    storedValues[2 * cell] = parentValue;

                    cell = 2 * cell; //move down to left child
                }
                else if (child == 1) //Right Child
                {
                    //Swap values
                    T parentValue = storedValues[cell];
                    T rightChildValue = storedValues[2 * cell + 1];

                    storedValues[cell] = rightChildValue;
                    storedValues[2 * cell + 1] = parentValue;

                    cell = 2 * cell + 1; //move down to right child
                }
            }
        }

        /// <summary>
        /// Returns if the value of a parent is greater than its child
        /// </summary>
        protected virtual bool IsParentBigger(int childCell)
        {
            if (childCell == 1)
                return false; //top of heap, no parent
            else
                return storedValues[childCell / 2].CompareTo(storedValues[childCell]) > 0;
                //return storedNodes[childCell / 2].Key > storedNodes[childCell].Key;
        }

        /// <summary>
        /// Returns whether the left child cell is smaller than the parent cell.
        /// Returns false if a left child does not exist.
        /// </summary>
        protected virtual bool IsLeftChildSmaller(int parentCell)
        {
            if (2 * parentCell >= storedValues.Count)
                return false; //out of bounds
            else
                return storedValues[2 * parentCell].CompareTo(storedValues[parentCell]) < 0;
                //return storedNodes[2 * parentCell].Key < storedNodes[parentCell].Key;
        }

        /// <summary>
        /// Returns whether the right child cell is smaller than the parent cell.
        /// Returns false if a right child does not exist.
        /// </summary>
        protected virtual bool IsRightChildSmaller(int parentCell)
        {
            if (2 * parentCell + 1 >= storedValues.Count)
                return false; //out of bounds
            else
                return storedValues[2 * parentCell + 1].CompareTo(storedValues[parentCell]) < 0;
                //return storedNodes[2 * parentCell + 1].Key < storedNodes[parentCell].Key;
        }

        /// <summary>
        /// Compares the children cells of a parent cell. -1 indicates the left child is the smaller of the two,
        /// 1 indicates the right child is the smaller of the two, 0 inidicates that neither child is smaller than the parent.
        /// </summary>
        protected virtual int CompareChild(int parentCell)
        {
            bool leftChildSmaller = this.IsLeftChildSmaller(parentCell);
            bool rightChildSmaller = this.IsRightChildSmaller(parentCell);

            if (leftChildSmaller || rightChildSmaller)
            {
                if (leftChildSmaller && rightChildSmaller)
                {
                    //Figure out which of the two is smaller
                    int leftChild = 2 * parentCell;
                    int rightChild = 2 * parentCell + 1;

                    T leftValue = this.storedValues[leftChild];
                    T rightValue = this.storedValues[rightChild];

                    //Compare the values of the children
                    if (leftValue.CompareTo(rightValue) <= 0)
                        return -1; //left child is smaller
                    else
                        return 1; //right child is smaller
                }
                else if (leftChildSmaller)
                    return -1; //left child is smaller
                else
                    return 1; //right child smaller
            }
            else
                return 0; //both children are bigger or don't exist
        }

    }
}
