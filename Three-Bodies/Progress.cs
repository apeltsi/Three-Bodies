using System.Diagnostics;
using System.Numerics;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.UI;
using ThreeBodies.Utils;
using Debug = SolidCode.Atlas.Debug;

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
        if (Program.SimulationsPerformed == 0 && !Program.HasInitialized)
            tr.Text = "Loading Assets...";
        else if (GSim.Status != "")
            tr.Text = GSim.Status;
        else 
            tr.Text = "Progress: " +
                  (Math.Round((float)Program.SimulationsPerformed / (float)Program.TotalSimulations * 10000.0) / 100.0).ToString("0.00") +
                  "% - (" + Program.SimulationsPerformed + ")\n" + FormatSimCount(GSim.SimsPerSecond);
    }

    private string FormatSimCount(int simcount)
    {
        if(simcount > 1_000_000)
        {
            return (simcount / 1_000_000f).ToString("F2") + "m/s";
        }
        if (simcount > 10_000)
        {
            return simcount / 1000 + "k/s";
        }
        if (simcount > 1000)
        {
            return (simcount / 1000f).ToString("F2") + "k/s";
        }

        return simcount + "/s";
    }

    private int _index = 0;
    public void Tick()
    {
    }
}