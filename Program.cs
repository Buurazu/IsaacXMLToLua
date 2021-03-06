using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace IsaacXMLToLua
{
    class Program
    {
        static void Main(string[] args)
        {
            List<List<float[]>> itemPools = new List<List<float[]>>();
            List<string> itemPoolNames = new List<string>();
            List<int> itemQuality = new List<int>();
            //great name idiot, but idk. it's the list of what pools each item is in, rather than what items are in each pool
            List<List<int>> itemItemPools = new List<List<int>>();

            List<string> recipeInputs = new List<string>();
            List<int> recipeOutputs = new List<int>();

            StreamWriter eidXmlData = new StreamWriter("eid_xmldata.lua", false);

            XmlReader itemMetadataXML = XmlReader.Create("resources/items_metadata.xml");
            XmlReader poolsXML = XmlReader.Create("resources/itempools.xml");
            XmlReader recipesXML = XmlReader.Create("resources/recipes.xml");

            Dictionary<char, int> recipeIngredients = new Dictionary<char, int>();
            //Poop, Penny
            recipeIngredients.Add('_', 29); recipeIngredients.Add('.', 8);
            //Red, Soul, Black Heart
            recipeIngredients.Add('h', 1); recipeIngredients.Add('s', 2); recipeIngredients.Add('b', 3);
            //Key, Bomb, Card
            recipeIngredients.Add('/', 12); recipeIngredients.Add('v', 15); recipeIngredients.Add('[', 21);
            //Gold Heart, Eternal Heart, Pill
            recipeIngredients.Add('g', 5); recipeIngredients.Add('e', 4); recipeIngredients.Add('(', 22);
            //Rotten Heart, Gold Key, Giga Bomb
            recipeIngredients.Add('r', 7); recipeIngredients.Add('|', 13); recipeIngredients.Add('V', 17);
            //Gold Bomb, Bone Heart
            recipeIngredients.Add('^', 16); recipeIngredients.Add('B', 6);
            //Dice Shard, Cracked Key
            recipeIngredients.Add('?', 24); recipeIngredients.Add('~', 25);
            //Is there a way to know the rest of them???

            int currentPool = -1;
            int highestID = 0;

            for (int i = 0; i < 1000; i++)
            {
                itemQuality.Add(-1);
                itemItemPools.Add(new List<int>());
            }

            while (itemMetadataXML.Read())
            {
                if (itemMetadataXML.Name == "item")
                {
                    int id = int.Parse(itemMetadataXML.GetAttribute("id"));
                    if (id > highestID) highestID = id;
                    itemQuality[id] = int.Parse(itemMetadataXML.GetAttribute("quality"));
                }
            }

            while (poolsXML.Read())
            {
                if (poolsXML.Name == "Pool" && poolsXML.NodeType != XmlNodeType.EndElement)
                {
                    itemPools.Add(new List<float[]>());
                    itemPoolNames.Add(poolsXML.GetAttribute("Name"));
                    currentPool++;
                }
                else if (poolsXML.Name == "Item")
                {
                    itemPools[itemPools.Count - 1].Add(new float[] {
                        float.Parse(poolsXML.GetAttribute("Id")), float.Parse(poolsXML.GetAttribute("Weight")) });
                    itemItemPools[int.Parse(poolsXML.GetAttribute("Id"))].Add(currentPool);
                }
            }

            while (recipesXML.Read())
            {
                if (recipesXML.Name == "recipe")
                {
                    string input = recipesXML.GetAttribute("input");
                    char[] chars = input.ToCharArray();
                    string convertedString = "";
                    List<int> components = new List<int>();
                    
                    for (int i = 0; i < chars.Length; i++)
                    {
                        components.Add(recipeIngredients[chars[i]]);
                    }
                    components.Sort();
                    for (int i = 0; i < components.Count; i++)
                    {
                        convertedString += components[i];
                        if (i != components.Count - 1) convertedString += ",";
                    }
                    recipeInputs.Add(convertedString);
                    recipeOutputs.Add(int.Parse(recipesXML.GetAttribute("output")));
                }
            }

            eidXmlData.WriteLine("--This file was autogenerated using https://github.com/Buurazu/IsaacXMLToLua");
            eidXmlData.WriteLine("--It will have to be updated whenever the game's item XML files are updated\n");

            eidXmlData.WriteLine("--The highest item ID found");
            eidXmlData.WriteLine("EID.XMLMaxItemID = " + highestID);

            eidXmlData.WriteLine("--The fixed recipes, for use in Bag of Crafting");
            eidXmlData.Write("EID.XMLRecipes = {");
            for (int i = 0; i < recipeInputs.Count; i++)
            {
                eidXmlData.Write("[\"" + recipeInputs[i] + "\"] = " + recipeOutputs[i] + ",");
            }
            eidXmlData.WriteLine("}");

            eidXmlData.WriteLine("--The contents of each item pool, and the item's weight, for use in Bag of Crafting");
            eidXmlData.Write("EID.XMLItemPools = {");
            for (int i = 0; i < itemPoolNames.Count; i++)
            {
                eidXmlData.Write("{");
                for (int j = 0; j < itemPools[i].Count; j++)
                {
                    eidXmlData.Write("{" + itemPools[i][j][0] + "," + itemPools[i][j][1] + "},");
                }
                eidXmlData.WriteLine("}, -- " + itemPoolNames[i]);
            }
            eidXmlData.WriteLine("}");

            eidXmlData.WriteLine("--The quality of each item, for use in Bag of Crafting");
            eidXmlData.Write("EID.XMLItemQualities = {");
            for (int i = 0; i < itemQuality.Count; i++)
            {
                if (itemQuality[i] >= 0)
                {
                    eidXmlData.Write("[" + i + "]=" + itemQuality[i] + ",");
                }
            }
            eidXmlData.WriteLine("}");

            eidXmlData.WriteLine("--The pools that each item is in, for roughly checking if a given item is unlocked");
            eidXmlData.Write("EID.XMLItemIsInPools = {");
            for (int i = 0; i < itemItemPools.Count; i++)
            {
                if (itemItemPools[i].Count > 0)
                {
                    eidXmlData.Write("[" + i + "]={");
                    for (int j = 0; j < itemItemPools[i].Count; j++) {
                        eidXmlData.Write(itemItemPools[i][j] + ",");
                    }
                    eidXmlData.Write("},");
                }
            }
            eidXmlData.WriteLine("}");



            eidXmlData.Flush();
            eidXmlData.Close();
        }
    }
}
