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
    public class XML_VersionTwo : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string consumeNodeName = "consumption";
        private const string pollutionNodeName = "pollution";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public override void readXML(XmlDocument doc)
        {
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
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
            attribute.Value = "2";
            rootNode.Attributes.Append(attribute);
            xmlDoc.AppendChild(rootNode);

            XmlNode popNode = xmlDoc.CreateElement(popNodeName);
            XmlNode consumeNode = xmlDoc.CreateElement(consumeNodeName);
            XmlNode pollutionNode = xmlDoc.CreateElement(pollutionNodeName);

            try
            {
                makeNodes(xmlDoc, "ResidentialLow", DataStore.residentialLow, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "ResidentialHigh", DataStore.residentialHigh, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "CommercialLow", DataStore.commercialLow, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "CommercialHigh", DataStore.commercialHigh, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "Office", DataStore.office, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "Industry", DataStore.industry, popNode, consumeNode, pollutionNode);

                makeNodes(xmlDoc, "IndustryFarm", DataStore.industry_farm, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "IndustryForest", DataStore.industry_forest, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "IndustryOre", DataStore.industry_ore, popNode, consumeNode, pollutionNode);
                makeNodes(xmlDoc, "IndustryOil", DataStore.industry_oil, popNode, consumeNode, pollutionNode);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }
            rootNode.AppendChild(popNode);
            rootNode.AppendChild(consumeNode);
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
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        public void makeNodes(XmlDocument xmlDoc, String buildingType, int[][] array, XmlNode rootPopNode, XmlNode consumNode, XmlNode pollutionNode)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                makeNodes(xmlDoc, buildingType, array[i], i, rootPopNode, consumNode, pollutionNode);
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
        public void makeNodes(XmlDocument xmlDoc, String buildingType, int[] array, int level, XmlNode rootPopNode, XmlNode consumNode, XmlNode pollutionNode)
        {
            rootPopNode.AppendChild(makePopNode(xmlDoc, buildingType, level, array[DataStore.PEOPLE]));
            consumNode.AppendChild(makeConsumeNode(xmlDoc, buildingType, level, array[DataStore.POWER], array[DataStore.WATER], array[DataStore.SEWAGE],
                                                    array[DataStore.GARBAGE], array[DataStore.INCOME]));
            pollutionNode.AppendChild(makePollutionNode(xmlDoc, buildingType, level, array[DataStore.GROUND_POLLUTION], array[DataStore.NOISE_POLLUTION]));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlNode makePopNode(XmlDocument xmlDoc, String buildingType, int level, int value)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("modifier");
            attribute.Value = Convert.ToString(value);
            node.Attributes.Append(attribute);

            return node;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="power"></param>
        /// <param name="water"></param>
        /// <param name="sewage"></param>
        /// <param name="garbage"></param>
        /// <param name="wealth"></param>
        /// <returns></returns>
        public XmlNode makeConsumeNode(XmlDocument xmlDoc, String buildingType, int level, int power, int water, int sewage, int garbage, int wealth)
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
            return node;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="ground"></param>
        /// <param name="noise"></param>
        /// <returns></returns>
        public XmlNode makePollutionNode(XmlDocument xmlDoc, String buildingType, int level, int ground, int noise)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("ground");
            attribute.Value = Convert.ToString(ground);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("noise");
            attribute.Value = Convert.ToString(noise);
            node.Attributes.Append(attribute);

            return node;
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
                    string[] attr = node.Name.Split(new char[] {'_'});
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
                    string[] attr = node.Name.Split(new char[] {'_'});
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int power = Convert.ToInt32(node.Attributes["power"].InnerText);
                    int water = Convert.ToInt32(node.Attributes["water"].InnerText);
                    int sewage = Convert.ToInt32(node.Attributes["sewage"].InnerText);
                    int garbage = Convert.ToInt32(node.Attributes["garbage"].InnerText);
                    int wealth = Convert.ToInt32(node.Attributes["wealth"].InnerText);

                    switch (name)
                    {
                        case "ResidentialLow":
                            setConsumptionRates(DataStore.residentialLow[level], power, water, sewage, garbage, wealth);
                            break;

                        case "ResidentialHigh":
                            setConsumptionRates(DataStore.residentialHigh[level], power, water, sewage, garbage, wealth);
                            break;

                        case "CommercialLow":
                            setConsumptionRates(DataStore.commercialLow[level], power, water, sewage, garbage, wealth);
                            break;

                        case "CommercialHigh":
                            setConsumptionRates(DataStore.commercialHigh[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Office":
                            setConsumptionRates(DataStore.office[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry":
                            setConsumptionRates(DataStore.industry[level], power, water, sewage, garbage, wealth);
                            break;

                        case "IndustryOre":
                            setConsumptionRates(DataStore.industry_ore[level], power, water, sewage, garbage, wealth);
                            break;

                        case "IndustryOil":
                            setConsumptionRates(DataStore.industry_oil[level], power, water, sewage, garbage, wealth);
                            break;

                        case "IndustryForest":
                            setConsumptionRates(DataStore.industry_forest[level], power, water, sewage, garbage, wealth);
                            break;

                        case "IndustryFarm":
                            setConsumptionRates(DataStore.industry_farm[level], power, water, sewage, garbage, wealth);
                            break;
                    }
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
                try
                {
                    string[] attr = node.Name.Split(new char[] {'_'});
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int modifier = Convert.ToInt32(node.Attributes["modifier"].InnerText);

                    switch (name)
                    {
                        case "ResidentialLow":
                            DataStore.residentialLow[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "ResidentialHigh":
                            DataStore.residentialHigh[level][ DataStore.PEOPLE] = modifier;
                            break;

                        case "CommercialLow":
                            DataStore.commercialLow[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "CommercialHigh":
                            DataStore.commercialHigh[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "Office":
                            DataStore.office[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "Industry":
                            DataStore.industry[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "IndustryOre":
                            DataStore.industry_ore[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "IndustryOil":
                            DataStore.industry_oil[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "IndustryForest":
                            DataStore.industry_forest[level][DataStore.PEOPLE] = modifier;
                            break;

                        case "IndustryFarm":
                            DataStore.industry_farm[level][DataStore.PEOPLE] = modifier;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debugging.panelMessage("readPopulationNode: " + e.Message);
                }
            }
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