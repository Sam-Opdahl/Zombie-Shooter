using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame2 {
    interface IHasNeighbours<N> {
        IEnumerable<N> Neighbors { get; }
        List<int> NeighborWeight { get; }
    }

    class Path<Node> : IEnumerable<Node> {

        public Node LastStep { get; private set; }
        public Path<Node> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }

        private static Random rand = new Random();

        private Path(Node lastStep, Path<Node> previousSteps, double totalCost) {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(Node start) : this(start, null, 0) { }
        public Path<Node> AddStep(Node step, double stepCost) {
            return new Path<Node>(step, this, TotalCost + stepCost);
        }
        public IEnumerator<Node> GetEnumerator() {
            for (Path<Node> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        static public Path<Node> FindPath<Node>(
            Node start,
            Node destination,
            Func<Node, double> estimate)
            where Node : IHasNeighbours<Node> {
                var closed = new HashSet<Node>();
                var queue = new PriorityQueue<double, Path<Node>>();
                queue.Enqueue(0, new Path<Node>(start));
                while (!queue.IsEmpty) {
                    var path = queue.Dequeue();
                    if (closed.Contains(path.LastStep))
                        continue;
                    if (path.LastStep.Equals(destination))
                        return path;
                    closed.Add(path.LastStep);

                    int i = 0;
                    foreach (Node n in path.LastStep.Neighbors) {
                        double d = path.LastStep.NeighborWeight[i];
                        var newPath = path.AddStep(n, d);
                        queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                        i++;
                    }

                }
                return null;
        }
    }
}
