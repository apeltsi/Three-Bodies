#define TIME_STEP 0.6f
#define TICKS 40000
#define G 0.0000000000667f
#define MASS 50.0f

[[vk::binding(0, 0)]]
RWStructuredBuffer<float2> PrimaryBuffer;

[[vk::binding(1, 0)]]
StructuredBuffer<float> RandomBuffer;

[[vk::binding(2, 0)]]
cbuffer BodyData
{
    float2 a;
    float2 b;
    float2 c;
    int simcount;
    int framecount;
}

struct ArrData
{
    float2 data[3];
};

struct Body
{
    float2 position;
    float2 velocity;
    float2 acceleration;
};

ArrData CalculateAccelerations(Body bodies[3])
{
    float2 accelerations[3];
    [loop]
    for (int i = 0; i < 3; i++)
    {
        float2 netAcceleration = float2(0.0f, 0.0f);
        [loop]
        for (int j = 0; j < 3; j++)
        {
            if (i != j)
            {
                float2 r = bodies[j].position - bodies[i].position;
                float softeningFactor = 0.001f;
                float distance = length(r) + softeningFactor;
                float2 direction = r / distance;
                float accelerationMagnitude = G * (MASS * MASS / (distance * distance));
                netAcceleration += direction * accelerationMagnitude;
            }
        }
        accelerations[i] = netAcceleration;
    }
    ArrData data;
    data.data = accelerations;
    return data;
}

[numthreads(128,1,1)]
void main(int3 id : SV_DispatchThreadID)
{
    const int access = id.x * 6;
    const int frameInterval = TICKS / framecount;
    Body bodies[3];
    const int randaccess = id.x * 10;

    // Initial positions with some randomness
    bodies[0].position = float2(a.x, a.y)
                        + float2(RandomBuffer[randaccess % 10000], RandomBuffer[(randaccess + 1) % 10000]) * 0.001f;
    bodies[1].position = float2(b.x, b.y)
                        + float2(RandomBuffer[(randaccess + 2) % 10000], RandomBuffer[(randaccess + 3) % 10000]) * 0.001f;
    bodies[2].position = float2(c.x, c.y)
                        + float2(RandomBuffer[(randaccess + 4) % 10000], RandomBuffer[(randaccess + 5) % 10000]) * 0.001f;
    
    for(int t = 0; t < TICKS; t++)
    {
        float2 initialAccelerations[3] = CalculateAccelerations(bodies).data;

        // Calculate the midpoint positions and velocities
        Body midbodies[3];
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            midbodies[i].position = bodies[i].position + bodies[i].velocity * TIME_STEP * 0.5f;
            midbodies[i].velocity = bodies[i].velocity + initialAccelerations[i] * TIME_STEP * 0.5f;
        }

        // Calculate midpoint accelerations
        const float2 mid_accelerations[3] = CalculateAccelerations(midbodies).data;

        // Update true positions & velocities using the midpoint values
        [unroll(3)]
        for (int i = 0; i < 3; i++)
        {
            bodies[i].position += midbodies[i].velocity * TIME_STEP;
            bodies[i].velocity += mid_accelerations[i] * TIME_STEP;
        }

        // Update the buffer of frames (not a framebuffer this is different)
        if(t % frameInterval == 0)
        {
            const int frame = floor(t / frameInterval);

            // Positional Data
            PrimaryBuffer[frame * simcount * 6 + access] = bodies[0].position;
            PrimaryBuffer[frame * simcount * 6 + access + 1] = bodies[1].position;
            PrimaryBuffer[frame * simcount * 6 + access + 2] = bodies[2].position;

            // Velocity Data
            PrimaryBuffer[frame * simcount * 6 + access + 3] = bodies[0].velocity;
            PrimaryBuffer[frame * simcount * 6 + access + 4] = bodies[1].velocity;
            PrimaryBuffer[frame * simcount * 6 + access + 5] = bodies[2].velocity;
        }
    }
}

