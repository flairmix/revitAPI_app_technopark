using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app_FacadePanelsInfo
{
    public class OrientedWall
    {
        private Element _wall;
        private bool _isOrientationMatchesSurfaceOrientation;
        private PlanarFace _planarFace;
        private double _angleTrueNorth;
        private double _pitchValue;
        private double _angleToTrueNorth;
        private bool _isEastSideFace;
        private bool _isValidObject;
        private bool _isClerestory;
        private string _faceOrientationCardial;
        private double _faceArea;
        private XYZ _faceNormal;
        private XYZ _trueNorthVector;

        public string temp;

        IDictionary<double, string> CardinalPoint = new Dictionary<double, string>() {
            { 0.0 , "N" },
            { 45.0 , "NE" },
            { 90.0  , "E" },
            { 135.0  , "SE" },
            { 180.0  , "S" },            
            { 225.0 , "SW" },
            { 270.0  , "W" },
            { 315.0  , "NW" }
        };

        /// <summary>
        /// <para> angleTrueNorth: angle in 0-360; </para>
        /// <para> pitchValue: 0.0-1.0; </para>
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="angleTrueNorth"></param>
        /// <param name="pitchValue"></param>
        /// <param name="isOrientationMatchesSurfaceOrientation"></param>
        public OrientedWall(Element wall,
            double angleTrueNorth, 
            double pitchValue,
            bool isOrientationMatchesSurfaceOrientation = true)
        {
            _wall = wall;
            _isOrientationMatchesSurfaceOrientation = isOrientationMatchesSurfaceOrientation;
            FindBiggestPlanarFace(_isOrientationMatchesSurfaceOrientation);

            if (_planarFace != null)
            {
                _isEastSideFace = _planarFace.FaceNormal.X > 0.0;
                _angleTrueNorth = angleTrueNorth;
                _pitchValue = pitchValue;
                GetAngleToTrueNorth();
                _isClerestory = _planarFace.FaceNormal.Z > pitchValue;
                _faceArea = _planarFace.Area;
                _faceNormal = _planarFace.FaceNormal.Normalize();

                if (_isClerestory)
                {
                    _faceOrientationCardial = "F";
                } else
                {
                    DefineCardinalPoint();
                }

                _isValidObject = true;
            }
            else {
                _isValidObject = false;
            }

        }
        public bool IsValidObject
        {
            get => _isValidObject;
        }
        public double AngleToTrueNorth
        {
            get => _angleToTrueNorth;
        }
        public bool IsClerestory
        {
            get => _isClerestory;
        }            
        public string FaceOrientationCardial
        {
            get => _faceOrientationCardial;
        }              
        public double FaceArea
        {
            get => _faceArea;
        }           
        public XYZ FaceNormal
        {
            get => _faceNormal;
        }         
        public XYZ TrueNorthVector
        {
            get => _trueNorthVector;
        }        


        /// <summary>
        /// Finding biggest Planar Face in a Wall element, considering orientation relatively surface
        /// </summary>
        /// <param name="isOrientationMatchesSurfaceNeed"></param>
        private void FindBiggestPlanarFace(bool isOrientationMatchesSurfaceNeed = true)
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
            if (biggestOne != null && biggestOne.ToString() == "Autodesk.Revit.DB.PlanarFace")
            {
                _planarFace = biggestOne as PlanarFace;
            }
            else {
                _planarFace =  null;
            }
         }

        private void GetAngleToTrueNorth()
        {
            XYZ planarFace_XY = new XYZ(_planarFace.FaceNormal.X, _planarFace.FaceNormal.Y, 0.0);
            XYZ projNorth_XY = new XYZ(0.0, 1.0, 0.0);

            _trueNorthVector = new XYZ(Math.Cos((_angleTrueNorth%90) * 3.141592 / 180), Math.Sin((_angleTrueNorth%90) * 3.141592 / 180), 0.0);

            double angleToProjNorth_XY = (short)(UnitUtils.ConvertFromInternalUnits(planarFace_XY.AngleTo(projNorth_XY), UnitTypeId.Degrees));

            _angleToTrueNorth = (short)(UnitUtils.ConvertFromInternalUnits(planarFace_XY.AngleTo(_trueNorthVector), UnitTypeId.Degrees));






        }

        private void DefineCardinalPoint()
        {
            IDictionary<double, string> anglesToCardinal = new Dictionary<double, string>();
            
            foreach (KeyValuePair<double, string> angle_orientation in CardinalPoint)
            {
                anglesToCardinal[ Math.Abs(angle_orientation.Key - _angleToTrueNorth)] =  angle_orientation.Value ;
            }

            double closestAngle = anglesToCardinal.Keys.Min();

            _faceOrientationCardial = anglesToCardinal[closestAngle];
        }






    }
}
