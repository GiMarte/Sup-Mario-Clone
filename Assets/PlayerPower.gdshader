shader_type canvas_item;

uniform int power_type = 0;
varying vec4 sprite_tint;

void vertex() {
    sprite_tint = COLOR;
}

void fragment() {
    vec4 pixel = texture(TEXTURE, UV);
    bool shirt = distance(pixel.rgb, vec3(76.0, 175.0, 80.0) / 255.0) < 0.01;
    bool hat = distance(pixel.rgb, vec3(34.0, 177.0, 76.0) / 255.0) < 0.01;
    bool overalls = distance(pixel.rgb, vec3(63.0, 81.0, 181.0) / 255.0) < 0.01;
    if (power_type == 2) {
        if (shirt) pixel.rgb = vec3(1.0, 0.48, 0.16);
        if (hat) pixel.rgb = vec3(0.94, 0.20, 0.12);
        if (overalls) pixel.rgb = vec3(0.72, 0.12, 0.14);
    } else if (power_type == 1) {
        if (shirt) pixel.rgb = vec3(0.56, 0.92, 1.0);
        if (hat) pixel.rgb = vec3(0.33, 0.78, 0.96);
        if (overalls) pixel.rgb = vec3(0.10, 0.48, 0.85);
    }
    COLOR = pixel * sprite_tint;
}
