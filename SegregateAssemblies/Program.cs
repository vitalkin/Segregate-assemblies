using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SegregateAssemblies
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = new List<string>();

            foreach (var executable in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe"))
            {
                if (executable.ToLower() == Assembly.GetExecutingAssembly().Location.ToLower())
                {
                    continue;
                }

                Assembly asm;
                try
                {
                    asm = Assembly.ReflectionOnlyLoadFrom(executable);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                DirectoryInfo di = Directory.CreateDirectory(Path.GetFileNameWithoutExtension(executable));

                files.Add(executable);
                foreach (var file in GetDependentAssemblyFiles(asm))
                {
                    files.Add(file.Name);
                }

                foreach (var file in files)
                {
                    File.Copy(file, di.FullName + "\\" + Path.GetFileName(file), true);
                }

                files.Clear();
            }
        }

        static private IEnumerable<FileStream> GetDependentAssemblyFiles(Assembly assembly)
        {
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                Assembly temp;
                try
                {
                    temp = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                if (!temp.GlobalAssemblyCache)
                {
                    foreach (var file in temp.GetFiles(true))
                    {
                        yield return file;
                    }

                    foreach (var file in GetDependentAssemblyFiles(temp))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
