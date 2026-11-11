using UnityEngine;

public sealed class CoopTrough : MonoBehaviour, IInteractable
{
    [SerializeField] private FarmStation coop;
    [SerializeField] private bool water;
    [SerializeField] private Transform contents;
    private float surfaceHeight;
    private Mesh feedSurface;
    private Texture2D feedTexture;

    public bool CanInteract() => coop != null;
    public string GetInteractTip() => water ? "F · 用花洒给水槽添水" : "F · 给食槽添草料";

    public void Interact(GameObject interactor)
    {
        if (water)
            coop.AddWater();
        else
            coop.AddFeed();
    }

    private void Awake()
    {
        var builtInSurface = transform.Find("SM_Prop_Trough_Water_01");
        if (contents == null || builtInSurface == null)
            return;

        var surface = builtInSurface.GetComponent<MeshFilter>().sharedMesh;
        contents.GetComponent<MeshFilter>().sharedMesh = surface;
        contents.localScale = builtInSurface.localScale;
        contents.localRotation = builtInSurface.localRotation;
        contents.localPosition = builtInSurface.localPosition;
        surfaceHeight = contents.localPosition.y;
        builtInSurface.gameObject.SetActive(false);
        if (!water)
            BuildChoppedFeed(surface);
    }

    private void BuildChoppedFeed(Mesh surface)
    {
        feedSurface = Instantiate(surface);
        var vertices = feedSurface.vertices;
        var bounds = feedSurface.bounds;
        var uv = new Vector2[vertices.Length];
        for (var i = 0; i < vertices.Length; i++)
            uv[i] = new Vector2(
                Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertices[i].x),
                Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertices[i].z));
        feedSurface.uv = uv;
        contents.GetComponent<MeshFilter>().sharedMesh = feedSurface;

        const int width = 192;
        const int height = 36;
        var pixels = new Color32[width * height];
        var baseColor = new Color32(101, 100, 43, 255);
        var shadow = new Color32(83, 79, 36, 255);
        var blades = new[]
        {
            new Color32(141, 151, 60, 255), new Color32(114, 132, 52, 255),
            new Color32(173, 158, 75, 255), new Color32(200, 180, 91, 255),
            new Color32(157, 142, 59, 255)
        };
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        var random = new System.Random(172);
        for (var i = 0; i < 430; i++)
        {
            var startX = random.Next(-8, width);
            var startY = random.Next(-5, height);
            var angle = (float)random.NextDouble() * Mathf.PI;
            var length = random.Next(4, 14);
            var stepX = Mathf.Cos(angle);
            var stepY = Mathf.Sin(angle);
            var color = blades[random.Next(blades.Length)];
            for (var j = 0; j < length; j++)
            {
                var x = startX + Mathf.RoundToInt(stepX * j);
                var y = startY + Mathf.RoundToInt(stepY * j);
                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;
                if (y > 0)
                    pixels[(y - 1) * width + x] = shadow;
                pixels[y * width + x] = color;
                if (j > 1 && j < length - 1 && y + 1 < height)
                    pixels[(y + 1) * width + x] = color;
            }
        }

        feedTexture = new Texture2D(width, height, TextureFormat.RGB24, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 4
        };
        feedTexture.SetPixels32(pixels);
        feedTexture.Apply(true, true);
        var appearance = new MaterialPropertyBlock();
        appearance.SetColor("_Color", Color.white);
        appearance.SetTexture("_MainTex", feedTexture);
        contents.GetComponent<Renderer>().SetPropertyBlock(appearance);
    }

    private void OnDestroy()
    {
        if (feedSurface != null)
            Destroy(feedSurface);
        if (feedTexture != null)
            Destroy(feedTexture);
    }

    private void LateUpdate()
    {
        if (contents == null || coop == null)
            return;

        var fill = water ? coop.WaterFill : coop.FeedFill;
        contents.gameObject.SetActive(fill > 0f);
        var position = contents.localPosition;
        position.y = surfaceHeight + Mathf.Lerp(0.006f, 0.084f, fill);
        contents.localPosition = position;
    }
}
