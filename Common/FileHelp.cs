using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class FileHelp
{
    public static async Task<string> GetFile(string path)
    {
        string res=string.Empty;
        if(File.Exists(path)){
            using(StreamReader sr=new StreamReader(path)){
                res=await sr.ReadToEndAsync();
            }
        }
        return res;
    }
    public static List<string> ReadFile(string filePath) 
    {
        List<string> result=new List<string>();
        if (File.Exists(filePath))
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                //while (sr.ReadLine())
                //{

                //}
            }
        }
        return result;
    }
    public static string GetAppPath(){
        return "";
    }
}