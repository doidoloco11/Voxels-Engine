#version 330

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec3 in_normal;
layout (location = 2) in float in_blockId;
layout (location = 3) in float in_faceId;

uniform mat4 m_proj;
uniform mat4 m_look;

out vec3 normal;
out vec3 position;
out float blockId;
out float faceId;

void main() {
    
    vec4 clippos = (m_proj * m_look * vec4(in_position, 1)).xyzw;
    
    position = in_position;
    normal = in_normal;
    blockId = in_blockId;
    faceId = in_faceId;
    gl_Position = clippos;
}