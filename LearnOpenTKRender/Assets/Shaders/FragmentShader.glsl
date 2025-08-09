#version 460 core

out vec4 FragColor;
layout(location = 0)  in vec3 iColor; // Input color from vertex shader

void main()
{
    // Set the output color to a light blue
    FragColor = vec4(iColor,1); // Light blue color
}