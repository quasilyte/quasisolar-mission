using System;

public class DebrisContainer {
    public int other = 0;
    public int wertu = 0;
    public int krigia = 0;
    public int zyth = 0;
    public int phaa = 0;
    public int draklid = 0;
    public int vespion = 0;
    public int rarilou = 0;

    public void Each(Action<int, Faction> f) {
        Update((x, faction) => {
            f(x, faction);
            return x;
        });
    }

    public void Update(Func<int, Faction, int> f) {
        other = f(other, Faction.Neutral);
        wertu = f(wertu, Faction.Wertu);
        krigia = f(krigia, Faction.Krigia);
        zyth = f(zyth, Faction.Zyth);
        phaa = f(phaa, Faction.Phaa);
        draklid = f(draklid, Faction.Draklid);
        vespion = f(vespion, Faction.Vespion);
        rarilou = f(rarilou, Faction.Rarilou);
    }

    public int Count() {
        int total = 0;
        Each((amount, kind) => {
            total += amount;
        });
        return total;
    }

    public int Count(Faction kind) {
        int result = other;
        Each((amount, kind2) => {
            if (kind2 == kind) {
                result = amount;
            }
        });
        return result;
    }

    public void Add(DebrisContainer x) {
        x.Each((amount, kind) => {
            Add(amount, kind);
        });
    }

    public void Add(int delta, Faction kind) {
        Update((x, kind2) => {
            if (kind != kind2) {
                return x;
            }
            return QMath.ClampMin(x + delta, 0);
        });
    }
}
