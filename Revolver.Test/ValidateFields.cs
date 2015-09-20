using NUnit.Framework;
using NUnit.Framework.Constraints;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  
  [TestFixture]
  [Category("Validate Fields")]
  public class ValidateFields : BaseCommandTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;
      this.InitContent();
      CreateTestTemplate();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      Item testTemplate = this._context.CurrentDatabase.GetItem(Constants.IDs.ValidateFieldsTemplateId);
      testTemplate.Delete();
    }

    private void CreateTestTemplate()
    {
      Item UserDefinedTemplatesFolder = this._context.CurrentDatabase.GetItem(
          Constants.IDs.UserDefinedTemplateFolder);
      Item item = TestUtil.CreateContentFromFile(
          "TestResources\\validate fields template.xml", UserDefinedTemplatesFolder, false);
    }

    [Test]
    public void ValidateFields_FieldWithValidID_ReturnsSuccessMessage()
    {
      var cmd = new Cmd.ValidateFields();
      InitCommand(cmd);

      var goodItem = this.CreateTestItem("good item", "abc 123 xyz");
      cmd.Path = goodItem.Paths.FullPath;
      CommandResult result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringStarting("PASSED: Validation passed for"));
    }

    [Test]
    public void ValidateFields_FieldWithInvalidContent_ReturnsFailedMessage()
    {
      var cmd = new Cmd.ValidateFields();
      InitCommand(cmd);

      var badItem = this.CreateTestItem("bad item", "abc $ xyz");
      cmd.Path = badItem.Paths.FullPath;
      CommandResult result = cmd.Run();

      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, new StartsWithConstraint("FAILED: Validation failed for"));
      Assert.That(result.Message, Contains.Substring("\"Alphanumeric characters and spaces only.\""));
    }

    private Item CreateTestItem(string itemName, string fieldValue)
    {
      Item goodItem = this._testRoot.Add(itemName, new TemplateID(Constants.IDs.ValidateFieldsTemplateId));
      goodItem.Editing.BeginEdit();
      goodItem.Fields["alpha numeric field"].Value = fieldValue;
      goodItem.Editing.EndEdit();
      return goodItem;
    }
  }
}
