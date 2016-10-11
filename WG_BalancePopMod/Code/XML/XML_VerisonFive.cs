using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace WG_BalancedPopMod
{
    public class XML_VersionFive : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string bonusHouseName = "bonusHouseHold";
        private const string bonusWorkName = "bonusWorker";
        private const string meshName = "meshName";
        private const string consumeNodeName = "consumption";
        private const string visitNodeName = "visitor";
        private const string pollutionNodeName = "pollution";
        private const string productionNodeName = "production";

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
                try
                {
                    if (node.Name.Equals(popNodeName))
                    {
                        readPopulationNode(node);
                    }
                    else if (node.Name.Equals(consumeNodeName))
                    {
                        readConsumptionNode(node);
                    }
                    else if (node.Name.Equals(visitNodeName))
                    {
                        readVisitNode(node);
                    }
                    else if (node.Name.Equals(pollutionNodeName))
                    {
                        readPollutionNode(node);
                    }
                    else if (node.Name.Equals(productionNodeName))
                    {
                        readProductionNode(node);
                    }
                    else if (node.Name.Equals(bonusHouseName))
                    {
                        readBonusHouseNode(node);
                    }
                    else if (node.Name.Equals(bonusWorkName))
                    {
                        readBonusWorkers(node);
                    }
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning(e.Message);
                    UnityEngine.Debug.LogException(e);
                }
            }
        } // end readXML


        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool writeXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CityMod");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "5";
            rootNode.Attributes.Append(attribute);

            /*
            attribute = xmlDoc.CreateAttribute("experimental");
            attribute.Value = DataStore.enableExperimental ? "true" : "false";
            rootNode.Attributes.Append(attribute);
            */

            xmlDoc.AppendChild(rootNode);

            XmlNode popNode = xmlDoc.CreateElement(popNodeName);
            attribute = xmlDoc.CreateAttribute("strictCapacity");
            attribute.Value = DataStore.strictCapacity ? "true" : "false";
            popNode.Attributes.Append(attribute);

            XmlNode consumeNode = xmlDoc.CreateElement(consumeNodeName);
            XmlNode visitNode = xmlDoc.CreateElement(visitNodeName);
            XmlNode pollutionNode = xmlDoc.CreateElement(pollutionNodeName);
            XmlNode productionNode = xmlDoc.CreateElement(productionNodeName);

            try
            {
                makeNodes(xmlDoc, "ResidentialLow", DataStore.residentialLow, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "ResidentialHigh", DataStore.residentialHigh, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialLow", DataStore.commercialLow, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialHigh", DataStore.commercialHigh, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialLeisure", DataStore.commercialLeisure, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "CommercialTourist", DataStore.commercialTourist, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "Office", DataStore.office, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "Industry", DataStore.industry, popNode, consumeNode, visitNode, pollutionNode, productionNode);

                makeNodes(xmlDoc, "IndustryFarm", DataStore.industry_farm, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryForest", DataStore.industry_forest, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryOre", DataStore.industry_ore, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                makeNodes(xmlDoc, "IndustryOil", DataStore.industry_oil, popNode, consumeNode, visitNode, pollutionNode, productionNode);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }

            // First segment
            createPopulationNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(popNode);
            createConsumptionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(consumeNode);
            createVisitNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(visitNode);
            createProductionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(productionNode);
            createPollutionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(pollutionNode);

            // Add mesh names to XML
            XmlNode bonusHouseholdNode = xmlDoc.CreateElement(bonusHouseName);
            attribute = xmlDoc.CreateAttribute("printResNames");
            attribute.Value = DataStore.printResidentialNames ? "true" : "false";
            bonusHouseholdNode.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("mergeResNames");
            attribute.Value = DataStore.mergeResidentialNames ? "true" : "false";
            bonusHouseholdNode.Attributes.Append(attribute);

            XmlComment comment = xmlDoc.CreateComment("Defaults: L1 2x3 Detached05,L1 3x3 Detached02,L1 4x4 Detached02,L1 4x4 Detached06a,");
            bonusHouseholdNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("L1 4x4 Detached11,L2 2x2 Detached05,L2 2x3 Semi-detachedhouse01,L2 3x4 Semi-detachedhouse02a,");
            bonusHouseholdNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("L3 3x3 Semi-detachedhouse02,L3 4x4 Semi-detachedhouse03a");
            bonusHouseholdNode.AppendChild(comment);

            SortedList<string, int> list = new SortedList<string, int>(DataStore.bonusHouseholdCache);
            foreach (string data in list.Keys)
            {
                XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                meshNameNode.InnerXml = data;
                attribute = xmlDoc.CreateAttribute("bonus");
                int value = 1;
                DataStore.bonusHouseholdCache.TryGetValue(data, out value);
                attribute.Value = Convert.ToString(value);
                meshNameNode.Attributes.Append(attribute);
                bonusHouseholdNode.AppendChild(meshNameNode);
            }
            rootNode.AppendChild(bonusHouseholdNode); // Append the bonusHousehold to root

            // Add mesh names to dictionary
            XmlNode bonusWorkNode = xmlDoc.CreateElement(bonusWorkName);
            attribute = xmlDoc.CreateAttribute("printWorkNames");
            attribute.Value = DataStore.printEmploymentNames ? "true" : "false";
            bonusWorkNode.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("mergeWorkNames");
            attribute.Value = DataStore.mergeEmploymentNames ? "true" : "false";
            bonusWorkNode.Attributes.Append(attribute);

            list = new SortedList<string, int>(DataStore.bonusWorkerCache);
            foreach (string data in list.Keys)
            {
                XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                meshNameNode.InnerXml = data;
                attribute = xmlDoc.CreateAttribute("bonus");
                int value = 1;
                DataStore.bonusWorkerCache.TryGetValue(data, out value);
                attribute.Value = Convert.ToString(value);
                meshNameNode.Attributes.Append(attribute);
                bonusWorkNode.AppendChild(meshNameNode);
            }
            rootNode.AppendChild(bonusWorkNode); // Append the bonusWorkers to root

            try
            {
                if (File.Exists(fullPathFileName))
                {
                    if (File.Exists(fullPathFileName + ".bak"))
                    {
                        File.Delete(fullPathFileName + ".bak");
                    }

                    File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
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
        } // end writeXML


        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createPopulationNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("space_pp = Square metres per person");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("level_height = Height of a floor. The height of chimneys have been taken into account");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("calc = model or plot. To calculate the base using either the building model, or by the land size");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("visit_mult = The number of visitors as a multiple of workers to 1 decimal place. This is used for commercial only");
//            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("lvl_0 ... lvl_3 = Proportional values between the education levels (uneducated, educated, well educated, highly educated). Does not need to be percentages.");
            rootNode.AppendChild(comment);
        }


        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createConsumptionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Consumption values are per household, or per production unit");
            rootNode.AppendChild(comment);
        }

        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createVisitNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Visitor Values are multiplies of 100th of a person per cell.");
            rootNode.AppendChild(comment);
        }

        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createPollutionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Ground pollution is not used by residential, commercial and offices.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("Noise pollution is not spread over the land by residential or offices.");
            rootNode.AppendChild(comment);
        }


        /// <param name="xmlDoc"></param>
        /// <param name="rootNode"></param>
        private void createProductionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Production for offices is number of employees per production unit.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("For industry, it used as hundredths of a unit per cell block.");
            rootNode.AppendChild(comment);
        }


        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        private void makeNodes(XmlDocument xmlDoc, String buildingType, int[][] array, XmlNode rootPopNode, XmlNode consumNode, XmlNode visitNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                makeNodes(xmlDoc, buildingType, array[i], i, rootPopNode, consumNode, visitNode, pollutionNode, productionNode);
            }
        }


        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="array"></param>
        /// <param name="level"></param>
        /// <param name="rootPopNode"></param>
        /// <param name="consumNode"></param>
        /// <param name="pollutionNode"></param>
        private void makeNodes(XmlDocument xmlDoc, String buildingType, int[] array, int level, XmlNode rootPopNode, XmlNode consumNode, XmlNode visitNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            makePopNode(rootPopNode, xmlDoc, buildingType, level, array);
            makeConsumeNode(consumNode, xmlDoc, buildingType, level, array[DataStore.POWER], array[DataStore.WATER], array[DataStore.SEWAGE], array[DataStore.GARBAGE], array[DataStore.INCOME]);
            makeVisitNode(visitNode, xmlDoc, buildingType, level, array);
            makePollutionNode(pollutionNode, xmlDoc, buildingType, level, array[DataStore.GROUND_POLLUTION], array[DataStore.NOISE_POLLUTION]);
            makeProductionNode(productionNode, xmlDoc, buildingType, level, array[DataStore.PRODUCTION]);
        }


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

            attribute = xmlDoc.CreateAttribute("calc");
            attribute.Value = array[DataStore.CALC_METHOD] == 0 ? "model" : "plot";
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


        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="array"></param>
        private void makeVisitNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int[] array)
        {
            if (array[DataStore.VISIT] >= 0)
            {
                XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("visit");
                attribute.Value = Convert.ToString(array[DataStore.VISIT]);
                node.Attributes.Append(attribute);

                root.AppendChild(node);
            }
        }


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
                    Debugging.bufferWarning("readPollutionNode: " + name + " " + e.Message);
                }
            } // end foreach
        }


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
                    Debugging.bufferWarning("readConsumptionNode: " + e.Message);
                }
            }
        }


        /// <param name="popNode"></param>
        private void readPopulationNode(XmlNode popNode)
        {
            try
            {
                DataStore.strictCapacity = Convert.ToBoolean(popNode.Attributes["strictCapacity"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in popNode.ChildNodes)
            {
                // TODO - These two to be removed in Jan 2017
                if (node.Name.Equals(bonusHouseName))
                {
                    readBonusHouseNode(node);
                }
                else if (node.Name.Equals(bonusWorkName))
                {
                    readBonusWorkers(node);
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
                        Debugging.bufferWarning("readPopulationNode, part a: " + e.Message);
                    }

                    try
                    {
                        if (Convert.ToBoolean(node.Attributes["calc"].InnerText.Equals("plot")))
                        {
                            array[DataStore.CALC_METHOD] = 1;
                        }
                        else
                        {
                            array[DataStore.CALC_METHOD] = 0;
                        }
                    }
                    catch
                    {

                    }

                    if (!name.Contains("Residential"))
                    {
                        try
                        {
                            int dense = Convert.ToInt32(node.Attributes["ground_mult"].InnerText);
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
                            Debugging.bufferWarning("readPopulationNode, part b: " + e.Message);
                        }
                    }
                } // end if
            } // end foreach
        }


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


        /// <param name="node"></param>
        private void readBonusHouseNode(XmlNode parent)
        {
            try
            {
                DataStore.printResidentialNames = Convert.ToBoolean(parent.Attributes["printResNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            try
            {
                DataStore.mergeResidentialNames = Convert.ToBoolean(parent.Attributes["mergeResNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int bonus = 1;
                        bonus = Convert.ToInt32(node.Attributes["bonus"].InnerText);

                        if (name.Length > 0)
                        {
                            // Needs a value to be valid
                            DataStore.bonusHouseholdCache.Add(name, bonus);
                        }
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning("readBonusHouseNode: " + e.Message + ". Setting to 1");
                    }
                }
            }
        }

        /// <param name="node"></param>
        private void readBonusWorkers(XmlNode parent)
        {
            try
            {
                DataStore.printEmploymentNames = Convert.ToBoolean(parent.Attributes["printWorkNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int bonus = 5;
                        bonus = Convert.ToInt32(node.Attributes["bonus"].InnerText);

                        if (name.Length > 0)
                        {
                            // Needs a value to be valid
                            DataStore.bonusWorkerCache.Add(name, bonus);
                        }
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning("readBonusWorkers: " + e.Message + ". Setting to 5");
                    }
                }
            }
        }

        /// <param name="produceNode"></param>
        private void readVisitNode(XmlNode produceNode)
        {
            foreach (XmlNode node in produceNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int[] array = getArray(name, level, "readVisitNode");

                    array[DataStore.VISIT] = Convert.ToInt32(node.Attributes["visit"].InnerText);
                    if (array[DataStore.VISIT] <= 0)
                    {
                        array[DataStore.VISIT] = 1;
                    }
                }
                catch (Exception e)
                {
                    Debugging.bufferWarning("readVisitNode: " + e.Message);
                }
            }
        }


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
                    Debugging.bufferWarning("readProductionNode: " + e.Message);
                }
            }
        }


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
        } // end getArray


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