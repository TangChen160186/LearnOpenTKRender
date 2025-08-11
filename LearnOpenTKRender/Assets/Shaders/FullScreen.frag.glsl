#version 460 core

out vec4 FragColor;
layout(location = 0)  in vec2 iTexCoord;

layout(binding = 0)  uniform sampler2D tex;
void main()
{
    FragColor = texture(tex,iTexCoord); 
}