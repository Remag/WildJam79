using Godot;

namespace WildJam78.Scripts.Audio {
    [GlobalClass]
    public partial class RandomPitchSoundPlayer : AudioStreamPlayer {
        float _lastPitch = 1f;
        public new void Play( float fromPosition = 0f )
        {
            while( Mathf.Abs( _lastPitch - PitchScale ) < 0.1 ) {
                PitchScale = (float)GD.RandRange( 0.5, 1.5 );
            }
            _lastPitch = PitchScale;
            base.Play( fromPosition );
        }
    }
}
