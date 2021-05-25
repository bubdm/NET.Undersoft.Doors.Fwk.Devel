using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace System.Dors.Data.Afectors.Xml
{
    public static class XmlAfector
    {
        public static int CreateXml(this DataPage dataPage, Stream toStream, XmlSchema schemaxml = XmlSchema.Write)
        {
            return dataPage.PageSet.CreateXml(toStream, schemaxml);
        }
        public static int CreateXml(this DataSphere nset, Stream toStream, XmlSchema schemaxml = XmlSchema.Write)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec = xdoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement root = xdoc.DocumentElement;
            xdoc.InsertBefore(xdec, root);
            string nsetid = "";
            if (nset.SphereId != null)
                nsetid = nset.SphereId;
            else
                nsetid = "nsetxml";
            XmlElement ndata = xdoc.CreateElement(nsetid);
            xdoc.AppendChild(ndata);
            XmlElement config = xdoc.CreateElement("config");
            XmlElement menu = xdoc.CreateElement("path");
            menu.InnerText = nset.Config.Path.ToString();
            XmlElement store = xdoc.CreateElement("dataid");
            store.InnerText = nset.Config.DataId.ToString();
            XmlElement user = xdoc.CreateElement("dpotid");
            user.InnerText = nset.Config.DepotId.ToString();
            config.AppendChild(menu);
            config.AppendChild(store);
            config.AppendChild(user);
            ndata.AppendChild(config);

            if (schemaxml == XmlSchema.Write)
            {
                XmlElement schema = xdoc.CreateElement("schema");
                foreach (DataTrellis ntab in nset.Trells)
                {
                    XmlElement tableSchema = xdoc.CreateElement(ntab.TrellName);
                    schema.AppendChild(TableSchema(tableSchema, ntab));
                }
                ndata.AppendChild(schema);
            }
            XmlElement data = xdoc.CreateElement("data");
            int affected = 0;
            foreach (DataTrellis ntab in nset.Trells)
            {
                affected += TableData(data, ntab);
            }
            ndata.AppendChild(data);
            xdoc.Save(toStream);
            return affected;
        }
        public static int CreateXml(this DataTrellis ntab, Stream toStream, XmlSchema schemaxml = XmlSchema.Write)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec = xdoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement root = xdoc.DocumentElement;
            xdoc.InsertBefore(xdec, root);
            XmlElement ndata = xdoc.CreateElement("ntablexml");
            xdoc.AppendChild(ndata);
            if (schemaxml == XmlSchema.Write)
            {
                XmlElement schema = xdoc.CreateElement("schema");
                XmlElement tableSchema = xdoc.CreateElement(ntab.TrellName);
                schema.AppendChild(TableSchema(tableSchema, ntab));
                ndata.AppendChild(schema);
            }
            XmlElement data = xdoc.CreateElement("data");
            int affected = TableData(data, ntab);
            ndata.AppendChild(data);
            xdoc.Save(toStream);
            return affected;
        }
        public static int CreateXml(this DataPage dataPage, StringBuilder sb, XmlSchema schemaxml = XmlSchema.Write)
        {
            return dataPage.PageSet.CreateXml(sb, schemaxml);
        }
        public static int CreateXml(this DataSphere nset, StringBuilder sb, XmlSchema schemaxml = XmlSchema.Write)
        {
            StringWriter sw = new StringWriter(sb);
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec = xdoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement root = xdoc.DocumentElement;
            xdoc.InsertBefore(xdec, root);
            string nsetid = "";
            if (nset.SphereId != null)
                nsetid = nset.SphereId;
            else
                nsetid = "nsetxml";
            XmlElement ndata = xdoc.CreateElement(nsetid);
            xdoc.AppendChild(ndata);
            XmlElement config = xdoc.CreateElement("config");
            XmlElement menu = xdoc.CreateElement("path");
            menu.InnerText = nset.Config.Path.ToString();
            XmlElement store = xdoc.CreateElement("dataid");
            store.InnerText = nset.Config.DataId.ToString();
            XmlElement user = xdoc.CreateElement("dpotid");
            user.InnerText = nset.Config.DepotId.ToString();
            config.AppendChild(menu);
            config.AppendChild(store);
            config.AppendChild(user);
            ndata.AppendChild(config);

            if (schemaxml == XmlSchema.Write)
            {
                XmlElement schema = xdoc.CreateElement("schema");
                foreach (DataTrellis ntab in nset.Trells)
                {
                    XmlElement tableSchema = xdoc.CreateElement(ntab.TrellName);
                    schema.AppendChild(TableSchema(tableSchema, ntab));
                }
                ndata.AppendChild(schema);
            }
            XmlElement data = xdoc.CreateElement("data");
            int affected = 0;
            foreach (DataTrellis ntab in nset.Trells)
            {
                affected += TableData(data, ntab);
            }
            ndata.AppendChild(data);
            xdoc.Save(sw);
            return affected;
        }
        public static int CreateXml(this DataTrellis ntab, StringBuilder sb, XmlSchema schemaxml = XmlSchema.Write)
        {
            StringWriter sw = new StringWriter(sb);
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec = xdoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement root = xdoc.DocumentElement;
            xdoc.InsertBefore(xdec, root);
            XmlElement ndata = xdoc.CreateElement("ntablexml");
            xdoc.AppendChild(ndata);
            if (schemaxml == XmlSchema.Write)
            {
                XmlElement schema = xdoc.CreateElement("schema");
                XmlElement tableSchema = xdoc.CreateElement(ntab.TrellName);
                schema.AppendChild(TableSchema(tableSchema, ntab));
                ndata.AppendChild(schema);
            }
            XmlElement data = xdoc.CreateElement("data");
            int affected = TableData(data, ntab);
            ndata.AppendChild(data);
            xdoc.Save(sw);
            return affected;
        }

        public static XmlElement TableSchema(XmlElement _tableSchema, DataTrellis _ntab)
        {
            XmlElement tableSchema = _tableSchema;
            XmlElement columnSchema = tableSchema.OwnerDocument.CreateElement("columns");
            foreach (DataPylon nc in _ntab.Pylons)
            {
                XmlElement column = columnSchema.OwnerDocument.CreateElement("column");
                column.SetAttribute("type", nc.DataType.FullName);
                column.SetAttribute("name", nc.PylonName);
                column.SetAttribute("ordinal", nc.Ordinal.ToString());               
                columnSchema.AppendChild(column);
            }
            tableSchema.AppendChild(columnSchema);
            XmlElement keySchema = tableSchema.OwnerDocument.CreateElement("keys");
            foreach (DataPylon nc in _ntab.PrimeKey)
            {
                XmlElement key = keySchema.OwnerDocument.CreateElement("key");
                key.SetAttribute("type", nc.DataType.FullName);
                key.SetAttribute("name", nc.PylonName);
                key.SetAttribute("ordinal", nc.Ordinal.ToString());
                keySchema.AppendChild(key);
            }
            tableSchema.AppendChild(keySchema);
            XmlElement relaySchema = tableSchema.OwnerDocument.CreateElement("relays");
            XmlElement parentrelaySchema = tableSchema.OwnerDocument.CreateElement("parents");
            foreach (DataRelay nd in _ntab.ParentRelays)
            {
                XmlElement parent = tableSchema.OwnerDocument.CreateElement("parent");
                parent.SetAttribute("parentname", nd.Parent.Trell.TrellName);
                parent.SetAttribute("childname", nd.Child.Trell.TrellName);
                parent.SetAttribute("relayname", nd.RelayName);
                for (int i = 0; i < nd.Parent.RelayKeys.Count;i++)
                {
                    XmlElement relaykeySchema = tableSchema.OwnerDocument.CreateElement("relaykey");
                    XmlElement pkey = tableSchema.OwnerDocument.CreateElement("parentkey");
                    pkey.SetAttribute("type", nd.Parent.RelayKeys[i].DataType.FullName);
                    pkey.SetAttribute("name", nd.Parent.RelayKeys[i].PylonName);
                    pkey.SetAttribute("ordinal", nd.Parent.RelayKeys[i].Ordinal.ToString());
                    relaykeySchema.AppendChild(pkey);
                    XmlElement ckey = tableSchema.OwnerDocument.CreateElement("childkey");
                    ckey.SetAttribute("type", nd.Child.RelayKeys[i].DataType.FullName);
                    ckey.SetAttribute("name", nd.Child.RelayKeys[i].PylonName);
                    ckey.SetAttribute("ordinal", nd.Child.RelayKeys[i].Ordinal.ToString());
                    relaykeySchema.AppendChild(ckey);
                    parent.AppendChild(relaykeySchema);
                }               
                parentrelaySchema.AppendChild(parent);
            }
            relaySchema.AppendChild(parentrelaySchema);
            XmlElement childrelaySchema = tableSchema.OwnerDocument.CreateElement("childs");
            foreach (DataRelay nd in _ntab.ChildRelays)
            {
                XmlElement child = tableSchema.OwnerDocument.CreateElement("child");
                child.SetAttribute("parentname", nd.Parent.Trell.TrellName);
                child.SetAttribute("childname", nd.Child.Trell.TrellName);
                child.SetAttribute("relayname", nd.RelayName);
                for (int i = 0; i < nd.Child.RelayKeys.Count; i++)
                {
                    XmlElement relaykeySchema = tableSchema.OwnerDocument.CreateElement("relaykey");
                    XmlElement pkey = tableSchema.OwnerDocument.CreateElement("parentkey");
                    pkey.SetAttribute("type", nd.Parent.RelayKeys[i].DataType.FullName);
                    pkey.SetAttribute("name", nd.Parent.RelayKeys[i].PylonName);
                    pkey.SetAttribute("ordinal", nd.Parent.RelayKeys[i].Ordinal.ToString());
                    relaykeySchema.AppendChild(pkey);
                    XmlElement ckey = tableSchema.OwnerDocument.CreateElement("childkey");
                    ckey.SetAttribute("type", nd.Child.RelayKeys[i].DataType.FullName);
                    ckey.SetAttribute("name", nd.Child.RelayKeys[i].PylonName);
                    ckey.SetAttribute("ordinal", nd.Child.RelayKeys[i].Ordinal.ToString());
                    relaykeySchema.AppendChild(ckey);
                    child.AppendChild(relaykeySchema);
                }
                childrelaySchema.AppendChild(child);
            }
            relaySchema.AppendChild(childrelaySchema);
            tableSchema.AppendChild(relaySchema);
            return tableSchema;
        }

        public static int TableData(XmlElement _data, DataTrellis _ntab)
        {
            TableSingleton = CreateTableSingleton(_data, _ntab);
            object[][] rows = _ntab.Tiers.AsEnumerable().Select(p => p.DataArray.ToArray()).ToArray();
            int xLength = _ntab.Pylons.Count;                      
            int counter = 0;
            for (int i = 0; i < rows.Length;i++)
            {
                XmlNode singledata = TableSingleton.CloneNode(true);
                XmlNodeList nl = singledata.ChildNodes;
                for(int n = 0; n < xLength; n++)
                {
                    nl[n].InnerText = (rows[i][n] != null) ? rows[i][n].ToString() : "";
                }
                _data.AppendChild(singledata);
                counter++;              
            }
            return counter;
        }

        public static XmlNode TableSingleton;
        public static XmlNode CreateTableSingleton(XmlElement _data, DataTrellis _ntab)
        {
            XmlElement tableData = _data.OwnerDocument.CreateElement(_ntab.TrellName);           
            foreach (DataPylon nc in _ntab.Pylons.AsEnumerable())
            {
                string elemname = nc.PylonName.Replace("#", "_x0023_").Replace("=", "_x003D_");
                XmlElement column = tableData.OwnerDocument.CreateElement(elemname);
                tableData.AppendChild(column);
            }
            XmlNode node = tableData.Clone();
            return node;
        }

        public static string XmlString(this DataPage dataPage, string trellName = null)
        {
            StringBuilder sb = new StringBuilder();
            if (trellName != null)
            {
                DataTrellis trell = dataPage.PageSet.Trells.Collect(trellName);
                if (trell != null)
                {
                    trell.CreateXml(sb, XmlSchema.Write);
                    return sb.ToString();
                }
            }
            dataPage.PageSet.CreateXml(sb, XmlSchema.Write);
            return sb.ToString();
        }
        public static int XmlString(this DataPage dataPage, StringBuilder sb, XmlSchema schema = XmlSchema.Write, string trellName = null)
        {
            if (trellName != null)
            {
                DataTrellis trell = dataPage.PageSet.Trells.Collect(trellName);
                if (trell != null)
                    return trell.CreateXml(sb, schema);
            }
            return dataPage.PageSet.CreateXml(sb, schema);
        }

        public static string XmlString(this DataSphere PageSet, string trellName = null)
        {
            StringBuilder sb = new StringBuilder();
            if (trellName != null)
            {
                DataTrellis trell = PageSet.Trells.Collect(trellName);
                if (trell != null)
                {
                    trell.CreateXml(sb, XmlSchema.Write);
                    return sb.ToString();
                }
            }
            PageSet.CreateXml(sb, XmlSchema.Write);
            return sb.ToString();
        }
        public static int XmlString(this DataSphere PageSet, StringBuilder sb, XmlSchema schema = XmlSchema.Write, string trellName = null)
        {
            if (trellName != null)
            {
                DataTrellis trell = PageSet.Trells.Collect(trellName);
                if (trell != null)
                    return trell.CreateXml(sb, schema);
            }
            return PageSet.CreateXml(sb, schema);
        }

        public static string XmlString(this DataTrellis dataPage)
        {
            StringBuilder sb = new StringBuilder();
            dataPage.CreateXml(sb, XmlSchema.Write);
            return sb.ToString();
        }
        public static int XmlString(this DataTrellis dataPage, StringBuilder sb, XmlSchema schema = XmlSchema.Write, string trellName = null)
        {
            return dataPage.CreateXml(sb, schema);
        }

        public static MemoryStream XmlStream(this DataPage dataPage, string trellName = null)
        {
            MemoryStream ms = new MemoryStream();
            if (trellName != null)
            {
                DataTrellis trell = dataPage.PageSet.Trells.Collect(trellName);
                if (trell != null)
                {
                    trell.CreateXml(ms, XmlSchema.Write);
                    ms.Position = 0;
                    return ms;
                }            
            }
            dataPage.PageSet.CreateXml(ms, XmlSchema.Write);
            ms.Position = 0;
            return ms;
        }
        public static int XmlStream(this DataPage dataPage, Stream stream, XmlSchema schema = XmlSchema.Write, string trellName = null)
        {
            if (trellName != null)
            {
                DataTrellis trell = dataPage.PageSet.Trells.Collect(trellName);
                if (trell != null)
                    return trell.CreateXml(stream,schema);
            }
            return dataPage.PageSet.CreateXml(stream, schema);
        }

        public static MemoryStream XmlStream(this DataSphere PageSet, string trellName = null)
        {
            MemoryStream ms = new MemoryStream();
            if (trellName != null)
            {
                DataTrellis trell = PageSet.Trells.Collect(trellName);
                if (trell != null)
                {
                    trell.CreateXml(ms, XmlSchema.Write);
                    ms.Position = 0;
                    return ms;
                }
            }
            PageSet.CreateXml(ms, XmlSchema.Write);
            ms.Position = 0;
            return ms;
        }
        public static int XmlStream(this DataSphere PageSet, Stream stream, XmlSchema schema = XmlSchema.Write, string trellName = null)
        {
            if (trellName != null)
            {
                DataTrellis trell = PageSet.Trells.Collect(trellName);
                if (trell != null)
                    return trell.CreateXml(stream, schema);
            }
            return PageSet.CreateXml(stream, schema);
        }

        public static MemoryStream XmlStream(this DataTrellis PageSet)
        {
            MemoryStream ms = new MemoryStream();
            PageSet.CreateXml(ms, XmlSchema.Write);
            ms.Position = 0;
            return ms;
        }
        public static int XmlStream(this DataTrellis PageSet, Stream stream, XmlSchema schema = XmlSchema.Write)
        {
            return PageSet.CreateXml(stream, schema);
        }
    }   
    
    public enum XmlSchema
    {
        Write,
        None
    }
}
