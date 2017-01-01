using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace TurfCS
{
	public partial class Turf
	{
		/**
		 * Unwrap a coordinate from a Feature with a Point geometry, a Point
		 * geometry, or a single coordinate.
		 *
		 * @param {*} obj any value
		 * @returns {Array<number>} a coordinate
		 */
		static public List<double> GetCoord(Object obj)
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
			else if (obj is GeographicPosition)
			{
				var coords = (GeographicPosition)obj;
				return new List<double>() { coords.Longitude, coords.Latitude };
			}
			throw new Exception("A coordinate, feature, or point geometry is required");
		}

		/**
		 * Iterate over coordinates in any GeoJSON object, similar to
		 * Array.forEach.
		 *
		 * @name coordEach
		 * @param {Object} layer any GeoJSON object
		 * @param {Function} callback a method that takes (value)
		 * @param {boolean=} excludeWrapCoord whether or not to include
		 * the final coordinate of LinearRings that wraps the ring in its iteration.
		 * @example
		 * var point = { type: 'Point', coordinates: [0, 0] };
		 * coordEach(point, function(coords) {
		 *   // coords is equal to [0, 0]
		 * });
		 */
		static public void CoordEach(IGeoJSONObject layer, Action<List<double>> callback, bool excludeWrapCoord = false)
		{
			CoordEach(layer, (GeographicPosition obj) => { callback(GetCoord(obj)); }, excludeWrapCoord);
		}
		static public void CoordEach(IGeoJSONObject layer, Action<GeographicPosition> callback, bool excludeWrapCoord = false)
		{
			var isFeatureCollection = layer.Type == GeoJSONObjectType.FeatureCollection;
			var isFeature = layer.Type == GeoJSONObjectType.Feature;
			var stop = isFeatureCollection ? ((FeatureCollection)layer).Features.Count : 1;

			// This logic may look a little weird. The reason why it is that way
			// is because it's trying to be fast. GeoJSON supports multiple kinds
			// of objects at its root: FeatureCollection, Features, Geometries.
			// This function has the responsibility of handling all of them, and that
			// means that some of the `for` loops you see below actually just don't apply
			// to certain inputs. For instance, if you give this just a
			// Point geometry, then both loops are short-circuited and all we do
			// is gradually rename the input until it's called 'geometry'.
			//
			// This also aims to allocate as few resources as possible: just a
			// few numbers and booleans, rather than any temporary arrays as would
			// be required with the normalization approach.
			for (var i = 0; i < stop; i++)
			{
				var geometryMaybeCollection = isFeatureCollection ?
					(IGeometryObject)((FeatureCollection)layer).Features[i].Geometry :
											  isFeature ? (IGeometryObject)((Feature)layer).Geometry :
											  (IGeometryObject)layer;
				CoordEach(geometryMaybeCollection, callback, excludeWrapCoord);
			}
		}
		static private void CoordEach(IGeometryObject layer, Action<GeographicPosition> callback, bool excludeWrapCoord = false)
		{
			var isGeometryCollection = layer.Type == GeoJSONObjectType.GeometryCollection;
			var stopG = isGeometryCollection ? ((GeometryCollection)layer).Geometries.Count : 1;

			for (var g = 0; g < stopG; g++)
			{
				var geometry = isGeometryCollection ?
					((GeometryCollection)layer).Geometries[g] : layer;

				var wrapShrink = (excludeWrapCoord &&
								  (geometry.Type == GeoJSONObjectType.Polygon ||
								   geometry.Type == GeoJSONObjectType.MultiPolygon)) ? 1 : 0;

				if (geometry.Type == GeoJSONObjectType.Point)
				{
					callback((GeographicPosition)((Point)layer).Coordinates);
				}
				else if (geometry.Type == GeoJSONObjectType.LineString || geometry.Type == GeoJSONObjectType.MultiPoint)
				{
					var coords = geometry.Type == GeoJSONObjectType.LineString ?
										 ((LineString)layer).Coordinates :
										 ((MultiPoint)layer).Coordinates.Select(x => x.Coordinates).ToList();
					for (var j = 0; j < coords.Count; j++) callback((GeographicPosition)coords[j]);
				}
				else if (geometry.Type == GeoJSONObjectType.Polygon || geometry.Type == GeoJSONObjectType.MultiLineString)
				{
					var coords1 = geometry.Type == GeoJSONObjectType.Polygon ?
										  ((Polygon)layer).Coordinates : ((MultiLineString)layer).Coordinates;
					for (var j = 0; j < coords1.Count; j++)
					{
						var coords2 = coords1[j].Coordinates;
						for (var k = 0; k < coords2.Count - wrapShrink; k++)
							callback((GeographicPosition)coords2[k]);
					}
				}
				else if (geometry.Type == GeoJSONObjectType.MultiPolygon)
				{
					var coords1 = ((MultiPolygon)layer).Coordinates;
					for (var j = 0; j < coords1.Count; j++)
					{
						var coords2 = coords1[j].Coordinates;
						for (var k = 0; k < coords2.Count; k++)
						{
							var coords3 = coords2[k].Coordinates;
							for (var l = 0; l < coords3.Count - wrapShrink; l++)
								callback((GeographicPosition)coords3[l]);
						}
					}
				}
				else if (geometry.Type == GeoJSONObjectType.GeometryCollection)
				{
					for (var j = 0; j < ((GeometryCollection)geometry).Geometries.Count; j++)
						CoordEach(((GeometryCollection)geometry).Geometries[j], callback, excludeWrapCoord);
				}
				else {
					throw new Exception("Unknown Geometry Type");
				}
			}
		}

		static public object CoordReduce(IGeoJSONObject layer, Func<object, GeographicPosition, object> callback,
										 object memo, bool excludeWrapCoord = false)
		{
			object ret = memo;
			Action<GeographicPosition> internal_cb = (GeographicPosition coord) => {
				ret = callback(ret, coord);
			};
			CoordEach(layer, internal_cb, excludeWrapCoord);
			return ret;
		}

		static public void PropEach(IGeoJSONObject layer, Action<Dictionary<string, object>, int> callback)
		{
			if (layer.Type == GeoJSONObjectType.FeatureCollection)
			{
				for (var i = 0; i < ((FeatureCollection)layer).Features.Count; i++)
				{
					callback(((FeatureCollection)layer).Features[i].Properties, i);
				}
			}
			else if (layer.Type == GeoJSONObjectType.Feature)
			{
				callback(((Feature)layer).Properties, 0);
			}
			else {
				throw new Exception("Feature or FeatureCollection must be given");
			}
		}
	}
}
