using System;
using System.Collections.Generic;
using NUnit.Framework;
using Revolver.Core;
using Sitecore.Data;
using Sitecore.Data.Engines.DataCommands;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("ItemPageEvents")]
  public class ItemPageEvents : BaseCommandTest
  {
    private readonly Guid LoginGoalId = Guid.Parse("{66722F52-2D13-4DCC-90FC-EA7117CF2298}");
    private readonly Guid ErrorPageEventId = Guid.Parse("{C8BF254A-9CCC-4E16-9009-82B7CD33E4BE}");
    private readonly Guid SearchPageEventId = Guid.Parse("{0C179613-2073-41AB-992E-027D03D523BF}");

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
          $"<tracking><event id=\"{LoginGoalId}\" name=\"Login\" /></tracking>";
      }

      var command = CreateCommand(testItem);

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message, Is.StringContaining(LoginGoalId.ToString()));
    }

    [Test]
    public void AddAndRemove_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{LoginGoalId}\" name=\"Login\" /></tracking>";
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
          $"<tracking><event id=\"{LoginGoalId}\" name=\"Login\" /></tracking>";
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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { LoginGoalId.ToString() });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message.ToLower(), Is.StringMatching($"added page event {LoginGoalId:B}"));

      testItem.Reload();
      var trackingFieldValeue = testItem[Cmd.ItemPageEvents.TrackingFieldName].ToLower();
      Assert.That(trackingFieldValeue, Is.StringContaining(LoginGoalId.ToString("D")));
    }

    [Test]
    public void Add_WithMultipleValidId_AddsId()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { LoginGoalId.ToString(), SearchPageEventId.ToString() });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message.ToLower(), Is.StringContaining($"added page event {LoginGoalId:B}"));
      Assert.That(result.Message.ToLower(), Is.StringContaining($"added page event {SearchPageEventId:B}"));

      testItem.Reload();
      var trackingFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName].ToLower();
      Assert.That(trackingFieldValue, Is.StringContaining(LoginGoalId.ToString("D")));
      Assert.That(trackingFieldValue, Is.StringContaining(SearchPageEventId.ToString("D")));
    }

    [Test]
    public void Add_WithValidUnknownId_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);

      command.Add = true;
      command.PageEventIds = new List<string>(new[]
        {LoginGoalId.ToString(), "{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}"});

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));

      testItem.Reload();
      var trackingFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName].ToLower();
      Assert.That(trackingFieldValue, Is.Not.StringContaining("{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}"));

      // Ensure the valid ID was added
      Assert.That(trackingFieldValue, Is.StringContaining(LoginGoalId.ToString("D")));
    }

    [Test]
    public void Add_WithValidIdWhenIdExists_DoesNothing()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var initialFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName];

      var command = CreateCommand(testItem);
      command.Add = true;
      command.PageEventIds = new List<string>(new[] { ErrorPageEventId.ToString() });

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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { ErrorPageEventId.ToString() });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message.ToLower(), Is.StringMatching($"removed page event {ErrorPageEventId:B}"));

      testItem.Reload();
      var trackingFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName].ToLower();
      Assert.That(trackingFieldValue, Is.Not.StringContaining(ErrorPageEventId.ToString()));
    }

    [Test]
    public void Remove_WithMultipleValidId_RemovesIds()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /><event id=\"{LoginGoalId}\" name=\"Login\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { ErrorPageEventId.ToString(), LoginGoalId.ToString() });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
      Assert.That(result.Message.ToLower(), Is.StringContaining($"removed page event {ErrorPageEventId:B}"));
      Assert.That(result.Message.ToLower(), Is.StringContaining($"removed page event {LoginGoalId:B}"));

      testItem.Reload();
      var trackingFieldValue = testItem[Cmd.ItemPageEvents.TrackingFieldName].ToLower();
      Assert.That(trackingFieldValue, Is.Not.StringContaining(ErrorPageEventId.ToString()));
      Assert.That(trackingFieldValue, Is.Not.StringContaining(LoginGoalId.ToString()));
    }

    [Test]
    public void Remove_WithValidUnknownId_ReturnsFailure()
    {
      // arrange
      var testItem = _testRoot.Add(ID.NewID.ToShortID().ToString(), new TemplateID(Constants.IDs.DocTemplateId));

      using (new EditContext(testItem))
      {
        testItem[Cmd.ItemPageEvents.TrackingFieldName] =
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
      }

      var command = CreateCommand(testItem);
      command.Remove = true;
      command.PageEventIds = new List<string>(new[] { ErrorPageEventId.ToString(), "{FFFB8EE3-9FFB-4C58-9CB4-301C1C710F89}" });

      // act
      var result = command.Run();

      // assert
      Assert.That(result.Status, Is.EqualTo(CommandStatus.Failure));

      testItem.Reload();
      Assert.That(testItem[Cmd.ItemPageEvents.TrackingFieldName], Is.Not.StringContaining(ErrorPageEventId.ToString()));
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
          $"<tracking><event id=\"{ErrorPageEventId}\" name=\"Error\" /></tracking>";
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