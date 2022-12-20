sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;


float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    //Calculate the radius based on the time
    //Ingame the radius will be controlled by an external variable instead
    //float modTime = frac(uTime / 10) * 1.0;

    //Make the fire more intense close to the edge of the radius, tapering off with distance
    float distanceFactor = 0;
    float textureSize = 4096;
    float projectileWidth = 140;
    
        
    //float2 pixelatedCoord = coords;
    //float pixelSize = 0.001;
    //pixelatedCoord.x = coords.x - fmod(coords.x, pixelSize);
    //pixelatedCoord.y = coords.y - fmod(coords.y, pixelSize);
    
    //coords = pixelatedCoord;
    //return float4(1, 1, 1, 1) * coords.x;

    //distanceFactor = pow(abs(distanceFactor), 45.0);
    float2 samplePos = float2(0, 0);
    //Left
    if (uSaturation == 0) {
        samplePos.x = -1;
        distanceFactor = 1 - (coords.x * textureSize / projectileWidth);
    }
    //Right
    if (uSaturation == 1) {
        samplePos.x = 1;
        distanceFactor = coords.x * textureSize / projectileWidth;
    }
    //Top (evil)

    float timeFactor = uTime / 7;
    float cloudProgress = 0;
    if (uSaturation == 2) {
        timeFactor = uSecondaryColor.z / 500;

        distanceFactor = 1 - (coords.y * textureSize / projectileWidth);

        samplePos.y = -1;

        if (uSecondaryColor.y > 0) {
            cloudProgress = uSecondaryColor.y / 300;
            distanceFactor = lerp(distanceFactor, 14 * pow((distanceFactor * (coords.y * textureSize / projectileWidth)), 2), cloudProgress);
            //timeFactor = lerp(uTime / 7, uTime / 37, cloudProgress);
            //samplePos.y = lerp(0, -1, cloudProgress);
            //samplePos.x = lerp(-1, 0, cloudProgress);
        }


        //distanceFactor += cloudProgress;
        if (distanceFactor > 1) {
            distanceFactor = 1;
        }
    }

    //Bottom
    if (uSaturation == 3) {
        samplePos.y = 1;
        distanceFactor = coords.y * textureSize / projectileWidth;
    }

    samplePos *= timeFactor;
    samplePos += coords;
    samplePos = frac(samplePos);

    //distanceFactor *= 60;
    //Calculate how intense a pixel should be based on the noise generator
    float intensity = tex2D(uImage0, samplePos).r;
    intensity = pow(intensity, 1.8);

    //Calculate and output the final color of the pixel    
    //distanceFactor = distanceFactor * 2.5f;
    //return float4(distanceFactor, intensity, 1, 0.5);

    float r = 2 * pow(intensity, 1) * 1.5f * pow(distanceFactor, 1.5);
    float g = 2 * pow(intensity, 2.0) * distanceFactor * distanceFactor;
    float b = 2 * pow(intensity, 3.0) * 0.15f * distanceFactor * distanceFactor;

    float3 final = lerp(float3(r, g, b), float3(1, 1, 1) * pow(intensity, 1.5) * distanceFactor, cloudProgress);

    return float4(final, 1) * 2 * (uSecondaryColor.x / 100.0);
}

technique FireWallShader
{
    pass FireWallShaderPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}