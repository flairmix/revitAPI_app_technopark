using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace ApplicationNamespace
{
    public class gbXML_Model
    {

        XmlDocument _doc;

        public gbXML_Model(string path)
        {
            _doc = new XmlDocument();
            _doc.LoadXml(path);

        }







    }
}
