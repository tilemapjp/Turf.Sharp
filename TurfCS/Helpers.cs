using System;
using System.Collections.Generic;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		/**
		 * Takes coordinates and properties (optional) and returns a new {@link Point} feature.
		 *
		 * @name point
		 * @param {Array<number>} coordinates longitude, latitude position (each in decimal degrees)
		 * @param {Object=} properties an Object that is used as the {@link Feature}'s
		 * properties
		 * @returns {Feature<Point>} a Point feature
		 * @example
		 * var pt1 = turf.point([-75.343, 39.984]);
		 *
		 * //=pt1
		 */
		public static Feature Point(double[] coordinates, Dictionary<string,object> properties = null)
		{
			if (coordinates.Length < 2) throw new Exception("Coordinates must be at least 2 numbers long");
			return Point(new GeographicPosition(coordinates[1], coordinates[0]),properties);
		}
		public static Feature Point(List<double> coordinates, Dictionary<string, object> properties = null)
		{
			if (coordinates.Count < 2) throw new Exception("Coordinates must be at least 2 numbers long");
			return Point(new GeographicPosition(coordinates[1], coordinates[0]), properties);
		}
		public static Feature Point(IPosition coordinates, Dictionary<string, object> properties = null)
		{
			var point = new Point(coordinates);
			return new Feature(point, properties);
		}

		static private Dictionary<string, double> factors = new Dictionary<string, double>() {
			{"miles", 3960},
			{"nauticalmiles", 3441.145},
			{"degrees", 57.2957795},
			{"radians", 1},
			{"inches", 250905600},
			{"yards", 6969600},
			{"meters", 6373000},
			{"metres", 6373000},
			{"kilometers", 6373},
			{"kilometres", 6373},
			{"feet", 20908792.65}
		};

		/*
		 * Convert a distance measurement from radians to a more friendly unit.
		 *
		 * @name RadiansToDistance
		 * @param {double} distance in radians across the sphere
		 * @param {string} [units=kilometers] can be degrees, radians, miles, or kilometers
		 * inches, yards, metres, meters, kilometres, kilometers.
		 * @returns {double} distance
		 */
		static public double RadiansToDistance(double radians, string units = "kilometers")
		{
			double factor;
			if (factors.TryGetValue(units,out factor))
			{
				return radians * factor;
			} else {
				throw new Exception("Invalid unit");
			}
		}


	
	
	}
}
