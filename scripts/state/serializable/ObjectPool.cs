using System.Collections.Generic;
using System;
using Godot;

public class ObjectPool<T> where T: AbstractPoolValue, new() {
    public List<T> objects = new List<T>{};

    public void RemoveInactive() {
        objects = objects.FindAll(x => x.active);
    }

    public T New() {
        var o = new T();
        o.id = objects.Count + 1;
        o.active = true;
        objects.Add(o);
        return o;
    }

    public bool Contains(int id) {
        if (id == 0) {
            return false;
        }
        int index = id - 1;
        return index >= 0 && index < objects.Count;
    }

    public T Get(int id) {
        if (id == 0) {
            throw new Exception("getting an object with id=0");
        }
        return (T)objects[id - 1];
    }
}
