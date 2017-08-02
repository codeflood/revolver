using NUnit.Framework;
using Revolver.Core;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using System.Xml;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("CopyPresentation")]
  public class CopyPresentation : BaseCommandTest
  {
    private Item _sourceItem = null;
    private Item _targetItem = null;
    private string _defaultDeviceId = string.Empty;
    private string _printDeviceId = string.Empty;

    [TestFixtureSetUp]
    public void Init()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      _defaultDeviceId = _context.CurrentDatabase.Resources.Devices["default"].ID.ToString();
      _printDeviceId = _context.CurrentDatabase.Resources.Devices["print"].ID.ToString();

      InitContent();
    }

    [SetUp]
    public void SetUp()
    {
      _context.CurrentDatabase = Sitecore.Configuration.Factory.GetDatabase("web");
      var template = _context.CurrentDatabase.Templates[Constants.Paths.DocTemplate];

      _sourceItem = _testRoot.Add("source item", template);
      _targetItem = _testRoot.Add("target item", template);

      using (new EditContext(_targetItem))
      {
        _targetItem[FieldIDs.LayoutField] = string.Empty;
      }
    }

    [TearDown]
    public void TearDown()
    {
      _testRoot.DeleteChildren();
    }

    [Test]
    public void NoPaths()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      var previousLayout = _sourceItem[FieldIDs.LayoutField];

      var result = cmd.Run();
      _sourceItem.Reload();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.AreEqual(previousLayout, _sourceItem[FieldIDs.LayoutField]);
    }

    [Test]
    public void InvalidSourceDevice()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = "this is an invalid device";
      cmd.TargetDeviceName = _context.CurrentDatabase.Resources.Devices.GetAll()[0].Name;

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void InvalidTargetDevice()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = _context.CurrentDatabase.Resources.Devices.GetAll()[0].Name;
      cmd.TargetDeviceName = "this is an invalid device";

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void MissingTargetDevice()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = _context.CurrentDatabase.Resources.Devices.GetAll()[0].Name;

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Failure, result.Status);
    }

    [Test]
    public void BetweenDevicesOnSameItem()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = "default";
      cmd.TargetDeviceName = "print";
      cmd.SourcePath = _context.CurrentItem.ID.ToString();
      cmd.TargetPath = _context.CurrentItem.Paths.FullPath;

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();

      var layout = LayoutDefinition.Parse(LayoutField.GetFieldValue(_sourceItem.Fields[FieldIDs.LayoutField]));
      var defaultDevice = layout.GetDevice(_defaultDeviceId);
      var printDevice = layout.GetDevice(_printDeviceId);

      AssertLayout(defaultDevice, printDevice);
    }

    [Test]
    public void BetweenDevicesNoPaths()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = "default";
      cmd.TargetDeviceName = "print";

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();

      var layout = LayoutDefinition.Parse(LayoutField.GetFieldValue(_sourceItem.Fields[FieldIDs.LayoutField]));
      var defaultDevice = layout.GetDevice(_defaultDeviceId);
      var printDevice = layout.GetDevice(_printDeviceId);

      AssertLayout(defaultDevice, printDevice);
    }

    [Test]
    public void BetweenDevicesByIDOnSameItem()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = _defaultDeviceId;
      cmd.TargetDeviceName = _printDeviceId;
      cmd.SourcePath = _context.CurrentItem.ID.ToString();
      cmd.TargetPath = _context.CurrentItem.ID.ToString();

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();

      var layout = LayoutDefinition.Parse(LayoutField.GetFieldValue(_sourceItem.Fields[FieldIDs.LayoutField]));
      var defaultDevice = layout.GetDevice(_defaultDeviceId);
      var printDevice = layout.GetDevice(_printDeviceId);

      AssertLayout(defaultDevice, printDevice);
    }

    [Test]
    public void AllDevicesToTarget()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.TargetPath = "../" + _targetItem.Name;

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();
      _targetItem.Reload();

      Assert.AreEqual(_sourceItem[FieldIDs.LayoutField], _targetItem[FieldIDs.LayoutField]);
    }

    [Test]
    public void AllDevicesFromSource()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _targetItem;

      cmd.SourcePath = _sourceItem.ID.ToString();

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();
      _targetItem.Reload();

      Assert.AreEqual(_sourceItem[FieldIDs.LayoutField], _targetItem[FieldIDs.LayoutField]);
    }

    [Test]
    public void SameDevicesOnDifferentItems()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _testRoot;

      cmd.SourceDeviceName = "default";
      cmd.TargetDeviceName = "default";
      cmd.SourcePath = _sourceItem.Name;
      cmd.TargetPath = _targetItem.Name;

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _targetItem.Reload();

      var sourceDevice = LayoutDefinition.Parse(LayoutField.GetFieldValue(_sourceItem.Fields[FieldIDs.LayoutField])).GetDevice(_defaultDeviceId);
      var targetDevice = LayoutDefinition.Parse(LayoutField.GetFieldValue(_targetItem.Fields[FieldIDs.LayoutField])).GetDevice(_defaultDeviceId);

      AssertLayout(sourceDevice, targetDevice);
    }

    [Test]
    public void SameDeviceSameItem()
    {
      var cmd = new Cmd.CopyPresentation();
      InitCommand(cmd);

      _context.CurrentItem = _sourceItem;

      cmd.SourceDeviceName = "default";
      cmd.TargetDeviceName = "default";
      cmd.SourcePath = _context.CurrentItem.ID.ToString();
      cmd.TargetPath = _context.CurrentItem.Paths.FullPath;

      // Sometimes playing with the layout field can introduce non-significant whitespace
      var layoutBefore = new XmlDocument();
      layoutBefore.LoadXml(_sourceItem[FieldIDs.LayoutField]);

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      _sourceItem.Reload();
      var layoutAfter = new XmlDocument();
      layoutAfter.LoadXml(_sourceItem[FieldIDs.LayoutField]);
      Assert.AreEqual(layoutBefore.DocumentElement.OuterXml, layoutAfter.DocumentElement.OuterXml);
    }

    private void AssertLayout(DeviceDefinition expected, DeviceDefinition actual)
    {
      Assert.That(actual.Layout, Is.EqualTo(expected.Layout));
      Assert.That(actual.Renderings.Count, Is.EqualTo(expected.Renderings.Count));
      
      for (var i = 0; i < expected.Renderings.Count; i++)
      {
        Assert.That(((RenderingDefinition)actual.Renderings[i]).ToXml(), Is.EqualTo(((RenderingDefinition)expected.Renderings[i]).ToXml()));
      }
    }
  }
}
