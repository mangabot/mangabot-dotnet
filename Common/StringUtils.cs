﻿namespace Common
{
    // Exclude this method because it is used by WebScaper which invokes this method by method name via reflection
    public class StringUtils
    {
        public static string GenerateOrdinal(int totalRows, int currentRowIndex)
        {
            string name = "";
            int lengthOfName = totalRows.ToString().Length - currentRowIndex.ToString().Length;
            while (lengthOfName > 0)
            {
                name += "0";
                lengthOfName--;
            }
            name += currentRowIndex;
            return name;
        }
    }
}
