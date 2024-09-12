using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app_FacadePanelsInfo
{
    public class OrientWall
    {
        private Wall _wall;
        private bool _isOrientationMatchesSurfaceOrientation;
        private PlanarFace _planarFace;
        private double _angleTrueNorth;
        private double _pitchValue;
        private double _angleToTrueNorth;
        private bool _isPitchLikeWall;

        IDictionary<double, string> Orientation = new Dictionary<double, string>() {
            { 0.0 , "N" },
            { 45.0 , "NE" },
            { 90.0  , "E" },
            { 135.0  , "SE" },
            { 180.0  , "S" },            
            { -45.0 , "NW" },
            { -90.0  , "W" },
            { -135.0  , "WE" },
        };

        public OrientWall(Wall wall,
            bool isOrientationMatchesSurfaceOrientation, 
            bool isEastSideFace, 
            double angleTrueNorth, 
            double pitchValue)
        {
            _wall = wall;
            _isOrientationMatchesSurfaceOrientation = isOrientationMatchesSurfaceOrientation;
            _planarFace = FindBiggestPlanarFace(_isOrientationMatchesSurfaceOrientation);
            isEastSideFace = _planarFace.FaceNormal.X > 0.0;
            _angleTrueNorth = angleTrueNorth;
            _pitchValue = pitchValue;

        }

        public double AngleToTrueNorth
        {
            get => _angleToTrueNorth;
        }
        public bool IsPitchLikeWall
        {
            get =>_isPitchLikeWall;
        }

        private PlanarFace FindBiggestPlanarFace(bool isOrientationMatchesSurfaceNeed)
        {

            Options opt = new Options();
            opt.DetailLevel = ViewDetailLevel.Coarse;
            Solid wallSolid = null;

            foreach (GeometryObject obj in _wall.get_Geometry(opt))
            {
                Solid s = obj as Solid;
                if (s != null)
                {
                    wallSolid = s;
                }
            }

            // only for PlanarFace
            Face biggestOne = null;

            if (wallSolid != null)
            {
                foreach (Face face in wallSolid.Faces)
                {
                    if ((face.OrientationMatchesSurfaceOrientation == isOrientationMatchesSurfaceNeed) &&
                            (biggestOne == null || (Math.Round(biggestOne.Area, 5) <= Math.Round(face.Area, 5))))
                    {
                        biggestOne = face;
                    }
                }
            }
            if (biggestOne.ToString() == "Autodesk.Revit.DB.PlanarFace")
            {
                return biggestOne as PlanarFace;
            }
            else {
                return null;
            }
         }


    }
}
