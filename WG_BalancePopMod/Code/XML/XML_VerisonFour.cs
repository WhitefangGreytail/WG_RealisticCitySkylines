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
        private const string bonusHouseName = "bonusHouseMeshName";
        private const string consumeNodeName = "consumption";
        private const string pollutionNodeName = "pollution";
        private const string productionNodeName = "production";
        private const char STRING_DELIMITER = ',';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public override void readXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;
            try
            {
                //DataStore.enableExperimental = Convert.ToBoolean(root.Attributes["experimental"].InnerText);
                //DataStore.timeBasedRealism = Convert.ToBoolean(root.Attributes["enableTimeVariation"].InnerText);
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
                else if (node.Name.Equals(bonusHouseName))
                {
                    readBonusHouseNode(node);
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
            return false;
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
                if (node.Name.Equals(bonusHouseName))
                {
                    string[] values = node.InnerText.Split(new char[] { STRING_DELIMITER });

                    DataStore.xmlMeshNames.Clear(); // Okay to clear now that we have seen the meshname node
                    foreach (string name in values)
                    {
                        DataStore.xmlMeshNames.Add(name);
                    }
                }
                else 
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
                    else
                    {
                        try
                        {
                            int bonus = Convert.ToInt32(node.Attributes["bonus_houseHold"].InnerText);
                            array[DataStore.BONUS_HOUSEHOLD] = bonus >= 0 ? bonus : 0;  // Force to be greater than 0
                        }
                        catch (Exception e)
                        {
                            Debugging.panelMessage("readPopulationNode, part c: " + e.Message);
                        }
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
        /// <param name="node"></param>
        private void readBonusHouseNode(XmlNode node)
        {
            try
            {
                string[] values = node.InnerText.Split(new char[] { STRING_DELIMITER });
                foreach(string value in values)
                {
                    // Marge into the list or dictionary
                }
            }
            catch (Exception e)
            {
                Debugging.panelMessage("readBonusHouseNode: " + e.Message);
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