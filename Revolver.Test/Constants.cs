using Sitecore.Data;

namespace Revolver.Test
{
	public static class Constants
	{
		public static class Paths
		{
      public const string Content = "/sitecore/content";
      public static readonly string Home = "/sitecore/content/" + Names.Home;
      public const string DocTemplate = "Sample/Sample Item";

		    public const string Branch = "Sample Branch"; // used by CreateItem to create the sample branch
      public const string FolderTemplate = "Common/Folder";
      public const string Layout = "Sample Layout";
      public const string Rendering = "Sample/Sample Rendering";
		}

		public static class Names
		{
			public static readonly string Home = "revtesthome";
		}

		public static class Values
		{
			public static readonly int AttributeCount = 9;
			public static readonly int RootChildCount = 5;
		}

        public static class IDs
        {
            public static readonly ID DocTemplateId = new ID("{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");

            public static readonly ID SampleWorkflowId = new ID("{A5BC37E7-ED96-4C1E-8590-A26E64DB55EA}");

            public static readonly ID UserDefinedTemplateFolder = new ID("{B29EE504-861C-492F-95A3-0D890B6FCA09}");

            public static readonly ID ValidateFieldsTemplateId = new ID("{E2452700-5DEF-4335-B213-7FEEF532EA0C}");
        }
	}
}
