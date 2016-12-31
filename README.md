# Turf.Sharp
C# porting of Turf.js "A modular geospatial engine written in JavaScript http://turfjs.org/"

# Dependency

* GeoJSON.Net (https://github.com/GeoJSON-Net/GeoJSON.Net)
* Json.NET (https://github.com/JamesNK/Newtonsoft.Json)

# Porting status
Now porting is based on v 3.7.0 (https://github.com/Turfjs/turf/tree/v3.7.0)

## AGGREGATION
* collect

## MEASUREMENT
* along => done  
* area  
* bboxPolygon  
* bearing => done  
* center
* centroid
* destination => done
* distance => done
* envelope
* lineDistance
* midpoint
* pointOnSurface
* square

## TRANSFORMATION
* bezier
* buffer
* concave
* convex
* difference
* intersect
* simplify
* union

## MISC
* combine
* explode
* flip
* kinks
* lineSlice
* pointOnLine

## HELPER
* featureCollection
* feature
* lineString
* multiLineString
* point => done
* polygon
* multiPoint
* multiPolygon
* geometryCollection

## DATA
* random
* sample

## INTERPOLATION
* isolines
* planepoint
* tin => done

## JOINS
* inside => done
* tag => done

## GRIDS
* hexGrid
* pointGrid
* squareGrid
* triangleGrid
* within

## CLASSIFICATION
* nearest

## META
* propEach
* coordEach
* coordReduce
* featureEach
* getCoord

## ASSERTIONS
* featureOf
* collectionOf
* bbox
* centerOfMass
* circle
* index
* geojsonType
* lineSliceAlong
* propReduce
* coordAll
* geomEach
* tesselate
