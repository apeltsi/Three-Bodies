using System.Numerics;
using SolidCode.Atlas.ECS;

namespace ThreeBodies;

public class PathFollower : Component
{
    public Vector2[] Path = Array.Empty<Vector2>();

    private Transform tr;
    public void Start()
    {
        tr = GetComponent<Transform>();
    }

    private int frame = 0;
    
    public void Update()
    {
        if (frame < Path.Length)
        {
            tr.Position = Path[frame++];
            frame += 10;
        }
        else
            frame = 0;
    }
}