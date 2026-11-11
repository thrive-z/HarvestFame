using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public sealed class CampaignSave
{
    private const int CurrentVersion = 1;
    private static string FileName => Path.Combine(Application.persistentDataPath, "harvest-fame.json");

    public int version = CurrentVersion;
    public int chapter;
    public int coins = 100;
    public Dictionary<string, int> progress;
    public float health;
    public float stamina;
    public float satiety;
    public bool sideComplete;
    public List<ItemData> toolbar;
    public List<ItemData> backpack;
    public List<ItemData> storage;
    public ItemData held;
    public int selectedSlot;
    public FarmPlot.Snapshot[] plots;
    public FarmStation.Snapshot[] stations;
    public bool[] defeated;
    public Position tractorPosition;
    public float tractorYaw;
    public Position pickupPosition;
    public float pickupYaw;
    public bool lastVehicleWasPickup;

    public static bool Exists => File.Exists(FileName);

    public static void Write(CampaignSave save)
    {
        var path = FileName;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonConvert.SerializeObject(save));
        if (File.Exists(path))
            File.Replace(temporary, path, null);
        else
            File.Move(temporary, path);
    }

    public static CampaignSave Read()
    {
        var save = JsonConvert.DeserializeObject<CampaignSave>(File.ReadAllText(FileName));
        if (save == null || save.version != CurrentVersion)
            throw new InvalidDataException("Unsupported save version");
        return save;
    }

    public sealed class Position
    {
        public float x;
        public float y;
        public float z;

        public Position() { }
        public Position(Vector3 position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }

        [JsonIgnore]
        public Vector3 Value => new Vector3(x, y, z);
    }
}
