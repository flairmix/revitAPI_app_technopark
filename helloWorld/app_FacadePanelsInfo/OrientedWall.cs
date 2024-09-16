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
        private double _angleToProjectNorth;
        private double _angleToTrueNorth;
        private bool _isValidObject;
        private bool _isClerestory;
        private string _faceOrientationCardial;
        private double _faceArea;
        private XYZ _faceNormal;
        private XYZ _trueNorthVector;
        private double _anglesToCardinal;
        private IDictionary<string, XYZ> _cardinalVectors = new Dictionary<string, XYZ>();

        //curve walls
        private RevolvedFace _revolvedFace;
        private RuledFace _ruledFace;


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
            _angleTrueNorth = angleTrueNorth;
            _isOrientationMatchesSurfaceOrientation = isOrientationMatchesSurfaceOrientation;
            _pitchValue = pitchValue;
            _trueNorthVector = GetTrueNorthXYZ(_angleTrueNorth);
            DefineCardinalVectors();

            FindBiggestFace(_isOrientationMatchesSurfaceOrientation);

            if (_planarFace != null)
            {
                _angleToProjectNorth = GetAngleToProjectNorth();
                _angleToTrueNorth = GetAngleToTrueNorth();
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

            else if (_revolvedFace != null) { 
                // get Mesh and calculate sum of normal vectors 
                



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
        public double AnglesToCardinal
        {
            get => _anglesToCardinal;
        }           
        
        public IDictionary<string, XYZ> CardinalVectors
        {
            get => _cardinalVectors;
        }        


        /// <summary>
        /// Finding biggest Planar Face in a Wall element, considering orientation relatively surface
        /// </summary>
        /// <param name="isOrientationMatchesSurfaceNeed"></param>
        private void FindBiggestFace(bool isOrientationMatchesSurfaceNeed = true)
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
            else if (biggestOne != null && biggestOne.ToString() == "Autodesk.Revit.DB.RevolvedFace")
            {
                _revolvedFace = biggestOne as RevolvedFace;
            }            
            else if (biggestOne != null && biggestOne.ToString() == "Autodesk.Revit.DB.RuledFace")
            {
                _ruledFace = biggestOne as RuledFace;
            }
            else {
                _planarFace =  null;
                _revolvedFace =  null;
                _ruledFace =  null;
            }
         }


        private XYZ GetTrueNorthXYZ(double angleTrueNorth)
        {
            return new XYZ(Math.Cos((angleTrueNorth % 90) * 3.141592 / 180), Math.Sin((angleTrueNorth % 90) * 3.141592 / 180), 0.0);
        }


        private void DefineCardinalVectors()
        {
            IList<string> CardinalNames = new List<string>()
            {
                "NW", "W", "SW", "S", "SE","E", "NE"
            };

            _cardinalVectors["N"] = _trueNorthVector;

            double prev_X = _trueNorthVector.X;
            double prev_Y = _trueNorthVector.Y;
            double angleToRotate = 0.785398;

            foreach (string cardinalName in CardinalNames)
            {
                double rotatedX = prev_X * Math.Cos(angleToRotate) - prev_Y * Math.Sin(angleToRotate);
                double rotatedY = prev_X * Math.Sin(angleToRotate) + prev_Y * Math.Cos(angleToRotate);

                _cardinalVectors[cardinalName] = new XYZ(rotatedX, rotatedY, 0.0);

                prev_X = rotatedX;
                prev_Y = rotatedY;
            }
        }


        private double GetAngleToProjectNorth()
        {
            XYZ planarFace_XY = new XYZ(_planarFace.FaceNormal.X, _planarFace.FaceNormal.Y, 0.0);
            XYZ projNorth_XY = new XYZ(0.0, 1.0, 0.0);
            return (short)(UnitUtils.ConvertFromInternalUnits(planarFace_XY.AngleTo(projNorth_XY), UnitTypeId.Degrees));
        }


        private double GetAngleToTrueNorth()
        {
           XYZ planarFace_XY = new XYZ(_planarFace.FaceNormal.X, _planarFace.FaceNormal.Y, 0.0);
           return (short)(UnitUtils.ConvertFromInternalUnits(planarFace_XY.AngleTo(_trueNorthVector), UnitTypeId.Degrees));
        }


        private void DefineCardinalPoint()
        {
            IDictionary<double, string> anglesToCardinal = new Dictionary<double, string>();
            XYZ planarFace_XY = new XYZ(_planarFace.FaceNormal.X, _planarFace.FaceNormal.Y, 0.0);

            foreach (KeyValuePair<string, XYZ> cardinalXYZ in _cardinalVectors)
            {
                double angleToCardinal = (short)(UnitUtils.ConvertFromInternalUnits(planarFace_XY.AngleTo(cardinalXYZ.Value), UnitTypeId.Degrees));
                
                anglesToCardinal[angleToCardinal] = cardinalXYZ.Key;
            }

            _anglesToCardinal = anglesToCardinal.Keys.Min();
            _faceOrientationCardial = anglesToCardinal[_anglesToCardinal];
        }

        private void GetRevolvedFaceNormal()
        {

        }





    }
}
