shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screenTexture: hint_screen_texture, repeat_disable, filter_nearest;
uniform vec4 color : source_color = vec4(1.0);
uniform float width: hint_range(0, 10) = 1.0;
uniform int pattern: hint_range(0, 2) = 0; // diamond, circle, square
uniform bool inside = false;

bool hasContraryNeighbour(vec2 uv, vec2 texture_pixel_size, sampler2D screen_texture) {
    for (float i = -ceil(width); i <= ceil(width); i++) {
        float x = abs(i) > width ? sign(i) : i;
        float offset;
        
        if (pattern == 0) {
            offset = width - abs(x);
        } else if (pattern == 1) {
            offset = floor(sqrt(pow(width + 0.5, 2) - x * x));
        } else if (pattern == 2) {
            offset = width;
        }
        
        for (float j = -ceil(offset); j <= ceil(offset); j++) {
            float y = abs(j) > offset ? offset * sign(j) : j;
            vec2 xy = uv + texture_pixel_size * vec2(x, y);
            
            if ((xy != clamp(xy, vec2(0.0), vec2(1.0)) || texture(screen_texture, xy).a == 0.0) == inside) {
                return true;
            }
        }
    }
    
    return false;
}

void fragment() {
    COLOR = textureLod(screenTexture, SCREEN_UV, 0.0);
    
    vec2 screen_size = vec2(textureSize(screenTexture, 0));
    vec2 base_resolution_size = vec2(640.0, 360.0);
    vec2 scale_multiplier = screen_size / base_resolution_size;
    vec2 adjusted_screen_pixel_size = SCREEN_PIXEL_SIZE * scale_multiplier;
    
    if ((COLOR.a > 0.0) == inside && hasContraryNeighbour(SCREEN_UV, adjusted_screen_pixel_size, screenTexture)) {
        COLOR.rgb = inside ? mix(COLOR.rgb, COLOR.rgb, color.a) : color.rgb;
        COLOR.a += (1.0 - COLOR.a) * color.a;
    }
}