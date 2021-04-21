using System.Collections.Generic;
using System;

public class WeaponDesign : IItem {
    public static WeaponDesign[] list;
    public static WeaponDesign[] specialList;
    public static Dictionary<string, WeaponDesign> weaponByName;

    public string name = "";
    public string description = "";
    public string extraDescription = "";
    public string targeting = "";
    public string special = "";
    public string special2 = "";
    public int level;

    public int sellingPrice = 0;
    public List<string> technologiesNeeded = new List<string>();

    public float cooldown = 0;
    public float energyCost = 0;
    public float range = 0;
    public float minRange = 0;
    public DamageKind damageKind;
    public float damage = 0;
    public float energyDamage = 0;

    public bool isSpecial = false;

    public bool ignoresAsteroids = false;

    public float botHintRange = 0;
    public float botHintEffectiveAngle = 0;
    public float botHintSnipe = 0;
    public float botHintScatter = 1;

    public float projectileSpeed = 0;

    public ItemKind GetItemKind() {
        return isSpecial ? ItemKind.SpecialWeapon : ItemKind.Weapon;
    }

    public static WeaponDesign Find(string name) {
        return weaponByName[name];
    }

    public string RenderHelp() {
        if (name == "Empty") {
            // A special case.
            return "An empty weapon slot";
        }

        var parts = new List<string>();
        parts.Add(name + " (" + ItemInfo.BuyingPrice(this) + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        if (damageKind != DamageKind.None) {
            parts.Add("Damage type: " + damageKind.ToString().ToLower());
        }
        if (damage > 0) {
            parts.Add("Damage: " + damage.ToString());
        }
        if (energyDamage != 0) {
            parts.Add("Energy damage: " + energyDamage.ToString());
        }
        if (minRange != 0) {
            parts.Add("Min range: " + minRange.ToString());
        }
        parts.Add("Range: " + range.ToString());
        parts.Add("Rate of fire: " + rateOfFireText());
        if (energyCost != 0) {
            parts.Add("Energy cost: " + energyCost.ToString());
        }
        parts.Add("Targeting: " + targeting);
        if (special != "") {
            parts.Add("Special: " + special);
        }
        if (special2 != "") {
            parts.Add("Special: " + special2);
        }
        return string.Join("\n", parts);
    }

    private string rateOfFireText() {
        if (cooldown <= 0.3f) {
            return "very fast";
        }
        if (cooldown <= 0.6f) {
            return "fast";
        }
        if (cooldown <= 1) {
            return "normal";
        }
        if (cooldown <= 1.5f) {
            return "slow";
        }
        if (cooldown <= 2) {
            return "very slow";
        }
        return "very, very slow";
    }

    public static void InitLists() {
        list = new WeaponDesign[]{
            SpreadGunWeapon.Design,
            ZapWeapon.Design,
            NeedleGunWeapon.Design,
            ScytheWeapon.Design,
            PhotonBurstCannonWeapon.Design,
            IonCannonWeapon.Design,
            HellfireWeapon.Design,
            PulseLaserWeapon.Design,
            AssaultLaserWeapon.Design,
            RocketLauncherWeapon.Design,
            HurricaneWeapon.Design,
            ShieldBreakerWeapon.Design,
            CutterWeapon.Design,
            GreatScytheWeapon.Design,
            TwinPhotonBurstCannonWeapon.Design,
            PointDefenseLaserWeapon.Design,
            StingerWeapon.Design,
            DiskThrowerWeapon.Design,
            PlasmaEmitterWeapon.Design,
            CrystalCannonWeapon.Design,
            StormbringerWeapon.Design,
            LancerWeapon.Design,
        };
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
        weaponByName = new Dictionary<string, WeaponDesign>();
        foreach (var w in list) {
            weaponByName.Add(w.name, w);
        }

        specialList = new WeaponDesign[]{
            HarpoonWeapon.Design,
            ReaperCannonWeapon.Design,
            TorpedoLauncherWeapon.Design,
            PhotonBeamWeapon.Design,
            MortarWeapon.Design,
            RestructuringRayWeapon.Design,
            WarpDeviceWeapon.Design,
            DisruptorWeapon.Design,
        };
        Array.Sort(specialList, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
        foreach (var w in specialList) {
            weaponByName.Add(w.name, w);
        }

        weaponByName.Add("Empty", EmptyWeapon.Design);
    }
}
