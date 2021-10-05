using Godot;
using System.Collections.Generic;

public class NodesGroup {
    private List<Node> _nodes = new List<Node>();

    public int Count() { return _nodes.Count; }

    public void Process() {
        _nodes.RemoveAll((n) => {
            return !Node.IsInstanceValid(n) || !n.IsInsideTree();
        });
    }

    public void Add(Node n) { _nodes.Add(n); }

    public List<Node> GetNodes() { return _nodes; }
}
