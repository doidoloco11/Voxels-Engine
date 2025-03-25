#version 330

out vec4 fragColor;

uniform sampler2D ScreenTex;
uniform sampler2D DepthTex;

const int halfKernelSize = 1;
float stepSize = 0.03;

in vec2 uv;

float linearEyeDepth(float nonlinear){
    float z = (2 * 0.1 * 1000) / (1000 + 0.1 - (2*nonlinear - 1) * (1000 - 0.1));
    return z;
}

float rand01(inout float idx){
    idx = sin(idx * 3532) * 4256;
    return fract(idx);
}

vec2 noise(inout vec2 v){
    float b = v.x * 42131 + v.y * 214;
    float x = rand01(b) * 2 - 1;
    float y = rand01(b) * 2 - 1;
    v += vec2(x, y);
    return vec2(x, y);
}

void main() {
    float non_linear_depth = texture(DepthTex, uv).r;
    float depth = linearEyeDepth(non_linear_depth);
    
    float ao = 0;
    int x = 0;
    
    vec2 coord = gl_FragCoord.xy;
    
    for (int i = -halfKernelSize; i <= halfKernelSize; i++){
        for (int j = -halfKernelSize; j <= halfKernelSize; j++){
            vec2 pos = uv + (vec2(i, j) + noise(coord)) * stepSize;
            if (pos.x > 1 || pos.x < 0 || pos.y > 1|| pos.y < 0){
                x++;
                continue;
            }
            float d = texture(DepthTex, pos).r;
            d = linearEyeDepth(d);
            if (depth > d + 0.025){
                float range = smoothstep(0, 1, 0.5/abs(depth - d));
                ao += 1 * range;
            }
        }
    }
    ao = 1 - ((ao + x) / ((halfKernelSize * 2 + 1) * (halfKernelSize * 2 + 1)));
    ao = max(min(ao, 1), 0);
    vec4 col = texture(ScreenTex, uv);
    float fogDensity = pow(exp(-depth), 0.03);
    float fog = mix(1, 0, fogDensity);
    if (depth > 999){
        fog /= 3;
    }
    //ao = pow(ao, 0.25);
    col *= ao;
    col += vec4(fog);
    
    col = pow(col, vec4(2.2));
    col = pow(col, vec4(1/2.2));
    
    fragColor = col;
    //fragColor = vec4(ao, ao, ao, 1);
}