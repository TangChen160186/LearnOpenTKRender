#version 460 core

out vec4 FragColor;
layout(location = 0)  in vec2 iTexCoord;
layout(location = 1)  in vec3 iNormal;
layout(location = 2)  in vec3 iFragPos;

layout(binding = 0)  uniform sampler2D uDiffuse;
layout(binding = 1)  uniform sampler2D uSpecular;
layout(binding = 2)  uniform sampler2D uemission;
uniform vec3 uLightPos;
uniform vec3 uLightColor;


layout (std140, binding = 0) uniform engineUbo
{
    mat4 uModel;
    mat4 uView;
    mat4 uProjection;
    vec3 uCameraPos;
    float uTime;
};

void main()
{
    vec3 diffuseTexColor = texture(uDiffuse,iTexCoord).rgb;
    vec3 sepcularTexColor = texture(uSpecular,iTexCoord).rgb;

    vec2 emissionTexCoord = vec2(iTexCoord.x, iTexCoord.y + uTime * 0.5 + 1);
    vec3 emissionTexColor = texture(uemission,emissionTexCoord).rgb;


    // ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * uLightColor;


    // diffuse 
    vec3 normal = normalize(iNormal);
    vec3 lightDir = normalize(uLightPos - iFragPos);
    float diff = max(dot(normal, lightDir), 0.);
    vec3 diffuse = uLightColor * diff;

    // specular

    vec3 viewDir = normalize(uCameraPos - iFragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    vec3 halfDirection = normalize(lightDir + viewDir);

    float spec = pow(max(dot(normal, halfDirection), 0.0), 32);

    vec3 specular = uLightColor * spec;
    
    
    // combine results
    vec3 result = (ambient + diffuse)* diffuseTexColor + specular * sepcularTexColor + emissionTexColor;
    FragColor = vec4(result, 1.0f);
}