using System.Collections.Generic;
using System;
using Godot;

public class ObjectPool<T> where T : AbstractPoolValue, new() {
    public long idSeq = 1;
    public Dictionary<long, T> objects = new Dictionary<long, T> { };

    public void RemoveInactive() {
        var filtered = new Dictionary<long, T>();
        foreach (var o in objects) {
            if (!o.Value.deleted) {
                filtered.Add(o.Key, o.Value);
            }
        }
        objects = filtered;
    }

    public T New() {
        var id = idSeq;
        idSeq++;

        var o = new T();
        o.id = id;
        o.deleted = false;

        objects.Add(id, o);

        if (objects.Count == 1000) {
            GD.Print(typeof(T).FullName + " pool may be leaking");
        }

        return o;
    }

    public bool Contains(long id) {
        if (id == 0) {
            return false;
        }
        return objects.ContainsKey(id);
    }

    public T Get(long id) {
        if (id == 0) {
            throw new Exception("getting an object with id=0");
        }
        var obj = (T)objects[id];
        if (obj.deleted) {
            throw new Exception("getting deleted object");
        }
        return obj;
    }
}
