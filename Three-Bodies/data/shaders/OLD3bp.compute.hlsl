#define TIME_STEP 0.2
#define TICKS 20000
#define G 6.67408e-11
#define MASS 50

[[vk::binding(0, 0)]]
RWStructuredBuffer<float2> PrimaryBuffer;
struct Body
{
    double2 position;
    double2 velocity;
    double2 acceleration;
};

[[vk::binding(1, 0)]]
StructuredBuffer<float> RandomBuffer;

[[vk::binding(2, 0)]]
cbuffer BodyData
{
    double ax;
    double ay;
    double bx;
    double by;
    double cx;
    double cy;
}

struct ArrData
{
    double2 data[3];
};

ArrData CalculateAccelerations(Body bodies[3])
{
    double2 accelerations[3] = {double2(0, 0), double2(0, 0), double2(0, 0)};
    [unroll(3)]
    for (int i = 0; i < 3; i++)
    {
        double2 netAcceleration = double2(0, 0);
        [unroll(3)]
        for (int j = 0; j < 3; j++)
        {
            if (i != j)
            {
                double2 r = bodies[j].position - bodies[i].position;
                double distance = sqrt(r.x * r.x + r.y * r.y);
                double2 direction = r / distance;
                double accelerationMagnitude = G * MASS / (distance * distance);
                netAcceleration += direction * accelerationMagnitude;
            }
        }
        accelerations[i] = netAcceleration;
    }
    ArrData data;
    data.data = accelerations;
    return data;
}

[numthreads(1,1,1)]
void main(int3 id : SV_DispatchThreadID)
{
    const int access = id.x * 3;
    Body bodies[3];
    bodies[0].position = double2(ax, ay) + double2(RandomBuffer[(id.x * 99) % 10000], RandomBuffer[(id.x * 99 + 39) % 10000]) * 0.01;
    bodies[1].position = double2(bx, by) + double2(RandomBuffer[(id.x * 99 + 6) % 10000], RandomBuffer[(id.x * 99 + 3) % 10000]) * 0.01;
    bodies[2].position = double2(cx, cy) + double2(RandomBuffer[(id.x * 99 + 92) % 10000], RandomBuffer[(id.x * 99 + 5) % 10000]) * 0.01;
    for(int t = 0; t < TICKS; t++)
    {
        double2 initialAccelerations[3] = CalculateAccelerations(bodies).data;

        // Vi räknar ut mellanstegen
        Body midbodies[3];
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            midbodies[i].position = bodies[i].position + bodies[i].velocity * TIME_STEP * 0.5;
            midbodies[i].velocity = bodies[i].velocity + initialAccelerations[i] * TIME_STEP * 0.5;
        }

        // Vi räknar ut mellan accelerationerna
        const double2 mid_accelerations[3] = CalculateAccelerations(midbodies).data;

        // Slutligen kan vi uppdatera positionerna och hastigheterna
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            bodies[i].position = bodies[i].position + midbodies[i].velocity * TIME_STEP;
            bodies[i].velocity = bodies[i].velocity + mid_accelerations[i] * TIME_STEP;
        }
    }
    PrimaryBuffer[access] = float2(bodies[0].position.x, bodies[0].position.y);
    PrimaryBuffer[access + 1] = float2(bodies[1].position.x, bodies[1].position.y);
    PrimaryBuffer[access + 2] = float2(bodies[2].position.x, bodies[2].position.y);
}

