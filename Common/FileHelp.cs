using System.IO;
public class FileHelp
{
    public static string GetFile(string path)
    {
        string res=string.Empty;
        if(File.Exists(path)){
            using(StreamReader sr=new StreamReader(path)){
                res=sr.ReadToEnd();
            }
        }
        return res;
    }

    public static string GetAppPath(){
        return "";
    }
}