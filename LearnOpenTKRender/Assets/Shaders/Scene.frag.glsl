#version 460 core

out vec4 FragColor;
layout(location = 0)  in vec3 iColor; // Input color from vertex shader
layout(location = 1)  in vec2 iTexCoord;

layout(binding = 0)  uniform sampler2D uDiffuse;
void main()
{
    vec4 diffuse = texture(uDiffuse,iTexCoord);
    FragColor = diffuse; 
}