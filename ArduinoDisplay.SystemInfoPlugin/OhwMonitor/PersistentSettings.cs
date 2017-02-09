﻿namespace ArduinoDisplay.SystemInfoPlugin.OhwMonitor
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    using OpenHardwareMonitor.Hardware;

    /// <summary>
    /// The my settings.
    /// </summary>
    public class PersistentSettings : ISettings
    {

        private IDictionary<string, string> settings =
          new Dictionary<string, string>();

        public void Load(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                try
                {
                    File.Delete(fileName);
                }
                catch { }

                string backupFileName = fileName + ".backup";
                try
                {
                    doc.Load(backupFileName);
                }
                catch
                {
                    try
                    {
                        File.Delete(backupFileName);
                    }
                    catch { }

                    return;
                }
            }

            XmlNodeList list = doc.GetElementsByTagName("appSettings");
            foreach (XmlNode node in list)
            {
                XmlNode parent = node.ParentNode;
                if (parent != null && parent.Name == "configuration" &&
                  parent.ParentNode is XmlDocument)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.Name == "add")
                        {
                            XmlAttributeCollection attributes = child.Attributes;
                            XmlAttribute keyAttribute = attributes["key"];
                            XmlAttribute valueAttribute = attributes["value"];
                            if (keyAttribute != null && valueAttribute != null &&
                              keyAttribute.Value != null)
                            {
                                this.settings.Add(keyAttribute.Value, valueAttribute.Value);
                            }
                        }
                    }
                }
            }
        }

        public void Save(string fileName)
        {

            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement configuration = doc.CreateElement("configuration");
            doc.AppendChild(configuration);
            XmlElement appSettings = doc.CreateElement("appSettings");
            configuration.AppendChild(appSettings);
            foreach (KeyValuePair<string, string> keyValuePair in this.settings)
            {
                XmlElement add = doc.CreateElement("add");
                add.SetAttribute("key", keyValuePair.Key);
                add.SetAttribute("value", keyValuePair.Value);
                appSettings.AppendChild(add);
            }

            byte[] file;
            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory, Encoding.UTF8))
                {
                    doc.Save(writer);
                }
                file = memory.ToArray();
            }

            string backupFileName = fileName + ".backup";
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(backupFileName);
                }
                catch { }
                try
                {
                    File.Move(fileName, backupFileName);
                }
                catch { }
            }

            using (var stream = new FileStream(fileName,
              FileMode.Create, FileAccess.Write))
            {
                stream.Write(file, 0, file.Length);
            }

            try
            {
                File.Delete(backupFileName);
            }
            catch { }
        }

        public bool Contains(string name)
        {
            return this.settings.ContainsKey(name);
        }

        public void SetValue(string name, string value)
        {
            this.settings[name] = value;
        }

        public string GetValue(string name, string value)
        {
            string result;
            if (this.settings.TryGetValue(name, out result))
                return result;
            else
                return value;
        }

        public void Remove(string name)
        {
            this.settings.Remove(name);
        }

        public void SetValue(string name, int value)
        {
            this.settings[name] = value.ToString();
        }

        public int GetValue(string name, int value)
        {
            string str;
            if (this.settings.TryGetValue(name, out str))
            {
                int parsedValue;
                if (int.TryParse(str, out parsedValue))
                    return parsedValue;
                else
                    return value;
            }
            else
            {
                return value;
            }
        }

        public void SetValue(string name, float value)
        {
            this.settings[name] = value.ToString(CultureInfo.InvariantCulture);
        }

        public float GetValue(string name, float value)
        {
            string str;
            if (this.settings.TryGetValue(name, out str))
            {
                float parsedValue;
                if (float.TryParse(str, NumberStyles.Float,
                  CultureInfo.InvariantCulture, out parsedValue))
                    return parsedValue;
                else
                    return value;
            }
            else
            {
                return value;
            }
        }

        public void SetValue(string name, bool value)
        {
            this.settings[name] = value ? "true" : "false";
        }

        public bool GetValue(string name, bool value)
        {
            string str;
            if (this.settings.TryGetValue(name, out str))
            {
                return str == "true";
            }
            else
            {
                return value;
            }
        }

        public void SetValue(string name, Color color)
        {
            this.settings[name] = color.ToArgb().ToString("X8");
        }

        public Color GetValue(string name, Color value)
        {
            string str;
            if (this.settings.TryGetValue(name, out str))
            {
                int parsedValue;
                if (int.TryParse(str, NumberStyles.HexNumber,
                  CultureInfo.InvariantCulture, out parsedValue))
                    return Color.FromArgb(parsedValue);
                else
                    return value;
            }
            else
            {
                return value;
            }
        }
    }
}