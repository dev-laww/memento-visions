shader_type canvas_item;

uniform float innerRadius : hint_range(0.0, 1.0) = 0.3;
uniform float outerRadius : hint_range(0.0, 1.0) = 0.7;
uniform float mainAlpha : hint_range(0.0, 1.0) = 1.0;
uniform vec4 vignetteColor: source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    float x = abs(UV.x - 0.5);
    float y = abs(UV.y - 0.5);
    float d = sqrt(x * x + y * y);

    float vignette = smoothstep(innerRadius, outerRadius, d);

    COLOR = vec4(vignetteColor.rgb, vignette * mainAlpha * vignetteColor.a);
}
