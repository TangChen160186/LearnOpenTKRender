#version 460 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;


layout (location = 0) out vec2 oTexCoord;
layout (location = 1) out vec3 oNormal;
layout (location = 2) out vec3 oFragPos;
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
    gl_Position = uProjection * uView * uModel *  vec4(aPos, 1.0);
    oFragPos = vec3(uModel * vec4(aPos,1.0));
    oTexCoord = aTexCoord;
    oNormal = normalize(mat3(transpose(inverse(uModel))) * aNormal);
}
