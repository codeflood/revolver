using System;
using System.Collections.Generic;
using NUnit.Framework;
using Revolver.Core;
using Revolver.Core.Formatting;
using Mod = Revolver.Core.Commands;

namespace Revolver.Test
{
    [TestFixture]
    [Category("AliasCommand")]
    public class AliasCommand
    {
        private TextOutputFormatter _formatter = null;

        public AliasCommand()
        {
            _formatter = new TextOutputFormatter();
        }

        [Test]
        public void AddAlias()
        {
            // arrange
            var ctx = CreateContext();
            var sut = new Mod.AliasCommand();

            sut.Initialise(ctx, _formatter);

            // act
            var result = sut.Run(new[] { "cc", "c" });

			// assert
			Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
            Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));

            var cmdArgs = ctx.CommandHandler.FindCommandAlias("cc");

            Assert.That(cmdArgs, Is.Not.Null);
            Assert.That(cmdArgs.CommandName, Is.EqualTo("c"));
        }

        [Test]
        public void ListAliases()
        {
            // arrange
            var ctx = CreateContext();
            ctx.CommandHandler.AddCommandAlias("cc", "c");

            var sut = new Mod.AliasCommand();
            sut.Initialise(ctx, _formatter);

            // act
            var result = sut.Run(new string[0]);

            // assert
            Assert.That(result.Status, Is.EqualTo(CommandStatus.Success));
            Assert.That(result.Message, Is.StringMatching("c\\s+cc"));
        }

        [Test]
        public void RemoveAlias()
        {
            // arrange
            var ctx = CreateContext();
            ctx.CommandHandler.AddCommandAlias("cc", "c");

            var sut = new Mod.AliasCommand();

            sut.Initialise(ctx, _formatter);

            // act
            var result = sut.Run(new[] { "cc" });

            // assert
            Assert.That(result.Status, Is.EqualTo(CommandStatus.Success), result.Message);
            Assert.That(ctx.CommandHandler.CustomCommands, Contains.Item(new KeyValuePair<string, Type>("c", typeof(CustomCommand))));

            var cmdArgs = ctx.CommandHandler.FindCommandAlias("cc");

            Assert.That(cmdArgs, Is.Null);
        }

        private Core.Context CreateContext()
        {
            var ctx = new Core.Context();
            ctx.CommandHandler = new Core.CommandHandler(ctx, _formatter);
            ctx.CommandHandler.AddCustomCommand("c", typeof(CustomCommand));

            return ctx;
        }
    }
}