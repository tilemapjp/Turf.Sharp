using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		/**
		 * Takes a set of features, calculates the bbox of all input features, and returns a bounding box.
		 *
		 * @name bbox
		 * @param {(Feature|FeatureCollection)} geojson input features
		 * @return {Array<number>} bbox extent in [minX, minY, maxX, maxY] order
		 * @example
		 * var pt1 = point([114.175329, 22.2524])
		 * var pt2 = point([114.170007, 22.267969])
		 * var pt3 = point([114.200649, 22.274641])
		 * var pt4 = point([114.200649, 22.274641])
		 * var pt5 = point([114.186744, 22.265745])
		 * var features = featureCollection([pt1, pt2, pt3, pt4, pt5])
		 *
		 * var bbox = turf.bbox(features);
		 *
		 * var bboxPolygon = turf.bboxPolygon(bbox);
		 *
		 * //=bbox
		 *
		 * //=bboxPolygon
		 */
		public static List<double> Bbox(IGeoJSONObject geojson)
		{
			var bbox = new List<double>()
			{
				double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity
			};
			Turf.CoordEach(geojson, (List<double> coord) => { 
				if (bbox[0] > coord[0]) bbox[0] = coord[0];
				if (bbox[1] > coord[1]) bbox[1] = coord[1];
				if (bbox[2] < coord[0]) bbox[2] = coord[0];
				if (bbox[3] < coord[1]) bbox[3] = coord[1];			
			});
			return bbox;
		}


		/**
		 * Takes a {@link Point} and calculates the circle polygon given a radius in degrees, radians, miles, or kilometers; and steps for precision.
		 *
		 * @name circle
		 * @param {Feature<Point>} center center point
		 * @param {number} radius radius of the circle
		 * @param {number} [steps=64] number of steps
		 * @param {string} [units=kilometers] miles, kilometers, degrees, or radians
		 * @returns {Feature<Polygon>} circle polygon
		 * @example
		 * var center = point([-75.343, 39.984]);
		 * var radius = 5;
		 * var steps = 10;
		 * var units = 'kilometers';
		 *
		 * var circle = turf.circle(center, radius, steps, units);
		 *
		 * //=circle
		 */
		public static Feature Circle(Feature center, double radius, int steps = 64, string units = "kilometers")
		{
			List<IPosition> coordinates = new List<IPosition>();

			for (var i = 0; i < steps; i++)
			{
				coordinates.Add(((Point)Turf.Destination(center, radius, i * 360 / steps, units).Geometry).Coordinates);
			}

			coordinates.Add(coordinates[0]);

			return new Feature(
				new Polygon(new List<LineString>() {
					new LineString( coordinates )
				})
			);
		}
	}
}
