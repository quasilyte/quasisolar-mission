using Godot;
using System;

public class ItemInfo {
    public ItemKind kind;
    public string itemName;

    public IItem ToItem() {
        if (kind == ItemKind.Weapon || kind == ItemKind.SpecialWeapon) {
            return WeaponDesign.Find(itemName);
        }
        if (kind == ItemKind.EnergySource) {
            return EnergySource.Find(itemName);
        }
        if (kind == ItemKind.Artifact) {
            return ArtifactDesign.Find(itemName);
        }
        if (kind == ItemKind.Shield) {
            return ShieldDesign.Find(itemName);
        }
        if (kind == ItemKind.Sentinel) {
            return SentinelDesign.Find(itemName);
        }
        if (kind == ItemKind.VesselDesign) {
            return VesselDesign.Find(itemName);
        }

        throw new Exception("unexpected item type: " + kind.ToString());
    }

    public static ItemInfo Of(IItem item) {
        if (item == null) {
            return null;
        }
        var kind = item.GetItemKind();
        var name = Name(item);
        return new ItemInfo{kind = kind, itemName = name};
    }

    public static string RenderHelp(IItem item) {
        if (item is WeaponDesign weapon) {
            return weapon.RenderHelp();
        } else if (item is EnergySource energySource) {
            return energySource.RenderHelp();
        } else if (item is ArtifactDesign artifact) {
            return artifact.RenderHelp();
        } else if (item is ShieldDesign shield) {
            return shield.RenderHelp();
        } else if (item is SentinelDesign sentinel) {
            return sentinel.RenderHelp();
        } else if (item is Vessel vessel) {
            return RenderHelp(vessel.Design());
        } else if (item is VesselDesign vesselDesign) {
            return vesselDesign.RenderHelp();
        }
        return "unknown_item";
    }

    public static string Name(IItem item) {
        if (item is WeaponDesign weapon) {
            return weapon.name;
        } else if (item is EnergySource energySource) {
            return energySource.name;
        } else if (item is ArtifactDesign artifact) {
            return artifact.name;
        } else if (item is ShieldDesign shield) {
            return shield.name;
        } else if (item is SentinelDesign sentinel) {
            return sentinel.name;
        } else if (item is Vessel vessel) {
            return Name(vessel.Design());
        } else if (item is VesselDesign vesselDesign) {
            return vesselDesign.affiliation + " " + vesselDesign.name;
        }
        return "unknown_item";
    }

    public static Texture Texture(IItem item) {
        var name = Name(item);
        name = name.Replace(' ', '_');
        name = name.Replace('-', '_');
        if (item is VesselDesign || item is Vessel) {
            return GD.Load<Texture>($"res://images/vessel/{name}.png");
        }
        return GD.Load<Texture>($"res://images/items/{name}.png");
    }

    public static int BuyingPrice(IItem item) {
        return ItemPrice(item, false);
    }

    public static int SellingPrice(IItem item) {
        return ItemPrice(item, true);
    }

    private static int ItemPrice(IItem item, bool selling) {
        if (item is WeaponDesign weapon) {
            if (!selling && weapon == NeedleGunWeapon.Design && RpgGameState.instance.technologiesResearched.Contains("Gauss Production")) {
                return (int)(weapon.sellingPrice * 0.8);
            }
            return weapon.sellingPrice;
        } else if (item is EnergySource energySource) {
            return energySource.sellingPrice;
        } else if (item is ArtifactDesign artifact) {
            return artifact.sellingPrice;
        } else if (item is ShieldDesign shield) {
            return shield.sellingPrice;
        } else if (item is SentinelDesign sentinel) {
            return sentinel.sellingPrice;
        } else if (item is Vessel vessel) {
            return SellingPrice(vessel.Design());
        } else if (item is VesselDesign vesselDesign) {
            return vesselDesign.sellingPrice;
        }

        throw new Exception("unexpected item type: " + item.GetType().Name);
    }

    public static int MinStarBaseLevel(IItem item) {
        if (item is WeaponDesign weapon) {
            return WeaponMinStarBaseLevel(weapon);
        } else if (item is ShieldDesign shield) {
            return ShieldMinStarBaseLevel(shield);
        } else if (item is Vessel vessel) {
            return MinStarBaseLevel(vessel.Design());
        } else if (item is VesselDesign vesselDesign) {
            return VesselMinStarBaseLevel(vesselDesign);
        }
        return 1;
    }

    public static int VesselMinStarBaseLevel(VesselDesign vessel) {
        switch (vessel.level) {
        case 3:
        case 4:
        case 5:
        case 6:
        case 7:
            return vessel.level - 2;
        case 8:
        case 9:
        case 10:
            return 100; // Impossible
        default:
            return 1;
        }
    }

    public static int ShieldMinStarBaseLevel(ShieldDesign shield) {
        if (shield.level == 1) {
            return 1;
        }
        if (shield.level == 2) {
            return 3;
        }
        if (shield.level == 3) {
            return 5;
        }
        throw new Exception($"illegal {shield.name} shield level {shield.level}");
    }

    private static int WeaponMinStarBaseLevel(WeaponDesign weapon) {
        if (weapon.level == 1) {
            return 1;
        }
        if (weapon.level == 2) {
            return weapon.isSpecial ? 3 : 2;
        }
        if (weapon.level == 3) {
            return weapon.isSpecial ? 5 : 4;
        }
        throw new Exception($"illegal {weapon.name} weapon level: {weapon.level}");

    }
}
