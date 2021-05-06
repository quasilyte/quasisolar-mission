using System;
using System.Collections.Generic;

public class SentinelDesign: IItem {
    public enum Kind {
        Attack,
        Defense,
    }

    public string name = "";
    public string description = "";
    public string extraDescription = "";

    public bool researchRequired = false;

    public Kind kind;

    public float hp;

    public int sellingPrice = 0;

    public float attackCooldown;
    public WeaponDesign weapon;

    public ItemKind GetItemKind() { return ItemKind.Sentinel; }

    public string RenderHelp() {
        if (name == "Empty") {
            // A special case.
            return "An empty sentinel slot";
        }

        var parts = new List<string>();
        parts.Add(name + " (" + ItemInfo.BuyingPrice(this) + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        parts.Add("Max hp: " + hp);
        if (kind == Kind.Attack) {
            parts.Add("Type: attack assistance");
        } else if (kind == Kind.Defense) {
            parts.Add("Type: defense assistance");
        }
        return string.Join("\n", parts);
    }

    public static SentinelDesign Find(string name) {
        if (designByName.ContainsKey(name)) {
            return designByName[name];
        }
        throw new Exception($"can't find {name} sentinel design");
    }

    private static Dictionary<string, SentinelDesign> designByName;

    public static void InitLists() {
        designByName = new Dictionary<string, SentinelDesign>();
        foreach (var d in list) {
            designByName.Add(d.name, d);
        }
    }

    public static SentinelDesign[] list = {
        new SentinelDesign{
            name = "Empty",
        },

        new SentinelDesign{
            name = "Ion Fighter",
            description = "TODO",
            kind = Kind.Attack,
            hp = 35,
            sellingPrice = 3000,
            weapon = IonCannonWeapon.Design,
            attackCooldown = 2,
        },

        new SentinelDesign{
            researchRequired = true,
            name = "Photon Fighter",
            description = "TODO",
            kind = Kind.Attack,
            hp = 60,
            sellingPrice = 3500,
            weapon = PhotonBurstCannonWeapon.Design,
            attackCooldown = 1.5f,
        },

        new SentinelDesign{
            researchRequired = true,
            name = "Point-Defense Guard",
            description = "TODO",
            kind = Kind.Defense,
            hp = 45,
            sellingPrice = 4200,
            weapon = PointDefenseLaserWeapon.Design,
            attackCooldown = 2,
        },

        new SentinelDesign{
            researchRequired = true,
            name = "Restructuring Guard",
            description = "TODO",
            kind = Kind.Defense,
            hp = 50,
            sellingPrice = 7500,
            weapon = RestructuringRayWeapon.Design,
            attackCooldown = 7,
        },
    };
}
