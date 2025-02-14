#version 330

out vec4 frag_color;

in vec3 normal;
in vec3 position;
in float blockId;
in float faceId;

const float ambient = 0.2;
const float diffuse = 0.5;

uniform sampler2DArray main_texture;

vec4 TriplanarTexture(){
    vec4 c0 = texture(main_texture, vec3(position.x, -position.z, blockId * 3+ faceId)) * abs(normal.y);
    vec4 c1 = texture(main_texture, vec3(position.x, -position.y, blockId * 3+ faceId)) * abs(normal.z);
    vec4 c2 = texture(main_texture, vec3(position.z, -position.y, blockId * 3+ faceId)) * abs(normal.x);
    
    vec4 col = c0 + c1 + c2;
    
    return col;
};

void main() {
    vec3 light_dir = normalize(vec3(1, -1, 1));
    float diffuse_light = max(dot(-light_dir, normal), 0) * diffuse;
    frag_color = TriplanarTexture() * (ambient + diffuse_light);
}