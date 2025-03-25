#version 330

out vec4 frag_color;

in vec3 normal;
in vec3 position;
in flat uint blockId;
in flat uint faceId;

const float ambient = 0.2;
const float diffuse = 0.5;

uniform sampler2DArray main_texture;
uniform sampler2DArray normalmap_texture;

vec4 TriplanarTexture(){
    vec4 c0 = texture(main_texture, vec3(position.x, -position.z, int(blockId) * 3+ int(faceId))) * abs(normal.y);
    vec4 c1 = texture(main_texture, vec3(position.x, -position.y, int(blockId) * 3+ int(faceId))) * abs(normal.z);
    vec4 c2 = texture(main_texture, vec3(position.z, -position.y, int(blockId) * 3+ int(faceId))) * abs(normal.x);

    /*vec4 c0 = texture(main_texture, vec3(position.x, -position.z, 3)) * abs(normal.y);
    vec4 c1 = texture(main_texture, vec3(position.x, -position.y, 3)) * abs(normal.z);
    vec4 c2 = texture(main_texture, vec3(position.z, -position.y, 3)) * abs(normal.x);*/
    
    vec4 col = c0 + c1 + c2;
    
    return col;
};

vec3 TriplanarNormal(){
    vec3 c0 = texture(normalmap_texture, vec3(position.x, -position.z, int(blockId) * 3+ int(faceId))).rgb;
    vec3 c1 = texture(normalmap_texture, vec3(position.x, -position.y, int(blockId) * 3+ int(faceId))).rgb;
    vec3 c2 = texture(normalmap_texture, vec3(position.z, -position.y, int(blockId) * 3+ int(faceId))).rgb ;

    c0 = vec3(c0.x * 2 - 1, normal.y, c0.y * 2 - 1) * abs(normal.y);
    c1 = vec3(c1.x * 2 - 1, c1.y * 2 - 1, normal.z) * abs(normal.z);
    c2 = vec3(normal.x, c2.x * 2 - 1, c2.y * 2 - 1) * abs(normal.x);

    vec3 n = normalize(c0 + c1 + c2);

    return n;
};

void main() {
    vec3 light_dir = normalize(vec3(1, -1, 1));
    vec3 n = TriplanarNormal();
    float diffuse_light = max(dot(-light_dir, normal), 0) * diffuse;
    frag_color = TriplanarTexture() * (ambient);
    //frag_color = vec4(1)  * (ambient + diffuse_light);
}