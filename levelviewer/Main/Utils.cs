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
    static class Utils
    {
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

        public static bool TestRayTriangleCollision(Math.Vector origin,
			Math.Vector direction,  Math.Vector v1,
		    Math.Vector v2, Math.Vector v3,
			out float distance, out Math.Vector collisionPoint)
		{
            distance = 0;
            collisionPoint = null;

			const float EPSILON = 0.00001f;
			
			Math.Vector edge1 = v2 - v1;
			Math.Vector edge2 = v3 - v1;
			
			Math.Vector pvec = direction.Cross(edge2);
			
			float det = edge1.Dot(pvec);
			
			if (det > -EPSILON && det < EPSILON)
				return false;
			
			float inv_det = 1.0f / det;
			
			Math.Vector tvec = origin - v1;
			
			float u = tvec.Dot(pvec) * inv_det;
			if (u < 0.0f || u > 1.0f)
				return false;
			
			Math.Vector qvec = tvec.Cross(edge1);
			
			float v = direction.Dot(qvec) * inv_det;
			if (v < 0.0f || u + v > 1.0f)
				return false;
			
			// pack up the results
            distance = edge2.Dot(qvec) * inv_det;
			
			collisionPoint = v1 + edge1 * u + edge2 * v;
			
			return true;
		}
    }
}
