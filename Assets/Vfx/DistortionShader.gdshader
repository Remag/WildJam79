shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

uniform sampler2D screenTexture : hint_screen_texture, filter_linear_mipmap;
uniform vec2 startPos;
uniform float effectPower;
uniform float outerSize;
uniform float waveSize;

void fragment() {
	float screenRatio = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
	vec2 scaledUv = ( SCREEN_UV - vec2( 0.5, 0.0 ) ) / vec2( screenRatio, 1.0 ) + vec2( 0.5, 0.0 );
	float outerMask = 1.0 - smoothstep( outerSize - 0.1, outerSize, length( scaledUv - startPos ) );
	float innerMask = smoothstep( outerSize - waveSize - 0.1, outerSize - waveSize, length( scaledUv - startPos ) );
	vec2 displacement = normalize( scaledUv - startPos ) * effectPower * innerMask * outerMask;
	COLOR = texture( screenTexture, SCREEN_UV - displacement );
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
