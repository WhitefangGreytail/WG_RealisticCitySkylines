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
    /// <summary>
    ///
    /// </summary>
    [Obsolete]
    public class XML_VersionOne : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string consumeNodeName = "consumption";


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
            }
        }

        public override bool writeXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_PopulationMod");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "1";
            rootNode.Attributes.Append(attribute);

            xmlDoc.AppendChild(rootNode);

            XmlNode populationNode = xmlDoc.CreateElement(popNodeName);
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialLow_Level1", DataStore.residentialLow[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialLow_Level2", DataStore.residentialLow[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialLow_Level3", DataStore.residentialLow[2][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialLow_Level4", DataStore.residentialLow[3][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialLow_Level5", DataStore.residentialLow[4][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialHigh_Level1", DataStore.residentialHigh[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialHigh_Level2", DataStore.residentialHigh[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialHigh_Level3", DataStore.residentialHigh[2][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialHigh_Level4", DataStore.residentialHigh[3][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "ResidentialHigh_Level5", DataStore.residentialHigh[4][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialLow_Level1", DataStore.commercialLow[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialLow_Level2", DataStore.commercialLow[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialLow_Level3", DataStore.commercialLow[2][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialHigh_Level1", DataStore.commercialHigh[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialHigh_Level2", DataStore.commercialHigh[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "CommercialHigh_Level3", DataStore.commercialHigh[2][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "Office_Level1", DataStore.office[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Office_Level2", DataStore.office[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Office_Level3", DataStore.office[2][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Level1", DataStore.industry[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Level2", DataStore.industry[1][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Level3", DataStore.industry[2][DataStore.PEOPLE]));

            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Farming", DataStore.industry_farm[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Forestry", DataStore.industry_forest[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Ore", DataStore.industry_ore[0][DataStore.PEOPLE]));
            populationNode.AppendChild(makePopNode(xmlDoc, "Industrial_Oil", DataStore.industry_oil[0][DataStore.PEOPLE]));

            rootNode.AppendChild(populationNode);

            XmlNode consumptionNode = xmlDoc.CreateElement(consumeNodeName);

            int[][] array = DataStore.residentialLow;
            for (int i = 0; i < 5; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "ResidentialLow", i + 1, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.residentialHigh;
            for (int i = 0; i < 5; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "ResidentialHigh", i + 1, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.office;
            for (int i = 0; i < 3; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Office", i + 1, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.industry;
            for (int i = 0; i < 3; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Industry_Generic", i + 1, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.industry_ore;
            for (int i = 0; i < 1; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Industry_Ore", 0, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                             array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.industry_oil;
            for (int i = 0; i < 1; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Industry_Oil", 0, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.industry_forest;
            for (int i = 0; i < 1; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Industry_Forest", 0, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            array = DataStore.industry_farm;
            for (int i = 0; i < 1; i++)
            {
                consumptionNode.AppendChild(makeConsumeNode(xmlDoc, "Industry_Farm", 0, array[i][DataStore.POWER], array[i][DataStore.WATER],
                                                            array[i][DataStore.SEWAGE], array[i][DataStore.GARBAGE], array[i][DataStore.INCOME]));
            }

            rootNode.AppendChild(consumptionNode);

            if (File.Exists(fullPathFileName))
            {
                try
                {
                    // Create a backup file
                    System.IO.File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
                catch (Exception e)
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, e.Message);
                }
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, e.Message);
                return false;  // Only time when we say there's an error
            }

            return true;
        }

        /**
         * Creates in format   <modifier variable="variableName">value</modifier>
         */
        public XmlNode makePopNode(XmlDocument xmlDoc, String variableName, int value)
        {
            XmlNode userNode = xmlDoc.CreateElement("modifier");
            XmlAttribute attribute = xmlDoc.CreateAttribute("variable");
            attribute.Value = variableName;
            userNode.Attributes.Append(attribute);
            userNode.InnerText = Convert.ToString(value);

            return userNode;
        }


        /**
         * Creates in format   <modifier variable="variableName">value</modifier>
         */
        public XmlNode makeConsumeNode(XmlDocument xmlDoc, String type, int level, int power, int water, int sewage, int garbage, int wealth)
        {
            XmlNode node = xmlDoc.CreateElement("consume");

            XmlAttribute attribute = xmlDoc.CreateAttribute("type");
            attribute.Value = type;
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("level");
            attribute.Value = Convert.ToString(level);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("power");
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

        private void readConsumptionNode(XmlNode consumeNode)
        {
            foreach (XmlNode node in consumeNode)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string attr = node.Attributes["type"].InnerText;
                    int level = Convert.ToInt32(node.Attributes["level"].InnerText) - 1;
                    int power = Convert.ToInt32(node.Attributes["power"].InnerText);
                    int water = Convert.ToInt32(node.Attributes["water"].InnerText);
                    int sewage = Convert.ToInt32(node.Attributes["sewage"].InnerText);
                    int garbage = Convert.ToInt32(node.Attributes["garbage"].InnerText);
                    int wealth = Convert.ToInt32(node.Attributes["wealth"].InnerText);

                    switch (attr)
                    {
                        case "ResidentialLow":
                            setConsumptionRates(DataStore.residentialLow[level], power, water, sewage, garbage, wealth);
                            break;

                        case "ResidentialHigh":
                            setConsumptionRates(DataStore.residentialHigh[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Office":
                            setConsumptionRates(DataStore.office[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry_Generic":
                            setConsumptionRates(DataStore.industry[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry_Ore":
                            setConsumptionRates(DataStore.industry_ore[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry_Oil":
                            setConsumptionRates(DataStore.industry_oil[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry_Forest":
                            setConsumptionRates(DataStore.industry_forest[level], power, water, sewage, garbage, wealth);
                            break;

                        case "Industry_Farm":
                            setConsumptionRates(DataStore.industry_farm[level], power, water, sewage, garbage, wealth);
                            break;
                    }
                }
                catch (Exception e)
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, e.Message);
                }
            }
        }


        private void readPopulationNode(XmlNode popNode)
        {
            foreach (XmlNode node in popNode)
            {
                string attr = node.Attributes["variable"].InnerText;
                int value = Convert.ToInt32(node.InnerText);
                switch (attr)
                {
                    case "ResidentialLow_Level1":
                        DataStore.residentialLow[0][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialLow_Level2":
                        DataStore.residentialLow[1][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialLow_Level3":
                        DataStore.residentialLow[2][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialLow_Level4":
                        DataStore.residentialLow[3][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialLow_Level5":
                        DataStore.residentialLow[4][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialHigh_Level1":
                        DataStore.residentialHigh[0][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialHigh_Level2":
                        DataStore.residentialHigh[1][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialHigh_Level3":
                        DataStore.residentialHigh[2][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialHigh_Level4":
                        DataStore.residentialHigh[3][DataStore.PEOPLE] = value;
                        break;
                    case "ResidentialHigh_Level5":
                        DataStore.residentialHigh[4][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialLow_Level1":
                        DataStore.commercialLow[0][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialLow_Level2":
                        DataStore.commercialLow[1][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialLow_Level3":
                        DataStore.commercialLow[2][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialHigh_Level1":
                        DataStore.commercialHigh[0][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialHigh_Level2":
                        DataStore.commercialHigh[1][DataStore.PEOPLE] = value;
                        break;
                    case "CommercialHigh_Level3":
                        DataStore.commercialHigh[2][DataStore.PEOPLE] = value;
                        break;
                    case "Office_Level1":
                        DataStore.office[0][DataStore.PEOPLE] = value;
                        break;
                    case "Office_Level2":
                        DataStore.office[1][DataStore.PEOPLE] = value;
                        break;
                    case "Office_Level3":
                        DataStore.office[2][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Level1":
                        DataStore.industry[0][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Level2":
                        DataStore.industry[1][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Level3":
                        DataStore.industry[2][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Farming":
                        DataStore.industry_farm[0][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Forestry":
                        DataStore.industry_forest[0][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Ore":
                        DataStore.industry_ore[0][DataStore.PEOPLE] = value;
                        break;
                    case "Industrial_Oil":
                        DataStore.industry_oil[0][DataStore.PEOPLE] = value;
                        break;
                }
            }
        }
        
        private void setConsumptionRates(int[] p, int power, int water, int sewage, int garbage, int wealth)
        {
            p[DataStore.POWER] = power;
            p[DataStore.WATER] = water;
            p[DataStore.SEWAGE] = sewage;
            p[DataStore.GARBAGE] = garbage;
            p[DataStore.INCOME] = wealth;
        }
    }
}