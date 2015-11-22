using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using UnityEngine;
using ColossalFramework.Plugins;


namespace WG_BalancedPopMod
{
    public class XML_VersionFour : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string consumeNodeName = "consumption";
        private const string pollutionNodeName = "pollution";
        private const string productionNodeName = "production";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public override void readXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;
            try
            {
                DataStore.enableExperimental = Convert.ToBoolean(root.Attributes["experimental"].InnerText);
            }
            catch (Exception)
            {
                DataStore.enableExperimental = false;
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals(popNodeName))
                {
                    readPopulationNode(node);
                }
                else if (node.Name.Equals(consumeNodeName))
                {
                    readConsumptionNode(node);
                }
                else if (node.Name.Equals(pollutionNodeName))
                {
                    readPollutionNode(node);
                }
                else if (node.Name.Equals(productionNodeName))
                {
                    readProductionNode(node);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool writeXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CityMod");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "4";
            rootNode.Attributes.Append(attribute);
            /*
            attribute = xmlDoc.CreateAttribute("experimental");
            attribute.Value = DataStore.enableExperimental ? "true" : "false";
            rootNode.Attributes.Append(attribute);
            */
            xmlDoc.AppendChild(rootNode);

            XmlNode popNode = xmlDoc.CreateElement(popNodeName);
            XmlNode consumeNode = xmlDoc.CreateElement(consumeNodeName);
            XmlNode pollutionNode = xmlDoc.CreateElement(pollutionNodeName);
            XmlNode productionNode = xmlDoc.CreateElement(productionNodeName);

            try
            {
                makeNodes(xmlDoc, "ResidentialLow", DataStore.residentialLow, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "ResidentialHigh", DataStore.residentialHigh, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialLow", DataStore.commercialLow, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialHigh", DataStore.commercialHigh, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialLeisure", DataStore.commercialLeisure, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialTourist", DataStore.commercialTourist, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "Office", DataStore.office, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "Industry", DataStore.industry, popNode, consumeNode, pollutionNode, productionNode);

                makeNodes(xmlDoc, "IndustryFarm", DataStore.industry_farm, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryForest", DataStore.industry_forest, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryOre", DataStore.industry_ore, popNode, consumeNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryOil", DataStore.industry_oil, popNode, consumeNode, pollutionNode, productionNode);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }

            createPopulationNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(popNode);
            createConsumptionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(consumeNode);
            createProductionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(productionNode);
            createPollutionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(pollutionNode);

            if (File.Exists(fullPathFileName))
            {
                try
                {
                    if (File.Exists(fullPathFileName + ".bak"))
                    {
                        File.Delete(fullPathFileName + ".bak");
                    }

                    File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
                return false;  // Only time when we say there's an error
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createPopulationNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("space_pp = Square metres per person");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("level_height = Height of a floor. This is recommended to be left alone for balance unless the height of chimneys is taken into account");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("lvl_0 ... lvl_3 = Proportional values between each of the education levels (uneducated, educated, well educated, highly educated). Does not need to be percentages.");
            rootNode.AppendChild(comment);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createConsumptionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Consumption values are per household, or per production unit");
            rootNode.AppendChild(comment);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createPollutionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Ground pollution is not used by residential, commercial and offices.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("Noise pollution is not spread over the land by residential or offices.");
            rootNode.AppendChild(comment);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createProductionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Production for offices is number of employees per production unit.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("For industry, it used as hundredths of a unit per cell block.");
            rootNode.AppendChild(comment);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        private void makeNodes(XmlDocument xmlDoc, String buildingType, int[][] array, XmlNode rootPopNode, XmlNode consumNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                makeNodes(xmlDoc, buildingType, array[i], i, rootPopNode, consumNode, pollutionNode, productionNode);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="level"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        private void makeNodes(XmlDocument xmlDoc, String buildingType, int[] array, int level, XmlNode rootPopNode, XmlNode consumNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            makePopNode(rootPopNode, xmlDoc, buildingType, level, array);
            makeConsumeNode(consumNode, xmlDoc, buildingType, level, array[DataStore.POWER], array[DataStore.WATER], array[DataStore.SEWAGE], array[DataStore.GARBAGE], array[DataStore.INCOME]);
            makePollutionNode(pollutionNode, xmlDoc, buildingType, level, array[DataStore.GROUND_POLLUTION], array[DataStore.NOISE_POLLUTION]);
            makeProductionNode(productionNode, xmlDoc, buildingType, level, array[DataStore.PRODUCTION]);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="array"></param>
        private void makePopNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int[] array)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("space_pp");
            attribute.Value = Convert.ToString(transformPopulationModifier(buildingType, level, array[DataStore.PEOPLE], true));
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("level_height");
            attribute.Value = Convert.ToString(array[DataStore.LEVEL_HEIGHT]);
            node.Attributes.Append(attribute);

            if (array[DataStore.WORK_LVL0] >= 0)
            {
                attribute = xmlDoc.CreateAttribute("ground_mult");
                attribute.Value = Convert.ToString(array[DataStore.DENSIFICATION]);
                node.Attributes.Append(attribute);

                for (int i = 0; i < 4; i++)
                {
                    attribute = xmlDoc.CreateAttribute("lvl_" + i);
                    attribute.Value = Convert.ToString(array[DataStore.WORK_LVL0 + i]);
                    node.Attributes.Append(attribute);
                }
            }

            root.AppendChild(node);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="power"></param>
        /// <param name="water"></param>
        /// <param name="sewage"></param>
        /// <param name="garbage"></param>
        /// <param name="wealth"></param>
        private void makeConsumeNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int power, int water, int sewage, int garbage, int wealth)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("power");
            attribute.Value = Convert.ToString(power);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("water");
            attribute.Value = Convert.ToString(water);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("sewage");
            attribute.Value = Convert.ToString(sewage);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("garbage");
            attribute.Value = Convert.ToString(garbage);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("wealth");
            attribute.Value = Convert.ToString(wealth);
            node.Attributes.Append(attribute);

            root.AppendChild(node);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="ground"></param>
        /// <param name="noise"></param>
        private void makePollutionNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int ground, int noise)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("ground");
            attribute.Value = Convert.ToString(ground);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("noise");
            attribute.Value = Convert.ToString(noise);
            node.Attributes.Append(attribute);

            root.AppendChild(node);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="production"></param>
        private void makeProductionNode(XmlNode root, XmlDocument xmlDoc, string buildingType, int level, int production)
        {
            if (production >= 0)
            {
                XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("production");
                attribute.Value = Convert.ToString(production);
                node.Attributes.Append(attribute);

                root.AppendChild(node);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pollutionNode"></param>
        private void readPollutionNode(XmlNode pollutionNode)
        {
            string name = "";
            foreach (XmlNode node in pollutionNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int ground = Convert.ToInt32(node.Attributes["ground"].InnerText);
                    int noise = Convert.ToInt32(node.Attributes["noise"].InnerText);

                    switch (name)
                    {
                        case "ResidentialLow":
                            setPollutionRates(DataStore.residentialLow[level], ground, noise);
                            break;

                        case "ResidentialHigh":
                            setPollutionRates(DataStore.residentialHigh[level], ground, noise);
                            break;

                        case "CommercialLow":
                            setPollutionRates(DataStore.commercialLow[level], ground, noise);
                            break;

                        case "CommercialHigh":
                            setPollutionRates(DataStore.commercialHigh[level], ground, noise);
                            break;

                        case "CommercialTourist":
                            setPollutionRates(DataStore.commercialTourist[level], ground, noise);
                            break;

                        case "CommercialLeisure":
                            setPollutionRates(DataStore.commercialLeisure[level], ground, noise);
                            break;

                        case "Office":
                            setPollutionRates(DataStore.office[level], ground, noise);
                            break;

                        case "Industry":
                            setPollutionRates(DataStore.industry[level], ground, noise);
                            break;

                        case "IndustryOre":
                            setPollutionRates(DataStore.industry_ore[level], ground, noise);
                            break;

                        case "IndustryOil":
                            setPollutionRates(DataStore.industry_oil[level], ground, noise);
                            break;

                        case "IndustryForest":
                            setPollutionRates(DataStore.industry_forest[level], ground, noise);
                            break;

                        case "IndustryFarm":
                            setPollutionRates(DataStore.industry_farm[level], ground, noise);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debugging.panelMessage("readPollutionNode: " + name + " " + e.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumeNode"></param>
        private void readConsumptionNode(XmlNode consumeNode)
        {
            foreach (XmlNode node in consumeNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int power = Convert.ToInt32(node.Attributes["power"].InnerText);
                    int water = Convert.ToInt32(node.Attributes["water"].InnerText);
                    int sewage = Convert.ToInt32(node.Attributes["sewage"].InnerText);
                    int garbage = Convert.ToInt32(node.Attributes["garbage"].InnerText);
                    int wealth = Convert.ToInt32(node.Attributes["wealth"].InnerText);
                    int[] array = getArray(name, level, "readConsumptionNode");

                    setConsumptionRates(array, power, water, sewage, garbage, wealth);
                }
                catch (Exception e)
                {
                    Debugging.panelMessage("readConsumptionNode: " + e.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="popNode"></param>
        private void readPopulationNode(XmlNode popNode)
        {
            foreach (XmlNode node in popNode.ChildNodes)
            {
                string[] attr = node.Name.Split(new char[] { '_' });
                string name = attr[0];
                int level = Convert.ToInt32(attr[1]) - 1;
                int[] array = new int[12];

                try
                {
                    array = getArray(name, level, "readPopulationNode");
                    int temp = Convert.ToInt32(node.Attributes["level_height"].InnerText);
                    array[DataStore.LEVEL_HEIGHT] = temp > 0 ? temp : 10;

                    temp = Convert.ToInt32(node.Attributes["space_pp"].InnerText);
                    if (temp <= 0)
                    {
                        temp = 100;  // Bad person trying to give negative or div0 error. 
                    }
                    array[DataStore.PEOPLE] = transformPopulationModifier(name, level, temp, false);

                }
                catch (Exception e)
                {
                    Debugging.panelMessage("readPopulationNode, part a: " + e.Message);
                }

                if (!name.Contains("Residential"))
                {
                    try
                    {
                        int dense  = Convert.ToInt32(node.Attributes["ground_mult"].InnerText);
                        array[DataStore.DENSIFICATION] = dense >= 0 ? dense : 0;  // Force to be greater than 0

                        int level0 = Convert.ToInt32(node.Attributes["lvl_0"].InnerText);
                        int level1 = Convert.ToInt32(node.Attributes["lvl_1"].InnerText);
                        int level2 = Convert.ToInt32(node.Attributes["lvl_2"].InnerText);
                        int level3 = Convert.ToInt32(node.Attributes["lvl_3"].InnerText);

                        // Ensure all is there first
                        array[DataStore.WORK_LVL0] = level0;
                        array[DataStore.WORK_LVL1] = level1;
                        array[DataStore.WORK_LVL2] = level2;
                        array[DataStore.WORK_LVL3] = level3;
                    }
                    catch (Exception e)
                    {
                        Debugging.panelMessage("readPopulationNode, part b: " + e.Message);
                    }
                }
            } // end foreach
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <param name="toXML">Transformation into XML value</param>
        /// <returns></returns>
        private int transformPopulationModifier(string name, int level, int value, bool toXML)
        {
            int dividor = 1;

            switch (name)
            {
                case "ResidentialLow":
                case "ResidentialHigh":
                    dividor = 5;   // 5 people
                    break;
            }

            if (toXML)
            {
                return (value / dividor);
            }
            else
            {
                return (value * dividor);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="produceNode"></param>
        private void readProductionNode(XmlNode produceNode)
        {
            foreach (XmlNode node in produceNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int[] array = getArray(name, level, "readProductionNode");

                    array[DataStore.PRODUCTION] = Convert.ToInt32(node.Attributes["production"].InnerText);
                    if (array[DataStore.PRODUCTION] <= 0)
                    {
                        array[DataStore.PRODUCTION] = 1;
                    }
                }
                catch (Exception e)
                {
                    Debugging.panelMessage("readProductionNode: " + e.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <param name="callingFunction">For debug purposes</param>
        /// <returns></returns>
        private static int[] getArray(string name, int level, string callingFunction)
        {
            int[] array = new int[14];

            switch (name)
            {
                case "ResidentialLow":
                    array = DataStore.residentialLow[level];
                    break;

                case "ResidentialHigh":
                    array = DataStore.residentialHigh[level];
                    break;

                case "CommercialLow":
                    array = DataStore.commercialLow[level];
                    break;

                case "CommercialHigh":
                    array = DataStore.commercialHigh[level];
                    break;

                case "CommercialTourist":
                    array = DataStore.commercialTourist[level];
                    break;

                case "CommercialLeisure":
                    array = DataStore.commercialLeisure[level];
                    break;

                case "Office":
                    array = DataStore.office[level];
                    break;

                case "Industry":
                    array = DataStore.industry[level];
                    break;

                case "IndustryOre":
                    array = DataStore.industry_ore[level];
                    break;

                case "IndustryOil":
                    array = DataStore.industry_oil[level];
                    break;

                case "IndustryForest":
                    array = DataStore.industry_forest[level];
                    break;

                case "IndustryFarm":
                    array = DataStore.industry_farm[level];
                    break;

                default:
                    Debugging.panelMessage(callingFunction + ". unknown element name: " + name);
                    break;
            }
            return array;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="power"></param>
        /// <param name="water"></param>
        /// <param name="sewage"></param>
        /// <param name="garbage"></param>
        /// <param name="wealth"></param>
        private void setConsumptionRates(int[] p, int power, int water, int sewage, int garbage, int wealth)
        {
            p[DataStore.POWER] = power;
            p[DataStore.WATER] = water;
            p[DataStore.SEWAGE] = sewage;
            p[DataStore.GARBAGE] = garbage;
            p[DataStore.INCOME] = wealth;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ground"></param>
        /// <param name="noise"></param>
        private void setPollutionRates(int[] p, int ground, int noise)
        {
            p[DataStore.GROUND_POLLUTION] = ground;
            p[DataStore.NOISE_POLLUTION] = noise;
        }
    }
}