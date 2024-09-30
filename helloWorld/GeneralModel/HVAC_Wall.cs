using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralModel
{
    public class HVAC_Wall
    {
        private Document _doc;
        private Wall _wall;
        private XYZ _centroid;
        private Space _spaceOfWall;

        public HVAC_Wall(Document doc, Wall wall
            )
        {
            _doc = doc;
            _wall = wall;
            ComputeCentroid();
        }


        public Space SpaceOfWall
        {
            get => _spaceOfWall;
            set
            {
            _spaceOfWall = value;
            }
        }



        private void ComputeCentroid()
        {
            Options opt = new Options();
            opt.DetailLevel = ViewDetailLevel.Coarse;
            Solid wallSolid = null;

            try
            {
                foreach (GeometryObject obj in _wall.get_Geometry(opt))
                {
                    Solid s = obj as Solid;
                    if (s != null)
                    {
                        wallSolid = s;
                        break;
                    }
                }
                if (wallSolid != null)
                {
                    _centroid = wallSolid.ComputeCentroid();
                }
            }
            catch (Exception ex) { 
            }
        }


        public void FindSpaceAround(Document _doc,
                                XYZ point,
                                double radius_ft,
                                StreamWriter logs,
                                Phase phase,
                                int radiusFindingStep_ft = 3,
                                double angleOfRotate = Math.PI / 2)
        {
            try
            {
                if (_doc.GetSpaceAtPoint(point, phase) != null)
                {
                    SpaceOfWall = _doc.GetSpaceAtPoint(point, phase);
                }

                for (int radiusDelta = 1; radiusDelta < (int)radius_ft; radiusDelta += radiusFindingStep_ft)
                {
                    XYZ localVector = new XYZ(0.0, 1.0, 0.0).Multiply(radiusDelta);

                    for (double angleDelta = 0; angleDelta < Math.PI * 2; angleDelta += angleOfRotate)
                    {
                        // counter clockwise rotation of vector
                        XYZ localVectorRotated = new XYZ((localVector.X * Math.Cos(angleDelta) - localVector.Y * Math.Sin(angleDelta)),
                                                        (localVector.X * Math.Sin(angleDelta) + localVector.Y * Math.Cos(angleDelta)),
                                                        localVector.Z
                                                        );

                        if (_doc.GetSpaceAtPoint(point.Add(localVectorRotated), phase) != null)
                        {
                            SpaceOfWall = _doc.GetSpaceAtPoint(point.Add(localVectorRotated), phase);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }


    }

}
