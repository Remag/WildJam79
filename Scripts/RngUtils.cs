using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public interface IWeightedValue {
	public int Weight { get; set; }
}

public static class Rng {
	public static RandomGenerator GlobalRng { get; set; } = new();

	public static ulong RandomSeed()
	{
		return GlobalRng.RandomSeed();
	}

	public static bool FlipCoin( double trueChance = 0.5 )
	{
		return GlobalRng.FlipCoin( trueChance );
	}

	public static double RandomDouble()
	{
		return GlobalRng.RandomDouble();
	}

	public static int RandomRange( int min, int max )
	{
		return GlobalRng.RandomRange( min, max );
	}

	public static float RandomRange( float min, float max )
	{
		return GlobalRng.RandomRange( min, max );
	}

	public static double RandomRange( double min, double max )
	{
		return GlobalRng.RandomRange( min, max );
	}

	public static T ChooseWeighted<T>( IEnumerable<T> list ) where T : IWeightedValue
	{
		return GlobalRng.ChooseWeighted( list );
	}

	public static T Choose<T>( IList<T> list )
	{
		return GlobalRng.Choose( list );
	}

	public static T ChooseFromSequence<T>( IEnumerable<T> variants )
	{
		return GlobalRng.ChooseFromSequence( variants );
	}

	public static TEnum ChooseFlag<TEnum>( TEnum flagSet ) where TEnum : struct, Enum, IConvertible
	{
		return GlobalRng.ChooseFlag( flagSet );
	}
}

// Custom XorShift RNG.
// Why wouldn't the game just use Random? Uhhhhh, brb.
public class RandomGenerator {
	private static Random _seedSrc = new();

	private ulong _seed;

	public RandomGenerator()
	{
		_seed = unchecked((ulong)_seedSrc.NextInt64());
	}

	public RandomGenerator( ulong seed )
	{
		_seed = seed;
	}

	private void UpdateSeed()
	{
		_seed ^= _seed << 13;
		_seed ^= _seed >> 7;
		_seed ^= _seed << 17;
	}

	public ulong RandomSeed()
	{
		UpdateSeed();

		var result = _seed;
		result ^= result << 8;
		result ^= result >> 13;
		result ^= result << 25;

		return result;
	}

	public double RandomDouble()
	{
		UpdateSeed();
		// See http://prng.di.unimi.it/
		return ( _seed >> 11 ) * ( 1.0 / ( 1ul << 53 ) );
	}

	public float RandomSingle()
	{
		UpdateSeed();
		// See http://prng.di.unimi.it/
		return ( _seed >> 40 ) * ( 1.0f / ( 1u << 24 ) );
	}

	public bool FlipCoin( double trueChance = 0.5 )
	{
		return RandomDouble() < trueChance;
	}

	public int RandomRange( int min, int max )
	{
		Debug.Assert( max >= min );
		UpdateSeed();

		var seed32s = _seed & 0x7FFFFFFF;
		var delta = (ulong)( max - min );
		return min + (int)( ( delta * seed32s + seed32s ) >> 31 );
	}
	public double RandomRange( double min, double max )
	{
		return RandomDouble() * ( max - min ) + min;
	}

	public float RandomRange( float min, float max )
	{
		return RandomSingle() * ( max - min ) + min;
	}

	public T Choose<T>( IList<T> list )
	{
		var index = RandomRange( 0, list.Count - 1 );
		return list[index];
	}

	public T ChooseWeighted<T>( IEnumerable<T> list ) where T : IWeightedValue
	{
		Debug.Assert( list.Any() );
		T result = default;
		var currentTotalWeight = 0.0F;
		foreach( var value in list ) {
			var weight = value.Weight;
			if( weight <= 0 ) {
				continue;
			}
			currentTotalWeight += weight;
			if( FlipCoin( weight / currentTotalWeight ) ) {
				result = value;
			}
		}
		Debug.Assert( currentTotalWeight > 0 );
		return result;
	}

	public T ChooseFromSequence<T>( IEnumerable<T> variants )
	{
		T result = default;
		var currentElem = 0;
		foreach( var val in variants ) {
			currentElem++;
			if( FlipCoin( 1.0F / currentElem ) ) {
				result = val;
			}
		}
		return result;
	}

	public TEnum ChooseFlag<TEnum>( TEnum flagSet ) where TEnum : struct, Enum, IConvertible
	{
		var flagList = Enum.GetValues<TEnum>().Where( ( flag ) => {
			if( !flagSet.HasFlag( flag ) ) {
				return false;
			}
			// Check for a single bit set.
			var flagVal = flag.ToUInt64( null );
			return ( flagVal & ( flagVal - 1 ) ) == 0;
		} );
		return ChooseFromSequence( flagList );
	}
}
