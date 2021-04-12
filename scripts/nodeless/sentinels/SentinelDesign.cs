public class SentinelDesign {
    public string name = "";
    public string description = "";
    public string extraDescription = "";

    public float hp;

    public int sellingPrice = 0;

    public float attackCooldown = 4;
    public WeaponDesign weapon;

    public static SentinelDesign[] list = {
        new SentinelDesign{
            name = "Photon Fighter",
            description = "TODO",
            hp = 60,
            sellingPrice = 3500,
            weapon = PhotonBurstCannonWeapon.Design,
        },
    };
}
