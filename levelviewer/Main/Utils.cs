// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public static class Utils
    {
        private static Random _random = new Random();

        public static string GetFilenameWithoutExtension(string filename)
        {
            int dot = filename.LastIndexOf('.');
            if (dot == -1 || dot == 0)
                return filename;

            return filename.Substring(0, dot);
        }

        public static string ConvertAsciiToString(byte[] ascii)
        {
            string text = System.Text.Encoding.ASCII.GetString(ascii);

            return text.Trim((char)0);
        }

        public static bool TestRayTriangleCollision(Math.Vector3 origin,
			Math.Vector3 direction,  Math.Vector3 v1,
		    Math.Vector3 v2, Math.Vector3 v3,
			out float distance, out Math.Vector3? collisionPoint)
		{
            distance = 0;
            collisionPoint = null;

			const float EPSILON = 0.00001f;
			
			Math.Vector3 edge1 = v2 - v1;
			Math.Vector3 edge2 = v3 - v1;
			
			Math.Vector3 pvec = direction.Cross(edge2);
			
			float det = edge1.Dot(pvec);
			
			if (det > -EPSILON && det < EPSILON)
				return false;
			
			float inv_det = 1.0f / det;
			
			Math.Vector3 tvec = origin - v1;
			
			float u = tvec.Dot(pvec) * inv_det;
			if (u < 0.0f || u > 1.0f)
				return false;
			
			Math.Vector3 qvec = tvec.Cross(edge1);
			
			float v = direction.Dot(qvec) * inv_det;
			if (v < 0.0f || u + v > 1.0f)
				return false;
			
			// pack up the results
            distance = edge2.Dot(qvec) * inv_det;
			
			collisionPoint = v1 + edge1 * u + edge2 * v;
			
			return true;
		}

        public static bool TestRayAABBCollision(Math.Vector3 aabbOffset, Math.Vector3 origin,
            Math.Vector3 direction, float[] aabb, out float distance)
        {
            // based on http://www.cs.utah.edu/~awilliam/box/box.pdf

            distance = float.MinValue;
            Math.Vector3 inverseDirection = new Math.Vector3(1.0f / direction.X, 1.0f / direction.Y, 1.0f / direction.Z);

            int signX = inverseDirection.X < 0 ? 1 : 0;
            int signY = inverseDirection.Y < 0 ? 1 : 0;
            int signZ = inverseDirection.Z < 0 ? 1 : 0;

            float tmin = (aabb[signX * 3 + 0] + aabbOffset.X - origin.X) * inverseDirection.X;
            float tmax = (aabb[(1 - signX) * 3 + 0] + aabbOffset.X - origin.X) * inverseDirection.X;
            float tymin = (aabb[signY * 3 + 1] + aabbOffset.Y - origin.Y) * inverseDirection.Y;
            float tymax = (aabb[(1 - signY) * 3 + 1] + aabbOffset.Y - origin.Y) * inverseDirection.Y;

            if (tmin > tymax || tymin > tmax)
                return false;
            if (tymin > tmin)
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;

            float tzmin = (aabb[signZ * 3 + 2] + aabbOffset.Z - origin.Z) * inverseDirection.Z;
            float tzmax = (aabb[(1 - signZ) * 3 + 2] + aabbOffset.Z - origin.Z) * inverseDirection.Z;

            if (tmin > tzmax || tzmin > tmax)
                return false;

            if (tzmin > tmin)
                tmin = tzmin;
            if (tzmax < tmax)
                tmax = tzmax;

            distance = tmin;
            return tmin > 0;

        }

        public static bool TestRaySphereCollision(Math.Vector3 origin,
            Math.Vector3 direction, Math.Vector3 spherePosition, float radius, out float distance)
        {
            Math.Vector3 sphereToOrigin = origin - spherePosition;
            float b = 2 * (sphereToOrigin.Dot(direction));
            float c = sphereToOrigin.Dot(sphereToOrigin) - radius * radius;

            float d = b * b - 4 * c;
            if (d < 0)
            {
                distance = 0;
                return false;
            }

            float dsqrt = (float)System.Math.Sqrt(d);
            float q;

            if (b < 0) q = (-b - dsqrt) * 0.5f;
            else q = (-b + dsqrt) * 0.5f;

            float t0 = q;
            float t1 = c / q;

            if (t0 > t1)
            {
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }

            if (t1 < 0)
            {
                distance = 0;
                return false;
            }

            if (t0 < 0)
            {
                distance = t1;
            }
            else
            {
                distance = t1;
            }
            
            return true;
        }

        public static float RollFloatingDie()
        {
            return (float)_random.NextDouble();
        }

        public static int PickRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static float DegreesToRadians(float degrees)
        {
            return degrees * Math.Constants.RadiansPerDegree;
        }

        /// <summary>
        /// If the given string doesn't have the specified ending it is appended.
        /// </summary>
        public static string MakeEndsWith(string original, string ending)
        {
            if (original.EndsWith(ending, StringComparison.OrdinalIgnoreCase) == false)
                return original + ending;

            return original;
        }

        public static string ReplaceStringCaseInsensitive(string original, string pattern, string replacement)
        {
            // based on code from http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();

            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + System.Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];

            return new string(chars, 0, count);
        }

        public static IntPtr IncrementIntPtr(IntPtr ptr, int numBytes)
        {
            return (IntPtr)(ptr.ToInt64() + numBytes);
        }
    }
}
