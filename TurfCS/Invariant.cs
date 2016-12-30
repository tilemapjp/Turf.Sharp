using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	internal class Invariant
	{
		/**
		 * Unwrap a coordinate from a Feature with a Point geometry, a Point
		 * geometry, or a single coordinate.
		 *
		 * @param {*} obj any value
		 * @returns {Array<number>} a coordinate
		 */
		static internal List<double> GetCoord(Object obj)
		{
			if (obj is List<double>)
			{
				return (List<double>)obj;
			}
			else if (obj is double[])
			{
				var dList = new List<double>();
				dList.AddRange((double[])obj);
				return dList;
			}
			else if ((obj is Feature && ((Feature)obj).Geometry.Type == GeoJSONObjectType.Point) ||
				obj is Point)
			{
				var point = obj is Feature ? (Point)((Feature)obj).Geometry : (Point)obj;
				var coords = (GeographicPosition)point.Coordinates;
				return new List<double>() { coords.Longitude, coords.Latitude };
			}
			throw new Exception("A coordinate, feature, or point geometry is required");
		}
	}
}
