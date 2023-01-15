using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoCvm
{
    internal static class CommandLineBuilderExtensions
    {
        /// <summary>
        /// Configures the application to show help when no tokens are passed on the command line.
        /// </summary>
        /// <param name="builder">A command line builder.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder"/>.</returns>
        public static CommandLineBuilder ShowHelpOnNoTokens(this CommandLineBuilder builder)
        {
            builder.AddMiddleware(async (context, next) =>
            {
                if (!context.ParseResult.Tokens.Any())
                {
                    context.InvocationResult = new HelpResult();
                }
                else
                {
                    await next(context);
                }
            }, MiddlewareOrder.Default);

            return builder;
        }
    }
}
