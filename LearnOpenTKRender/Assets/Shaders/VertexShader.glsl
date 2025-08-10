#version 460 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 0) out vec3 oColor;

layout (std140, binding = 0) uniform engineUbo
{
    mat4 uModel;
    mat4 uView;
    mat4 uProjection;
};

void main()
{   


    gl_Position = uProjection * uView * uModel *  vec4(aPos, 1.0);
    oColor = aColor; // Pass the color to the fragment shader
}
