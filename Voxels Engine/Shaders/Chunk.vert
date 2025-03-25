#version 330

precision highp uint;

layout (location = 0) in highp uint packedData;

uniform mat4 m_proj;
uniform mat4 m_look;
uniform mat4 m_model;

out vec3 normal;
out vec3 position;
out flat uint blockId;
out flat uint faceId;

void unpackData(out vec3 position, out vec3 normal, out uint faceId, out uint blockId){
    uint mask = 31u;
    uint mask2 = 3u;
    uint mask3 = 255u;
    uint mask4 = 7u;
    
    uint x = uint((packedData >> 6) & mask);
    uint y = uint((packedData >> 11) & mask);
    uint z = uint((packedData >> 16) & mask);
    
    int nx = int((packedData >> 0) & mask2);
    int ny = int((packedData >> 2) & mask2);
    int nz = int((packedData >> 4) & mask2);
    blockId = uint((packedData >> 21) & mask3);
    faceId = uint((packedData >> 29) & mask4);
    
    position = vec3(x, y, z);
    
    normal = vec3(nx, ny, nz) - 1;
}

void main() {
    vec3 in_position;
    vec3 in_normal;
    uint in_blockId = 0u;
    uint in_faceId = 0u;
    unpackData(in_position, in_normal, in_faceId, in_blockId);
    
    vec4 clippos = (m_proj * m_look * m_model * vec4(in_position, 1)).xyzw;
    
    position = in_position;
    normal = in_normal;
    blockId = in_blockId;
    faceId = in_faceId;
    gl_Position = clippos;
}