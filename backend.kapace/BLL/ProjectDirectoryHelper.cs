namespace backend.kapace.BLL;

public static class ProjectDirectoryHelper
{
    public static string GetProjectDirectory()
    {
        const string reundant = @"\bin\Debug\net7.0";
        var root = Directory.GetCurrentDirectory();
        root = root.Replace(reundant, "");

        /// For ubuntu architecture.
        if (root.EndsWith("app"))
        {
            return root;
        }
        
        /// For windows architecture.
        while (!root.EndsWith("backend.kapace"))
        {
            var directoryInfo = Directory.GetParent(root);
            if (directoryInfo == null)
            {
                return root;
            }

            root = directoryInfo.FullName;
        }
        return root;
    }
}