shader_type canvas_item;

uniform sampler2D Texture2DParameter : filter_nearest;
uniform vec4 Tint : source_color;
uniform float ResourceValue;
uniform float ResourceMax;

// rgb->hsl functions found originally at https://github.com/paddy-exe/ShaderFunction-Extras
vec3 rgb_to_hsv(vec3 color) {
	vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = mix(vec4(color.bg, K.wz), vec4(color.gb, K.xy), step(color.b, color.g));
	vec4 q = mix(vec4(p.xyw, color.r), vec4(color.r, p.yzx), step(p.x, color.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv_to_rgb(vec3 color) {
	vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	vec3 p = abs(fract(color.xxx + K.xyz) * 6.0 - K.www);
	return color.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), color.y);
}

void vertex() {
	// Called for every vertex the material is visible on.
	float Percentage = ResourceValue/ResourceMax;
	vec2 AdjustedUV = UV;
	AdjustedUV.x += 1.0 - Percentage;
	UV = AdjustedUV;
}

void fragment() {
	vec4 AlphaCorrectedColor = texture(Texture2DParameter, UV);
	float Percentage = ResourceValue/ResourceMax;
	
	if (UV.x - 1.0 + Percentage > Percentage)
	{
		discard;
	}
	
	vec3 hslColor = rgb_to_hsv(AlphaCorrectedColor.rgb);
	vec3 hslTint = rgb_to_hsv(Tint.rgb);
	hslColor.r = hslTint.r;
	hslColor.g = hslTint.g;
	vec3 tintedColor = hsv_to_rgb(hslColor);
	COLOR = vec4(tintedColor.r, tintedColor.g, tintedColor.b, AlphaCorrectedColor.a);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
