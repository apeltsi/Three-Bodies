#define TIME_STEP 0.8f
#define TICKS 5000
#define G 0.0000000000667f
#define MASS 50

[[vk::binding(0, 0)]]
RWStructuredBuffer<float2> PrimaryBuffer;
struct Body
{
    float2 position;
    float2 velocity;
    float2 acceleration;
};

[[vk::binding(1, 0)]]
StructuredBuffer<float> RandomBuffer;

[[vk::binding(2, 0)]]
cbuffer BodyData
{
    float ax;
    float ay;
    float bx;
    float by;
    float cx;
    float cy;
    float2 _padding;
}

struct ArrData
{
    float2 data[3];
};

ArrData CalculateAccelerations(Body bodies[3])
{
    float2 accelerations[3];
    [unroll(3)]
    for (int i = 0; i < 3; i++)
    {
        float2 netAcceleration = float2(0, 0);
        [unroll(3)]
        for (int j = 0; j < 3; j++)
        {
            if (i != j)
            {
                // float2 r = bodies[j].position - bodies[i].position;
                // float softeningFactor = 0.001;
                // float distance = length(r) + softeningFactor;
                // if (distance < 1e-10) continue;
                // float2 direction = r / distance;
                // float accelerationMagnitude = G * (MASS * MASS / (distance * distance));
                // netAcceleration += direction * accelerationMagnitude;
                float dx = bodies[j].position.x - bodies[i].position.x;
                float dy = bodies[j].position.y - bodies[i].position.y;
                float softeningFactor = 0.001f;
                float distance = sqrt(dx * dx + dy * dy) + softeningFactor;
                if (distance < 1e-10f) continue;
                float accelerationMagnitude = G * (MASS * MASS / (distance * distance));

                netAcceleration += float2(dx, dy) * accelerationMagnitude / distance;
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
    bodies[0].position = float2(ax, ay); + float2(RandomBuffer[(id.x * 99) % 10000], RandomBuffer[(id.x * 867 + 39) % 10000]) * 0.01f;
    bodies[1].position = float2(bx, by); + float2(RandomBuffer[(id.x * 152 + 6) % 10000], RandomBuffer[(id.x * 12 + 3) % 10000]) * 0.01f;
    bodies[2].position = float2(cx, cy); + float2(RandomBuffer[(id.x * 7 + 92) % 10000], RandomBuffer[(id.x * 786 + 5) % 10000]) * 0.01f;
    
    for(int t = 0; t < TICKS; t++)
    {
        float2 initialAccelerations[3] = CalculateAccelerations(bodies).data;

        // Vi räknar ut mellanstegen
        Body midbodies[3];
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            midbodies[i].position = bodies[i].position + bodies[i].velocity * TIME_STEP * 0.5f;
            midbodies[i].velocity = bodies[i].velocity + initialAccelerations[i] * TIME_STEP * 0.5f;
        }

        // Vi räknar ut mellan accelerationerna
        const float2 mid_accelerations[3] = CalculateAccelerations(midbodies).data;

        // Slutligen kan vi uppdatera positionerna och hastigheterna
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            bodies[i].position += midbodies[i].velocity * TIME_STEP;
            bodies[i].velocity += mid_accelerations[i] * TIME_STEP;
        }
    }
    PrimaryBuffer[access] =     float2(bodies[0].position.x, bodies[0].position.y);
    PrimaryBuffer[access + 1] = float2(bodies[1].position.x, bodies[1].position.y);
    PrimaryBuffer[access + 2] = float2(bodies[2].position.x, bodies[2].position.y);
}

