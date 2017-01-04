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
		 * Takes a feature or set of features and returns all positions as
		 * {@link Point|points}.
		 *
		 * @name explode
		 * @param {(Feature|FeatureCollection)} geojson input features
		 * @return {FeatureCollection<point>} points representing the exploded input features
		 * @throws {Error} if it encounters an unknown geometry type
		 * @example
		 * var poly = {
		 *   "type": "Feature",
		 *   "properties": {},
		 *   "geometry": {
		 *     "type": "Polygon",
		 *     "coordinates": [[
		 *       [177.434692, -17.77517],
		 *       [177.402076, -17.779093],
		 *       [177.38079, -17.803937],
		 *       [177.40242, -17.826164],
		 *       [177.438468, -17.824857],
		 *       [177.454948, -17.796746],
		 *       [177.434692, -17.77517]
		 *     ]]
		 *   }
		 * };
		 *
		 * var points = turf.explode(poly);
		 *
		 * //=poly
		 *
		 * //=points
		 */
		public static FeatureCollection Explode(IGeoJSONObject geojson)
		{
			var points = new List<Feature>();
			CoordEach(geojson,(GeographicPosition coord) => {
				points.Add(Turf.Point(coord));
			});
			return new FeatureCollection(points);
		}
	}
}
