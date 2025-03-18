shader_type canvas_item;
render_mode unshaded;

uniform sampler2D dissolveTex;
uniform sampler2D mainTex;

void vertex() {
}

void fragment() {
		float mainAlpha = texture( mainTex, UV ).r;
		float dissolveAlpha = texture( dissolveTex, UV ).r;

		float dissolveMult = smoothstep( COLOR.a, 1, dissolveAlpha );
		COLOR.a = mainAlpha * dissolveMult;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
