using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Banana.AutoCode.DbSchema;
using Microsoft.VisualStudio.TextTemplating;
using Mono.TextTemplating;

namespace Banana.AutoCode.Core
{
    [Serializable]
    public class CustomHost : TemplateGenerator, ITextTemplatingEngineHost
    {
        public CustomHost() : this(AppDomain.CreateDomain("Generation App Domain"))
        { 
            
        }

        public CustomHost(AppDomain appDomain)
        {
            HostDomain = appDomain;
        }

        protected AppDomain HostDomain { get; private set; }

        public Table Table { get; set; }

        //private Dictionary<string, object> _extendProperties = new Dictionary<string, object>();

        public void SetValue(string key, object value)
        {
            HostDomain.SetData(key, value);
        }

        public object GetValue(string key)
        {
            return HostDomain.GetData(key);
        }

        //the path and file name of the text template that is being processed  
        //---------------------------------------------------------------------  
        public string TemplateFile { get; set; }
        
        //This will be the extension of the generated text output file.  
        //The host can provide a default by setting the value of the field here.  
        //The engine can change this value based on the optional output directive  
        //if the user specifies it in the text template.  
        //---------------------------------------------------------------------  
        private string fileExtensionValue = ".cs";
        public string FileExtension
        {
            get { return fileExtensionValue; }
        }
        //This will be the encoding of the generated text output file.  
        //The host can provide a default by setting the value of the field here.  
        //The engine can change this value based on the optional output directive  
        //if the user specifies it in the text template.  
        //---------------------------------------------------------------------  
        private Encoding fileEncodingValue = Encoding.UTF8;
        public Encoding FileEncoding
        {
            get { return fileEncodingValue; }
        }
        
        //The host can provide standard assembly references.  
        //The engine will use these references when compiling and  
        //executing the generated transformation class.  
        //--------------------------------------------------------------  
        public IList<string> StandardAssemblyReferences
        {
            get
            {
                return new string[]  
                {  
                    //If this host searches standard paths and the GAC,  
                    //we can specify the assembly name like this.  
                    //---------------------------------------------------------  
                    //"System"  

                    //Because this host only resolves assemblies from the   
                    //fully qualified path and name of the assembly,  
                    //this is a quick way to get the code to give us the  
                    //fully qualified path and name of the System assembly.  
                    //---------------------------------------------------------  
                    typeof(System.Uri).Assembly.Location,
                    typeof(System.Collections.Generic.IEnumerable<string>).Assembly.Location,
                    typeof(System.Data.DbType).Assembly.Location,
                    typeof(System.Linq.Enumerable).Assembly.Location,
                    typeof(System.Data.SQLite.SQLiteConnection).Assembly.Location,
                    typeof(Oracle.ManagedDataAccess.Client.OracleDbType).Assembly.Location,
                    typeof(CustomHost).Assembly.Location
                };
            }
        }
        //The host can provide standard imports or using statements.  
        //The engine will add these statements to the generated   
        //transformation class.  
        //--------------------------------------------------------------  
        public IList<string> StandardImports
        {
            get
            {
                return new string[]  
                {  
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Text",
                    "System.Data",
                    "System.Data.SQLite",
                    "Oracle.ManagedDataAccess.Client",
                    "Banana.AutoCode",
                    "Banana.AutoCode.DbSchema",
                    "Banana.AutoCode.Core"                    
                };
            }
        }
        
        //The engine calls this method to change the extension of the   
        //generated text output file based on the optional output directive   
        //if the user specifies it in the text template.  
        //---------------------------------------------------------------------  
        public void SetFileExtension(string extension)
        {
            //The parameter extension has a '.' in front of it already.  
            //--------------------------------------------------------  
            fileExtensionValue = extension;
        }
        //The engine calls this method to change the encoding of the   
        //generated text output file based on the optional output directive   
        //if the user specifies it in the text template.  
        //----------------------------------------------------------------------  
        public void SetOutputEncoding(System.Text.Encoding encoding, bool fromOutputDirective)
        {
            fileEncodingValue = encoding;
        }
        //The engine calls this method when it is done processing a text  
        //template to pass any errors that occurred to the host.  
        //The host can decide how to display them.  
        //---------------------------------------------------------------------  
        public void LogErrors(CompilerErrorCollection errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return;
            }

            foreach (CompilerError error in errors)
            {
                Trace.TraceError(error.ErrorText);
            }
        }
    }
}
