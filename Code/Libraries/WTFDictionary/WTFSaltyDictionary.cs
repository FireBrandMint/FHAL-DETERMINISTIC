using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class WTFSaltyDictionary <K, V>
{
    public V this[K key]
    {
        get
        {
            var trueIdx1= Dict.GetTrueIndex(key);
            if(trueIdx1 == -1) throw new Exception("Doesn't exist.");

            var val = Dict.GetByTrueIndex(trueIdx1);
            V result;
            if(val.Value.Item1)
            {
                var n = Collisions[key].GetNode(key);
                if(n == null) throw new Exception("Doesn't exist.");
                result = n.Value.Value;
            }
            else
            {
                if(!key.Equals(val.Key)) throw new Exception("Doesn't exist.");
                result = val.Value.Item2;
            }

            return result;
        }
        set
        {
            var trueIdx1= Dict.GetTrueIndex(key);
            if(trueIdx1 == -1) throw new Exception("Doesn't exist.");

            var val = Dict.GetByTrueIndex(trueIdx1);

            if(val.Value.Item1)
            {
                var n = Collisions[key].GetNode(key);
                if(n == null) throw new Exception("Doesn't exist.");
                n.SetV(value);
            }
            else
            {
                if(!key.Equals(val.Key)) throw new Exception("Doesn't exist.");
                Dict.SetByTrueIndex(trueIdx1, new KeyValuePair<K, (bool, V)>(key, (false, value)));
            }
        }
    }

    private static Stack<Node> Cache = new Stack<Node>();

    WTFDictionary<K, KeyValuePair<K, (bool, V)>> Dict;

    WTFDictionary<K, Node> Collisions;

    public WTFSaltyDictionary ()
    {
        Dict = new WTFDictionary<K, KeyValuePair<K, (bool, V)>>(50);
        Collisions = new WTFDictionary<K, Node>(50);
    }

    public WTFSaltyDictionary (int capacity, int collisionTableSize)
    {
        Dict = new WTFDictionary<K, KeyValuePair<K, (bool, V)>>(capacity);
        Collisions = new WTFDictionary<K, Node>(collisionTableSize);
    }

    public void Add(K key, V value)
    {
        int trueInd = Dict.GetTrueIndex(key);
        if(trueInd == -1)
            Dict.Add(key, new KeyValuePair<K, (bool, V)>(key, (false, value)));
        else
        {
            var found = Dict.GetByTrueIndex(trueInd);
            if(found.Value.Item1)
            {
                if(!Collisions[key].AddNode(new KeyValuePair<K, V>(key, value)))
                    throw new Exception("Tried to add an already existing element");

            }
            else
            {
                Dict.SetByTrueIndex(trueInd, new KeyValuePair<K, (bool, V)>(key, (true, default(V))));
                Node first;
                if(!Cache.TryPop(out first)) first = new Node();
                first.Value = new KeyValuePair<K, V>(found.Key, found.Value.Item2);
                first.AddNode(new KeyValuePair<K, V>(key, value));

                Collisions.Add(key, first);
            }
        }
    }

    public void Remove(K key)
    {
        var initial = Dict.GetTrueIndex(key);

        if(Dict.GetByTrueIndex(initial).Value.Item1)
        {
            var nodeIdx = Collisions.GetTrueIndex(key);
            var node = Collisions.GetByTrueIndex(nodeIdx);
            if(node.Next == null)
            {
                if(!key.Equals(node.Value.Key)) throw new Exception($"Element '{key.ToString()}' doesn't exist, aborting.");

                Collisions.RemoveByTrueIndex(nodeIdx);
            }
            else
            {
                if(key.Equals(node.Value.Key))
                {
                    Collisions.SetByTrueIndex(nodeIdx, node.Next);
                    node.Clear();
                    Cache.Push(node);
                    return;
                }

                var res2 = node.RemoveNode(key);
                if(res2 == null) throw new Exception($"Element '{key.ToString()}' doesn't exist, aborting.");
                res2.Clear();
                Cache.Push(res2);
            }
        }
        else
            Dict.RemoveByTrueIndex(initial);
            
    }

    private class Node
    {

        public KeyValuePair<K, V> Value;

        public Node Next;

        public Node() {}

        public Node (KeyValuePair<K, V> val)
        {
            Value = val;
            Next = null;
        }

        public void Clear()
        {
            Value = new KeyValuePair<K, V>(default(K), default(V));
            Next = null;
        }

        public bool AddNode(KeyValuePair<K, V> val)
        {
            Node subject = this;
            while(subject.Next != null)
            {
                if(val.Key.Equals(subject.Value.Key)) return false;
                
                subject = subject.Next;
            }
            Node nn;
            if(!Cache.TryPop(out nn)) nn = new Node();
            nn.Value = val;
            subject.Next = nn;

            return true;
        }

        public Node GetNode(K key)
        {
            if(Value.Key.Equals(key)) return this;

            Node subject = this;
            Node last = null;
            while(subject.Next != null)
            {
                if(key.Equals(subject.Value.Key))
                    break;
                
                subject = subject.Next;
                last = subject;
            }

            if(key.Equals(subject.Value.Key))
            {
                return subject;
            }

            return null;
        }

        public void SetV(V v)
        {
            Value = new KeyValuePair<K, V>(Value.Key, v);
        }

        public Node RemoveNode(K key)
        {
            if(Value.Key.Equals(key)) return this;

            Node subject = this;
            Node last = null;
            while(subject.Next != null)
            {
                if(key.Equals(subject.Value.Key))
                    break;
                
                subject = subject.Next;
                last = subject;
            }

            if(key.Equals(subject.Value.Key))
            {
                last.Next = subject.Next;
                return subject;
            }

            return null;
        }
    }
}
