namespace ThreeBodies;

using System.Numerics;
using SolidCode.Atlas;
using SolidCode.Atlas.Components;
using SolidCode.Atlas.ECS;
using SolidCode.Atlas.Input;
using SolidCode.Atlas.Mathematics;
using SolidCode.Atlas.Rendering;

public class CameraController : Component
{
    Camera c;
    Transform ct;
    public void Start()
    {
        c = Entity.GetComponent<Camera>();
        ct = Entity.GetComponent<Transform>();
    }

    public static Entity GetCamera()
    {
        Entity e = new Entity("Camera", null, new Vector2(15f, 15f));
        e.AddComponent<Camera>();
        e.AddComponent<CameraController>();
        return e;
    }
    
    bool firstFrame = true;
    public double zoom = 15f;
    float zoomSpeed = 0.1f;
    public void Update()
    {
        if (Input.WheelDelta != 0 || firstFrame)
        {
            zoom = zoom + Input.WheelDelta * zoomSpeed * zoom;
            if (zoom == 0)
            {
                zoom = 0.01f;
            }
            firstFrame = false;
        }
        double speedModifier = 3f;
        if (Input.GetKey(Veldrid.Key.ShiftLeft) || Input.GetKey(Veldrid.Key.ShiftRight))
        {
            speedModifier = 9f;
        }
        if (Input.GetKey(Veldrid.Key.W))
        {
            ct.Position += new System.Numerics.Vector2(0f, (float)(zoom * Time.deltaTime * speedModifier));
        }
        if (Input.GetKey(Veldrid.Key.S))
        {
            ct.Position -= new System.Numerics.Vector2(0f, (float)(zoom * Time.deltaTime * speedModifier));
        }
        if (Input.GetKey(Veldrid.Key.A))
        {
            ct.Position -= new System.Numerics.Vector2((float)(zoom * Time.deltaTime * speedModifier), 0f);
        }
        if (Input.GetKey(Veldrid.Key.D))
        {
            ct.Position += new System.Numerics.Vector2((float)(zoom * Time.deltaTime * speedModifier), 0f);
        }
        ct.Scale = AMath.Lerp(ct.Scale, new System.Numerics.Vector2((float)zoom, (float)zoom), 0.1f);
    }
}