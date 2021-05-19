/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

// Easing functions for interpolation
// Values are evaluated from 0 to 1, without clamping.
// Reference: https://realtimevfx.com/t/collection-of-useful-curve-shaping-functions/3704/5

#ifndef UNITY_PI
#define UNITY_PI      3.14159265359f
#define UNITY_HALF_PI 1.57079632679f
#endif

inline float easeInOutLinear(float x)
{
	return x;
}

inline float easeInOutPow(float x, float power)
{
	return pow(x, power);
}

inline float easeInOutSin(float x, float power)
{
	return pow(sin(UNITY_HALF_PI * x), power);
}

inline float easeInOutCos(float x, float power)
{
	return 1.0 - pow(cos(UNITY_HALF_PI * x), power);
}


// Preset Functions

inline float easeInOutSmooth(float x)
{
	return easeInOutSin(x, 2.0);
}

inline float easeOutSmooth(float x)
{
	return easeInOutSin(x, 1.0);
}

inline float easeInSmooth(float x)
{
	return easeInOutCos(x, 1.0);
}