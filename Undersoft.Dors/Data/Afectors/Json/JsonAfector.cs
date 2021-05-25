using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Linq;
using System.Dors.Data;

namespace System.Dors.Data.Afectors.Json
{
    public class JsonAfector
    {
        public event EventHandler Ehandle = delegate { };
        private DataTrellis jsonUniversalTable;

        public JsonAfector()
        {
            Ehandle(this, EventArgs.Empty);
        }

        public DataTrellis GetJsonTrellis(string TrellName, string JsonString, string method)
        {
            jsonUniversalTable = new DataTrellis(TrellName);
            var dejsons = JsonParser.FromJson(JsonString.Trim());

            foreach (KeyValuePair<string, object> dejson in dejsons)
            {
                if (dejson.Key == "params")
                {
                    int level = 1;
                    List<object> trylist = convertObjectBackToList(dejson.Value);
                    if (trylist.Count > 0)
                    {
                        foreach (object dd in trylist)
                        {
                            Dictionary<string, object> me = convertObjectBackToDict(dd, jsonUniversalTable, method, level, null);
                            Dictionary<string, string> colectrow = new Dictionary<string, string>();
                            foreach (KeyValuePair<string, object> blink in me)
                            {

                                if (blink.Key != null)
                                {
                                    if (!jsonUniversalTable.Pylons.Have(blink.Key))
                                    {
                                        addTableColumn(jsonUniversalTable, blink.Key, "System.String");
                                    }
                                    colectrow.Add(blink.Key, blink.Value.ToString());
                                }
                            }

                            if (!jsonUniversalTable.Pylons.Have("LEVEL"))
                            {
                                addTableColumn(jsonUniversalTable, "LEVEL", "System.String");
                            }
                            colectrow.Add("LEVEL", level.ToString());

                            DataTier tier = new DataTier(jsonUniversalTable);

                            StringBuilder sb = new StringBuilder();

                            foreach (KeyValuePair<string, string> readyrow in colectrow)
                            {
                                tier[readyrow.Key] = readyrow.Value.ToString();
                                sb.Append(readyrow.Key + " : " + readyrow.Value.ToString());
                            }

                            jsonUniversalTable.Tiers.Add(tier);
                            colectrow.Clear();
                        }
                    }
                    else
                    {
                        Dictionary<string, object> me = convertObjectBackToDict(dejson.Value, jsonUniversalTable, method, level, null);
                    }
                }
            }

            return jsonUniversalTable;
        }

        private List<object> convertObjectBackToList(object Input)
        {
            if (Input is IEnumerable<object>)
                return ((IEnumerable<object>)Input).Cast<Object>().ToList();
            return new List<Object>() { Input };
        }
        private Dictionary<string, object> convertObjectBackToDict(object obj, DataTrellis resultTable, string method, int level, string index2)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(obj.GetType()))
            {
                IDictionary<string, object> idict = (IDictionary<string, object>)obj;
                Dictionary<string, object> newDict = new Dictionary<string, object>();
                IDictionary<string, object> nextidict = (IDictionary<string, object>)obj;

                foreach (object key in idict.Keys)
                {
                    if (objectToType(idict[key.ToString()]) != "&objectAgain")
                    {
                        newDict.Add(objectToType(key), objectToType(idict[key.ToString()]));
                    }
                    else
                    {
                        List<object> deresult = convertObjectBackToList(idict[key.ToString()]);
                        if (deresult.Count > 0)
                        {
                            foreach (object dl in deresult)
                            {
                                nextidict = convertObjectBackToDict(dl, resultTable, method, level + 1, index2);
                                Dictionary<string, string> colectrow = new Dictionary<string, string>();

                                foreach (KeyValuePair<string, object> blink in newDict)
                                {
                                    if (blink.Key != null)
                                    {
                                        if (!jsonUniversalTable.Pylons.Have(blink.Key))
                                        {
                                            addTableColumn(resultTable, blink.Key, "System.String");
                                        }

                                        colectrow.Add(blink.Key, blink.Value.ToString());
                                    }
                                }

                                foreach (KeyValuePair<string, object> blink in nextidict)
                                {
                                    if (!jsonUniversalTable.Pylons.Have(blink.Key))
                                    {
                                        addTableColumn(resultTable, blink.Key, "System.String");
                                    }
                                    colectrow.Add(blink.Key, blink.Value.ToString());
                                }

                                if (!jsonUniversalTable.Pylons.Have("LEVEL"))
                                {
                                    addTableColumn(jsonUniversalTable, "LEVEL", "System.String");
                                }

                                colectrow.Add("LEVEL", (level + 1).ToString());

                                if (!jsonUniversalTable.Pylons.Have("INDEX2") && index2 != null)
                                {
                                    addTableColumn(jsonUniversalTable, "INDEX2", "System.String");
                                }

                                if (index2 != null)
                                {
                                    colectrow.Add("INDEX2", index2.ToString());
                                }

                                DataTier tier = new DataTier(resultTable);

                                foreach (KeyValuePair<string, string> readyrow in colectrow)
                                {
                                    tier[readyrow.Key] = readyrow.Value.ToString();
                                }

                                resultTable.Tiers.Add(tier);
                                colectrow.Clear();
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<string, object> blink in convertObjectBackToDict(idict[key.ToString()], resultTable, method, level, index2))
                            {
                                newDict.Add(blink.Key, blink.Value.ToString());
                            }
                        }
                    }
                }
                return newDict;
            }
            else if (typeof(IDictionary<string, string>).IsAssignableFrom(obj.GetType()))
            {
                return null;
            }
            else
            {
                Dictionary<string, object> newDict = new Dictionary<string, object>();
                newDict.Add("method", method.ToString());
                newDict.Add("result", obj.ToString());
                return newDict;
            }
        }
        private string objectToType(object obj)
        {
            string str = "";
            if (obj == null)
            {
                str = "";
            }
            else if (obj.GetType().FullName == "System.String")
            {
                str = (string)obj;
            }
            else if (obj.GetType().FullName == "System.Double")
            {
                str = obj.ToString();
            }
            else
            {
                str = "&objectAgain";
            }
            return str;
        }
        private void addTableColumn(DataTrellis trellis, string colname, string type)
        {
            trellis.Pylons.Add(new DataPylon(System.Type.GetType(type), colname));
        }
    }
}
