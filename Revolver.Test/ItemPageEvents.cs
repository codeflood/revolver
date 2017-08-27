using System.Collections.Generic;
using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Engines.DataCommands;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ItemPageEvents")]
  public class ItemPageEvents : BaseCommandTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Sitecore.Context.IsUnitTesting = true;
      Sitecore.Context.SkipSecurityInUnitTests = true;

      InitContent();
    }

    [Test]
    public void NoParameters_WithEmptyField_ReturnEmptyMessage()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));
      var command = CreateCommand(testItem);

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("No events in field"));
    }

    [Test]
    public void NoParameters_WithInvalidFieldContent_ReturnErrorMessage()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] = "lorem ipsum dolor sit amed";
      }

      var command = CreateCommand(testItem);

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      Assert.That(result.Message, Is.StringMatching("Unable to parse XML from field."));
    }

    [Test]
    public void NoParameters_WithEventsInField_ReturnEventIds()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));
    }

    [Test]
    public void AddAndRemove_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.Remove = true;

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Add_WithInvalidIds_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { "not", "an", "ID" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Add_WithValidId_AddsId()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { "{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("Added page event {C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}"));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.StringContaining("{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
    }

    [Test]
    public void Add_WithMultipleValidId_AddsId()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { "{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}", "{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("Added page event {C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
      Assert.That(result.Message, Is.StringContaining("Added page event {BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}"));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.StringContaining("{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.StringContaining("{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}"));
    }

    [Test]
    public void Add_WithValidUnknownId_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { "{FFF9DFBC-E465-45FD-BA21-0A06EBE942D6}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{FFF9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
    }

    [Test]
    public void Add_WithMultipleValidAndInvalidIds_ReturnsFailureAndDoesNotUpdateField()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);

      command.Add = true;
      command.PageEventIds = new List<string>(new[]
        {"{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}", "{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}"});

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}"));

      // Ensure the valid ID was added
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.StringContaining("{C2D9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
    }

    [Test]
    public void Add_WithValidIdWhenIdExists_DoesNothing()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var initialFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName];

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { "{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.EqualTo(initialFieldValue));
    }

    [Test]
    public void Remove_WithInvalidIds_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "not", "an", "ID" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
    }

    [Test]
    public void Remove_WithValidExistingId_RemovesId()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringMatching("Removed page event {C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));
    }

    [Test]
    public void Remove_WithMultipleValidId_RemovesIds()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /><event id=\"{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}\" name=\"Opportunity\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}", "{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining("Removed page event {C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));
      Assert.That(result.Message, Is.StringContaining("Removed page event {BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}"));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}"));
    }

    [Test]
    public void Remove_WithValidUnknownId_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "{FFF9DFBC-E465-45FD-BA21-0A06EBE942D6}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{FFF9DFBC-E465-45FD-BA21-0A06EBE942D6}"));
    }

    [Test]
    public void Remove_WithMultipleValidAndInvalidIds_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}", "{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}"));
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining("{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}"));
    }

    [Test]
    public void Remove_WithValidNotExistingId_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          "<tracking><event id=\"{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}\" name=\"Error\" /></tracking>";
      }

      var initialFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName];

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { "{BF6B8EE3-9FFB-4C58-9CB4-301C1C710F89}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));
      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.EqualTo(initialFieldValue));
    }

    private Cmd.ItemPageEvents CreateCommand(Item contextItem)
    {
      var command = new Cmd.ItemPageEvents();
      InitCommand(command);
      _context.CurrentItem = contextItem;

      return command;
    }
  }
}