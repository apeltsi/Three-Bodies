using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.UI;

namespace ThreeBodies;

public class Progress : Component
{
    private TextRenderer tr;

    public void Start()
    {
        tr = GetComponent<TextRenderer>()!;
    }

    public static Entity GetText()
    {
        Entity e = new Entity("Progress", null, new Vector2(0.25f, 0.25f));
        e.AddComponent<TextRenderer>();
        e.AddComponent<Progress>();
        return e;
    }

    private int frames = 0;
    public void Update()
    {
        tr.Text = "Progress: " +
                  (Math.Round((float)Program.SimulationsPerformed / (float)Program.TotalSimulations * 10000.0) / 100.0).ToString("0.00") +
                  "% - (" + Program.SimulationsPerformed + ")";
    }
}