﻿using System.IO;
using NiDump;


namespace SMPEditor
{
    public static class NifFileEx
    {
        public static string GetNiNodeName(this NiHeader hdr,int id)
        {
            try
            {
                int typeIndex = hdr.GetBlockTypeIdxByName("NiNode");
            }
            catch (System.Exception)
            {
                return "";
            }            
            //Console.WriteLine("BT idx 'NiNode': {0}", bt_NiNode);
            if (id >= hdr.blocks.Length) return "";
            return hdr.strings[hdr.GetObject<NiNode>(id).name];
        }
    }
}
