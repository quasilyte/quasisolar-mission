using Godot;

public interface IMapViewContext {
    void AddPlayerUnitMember(Vessel v);
    void RemovePlayerUnitMember(int index);
    void EnterSystem(StarSystem sys);
    void UpdateUI();

    SceneTree GetTree();
    void AddUIChild(Node n);

    void CreateNotification(Vector2 pos, string text);
    void CreateBadNotification(Vector2 pos, string text);
}
