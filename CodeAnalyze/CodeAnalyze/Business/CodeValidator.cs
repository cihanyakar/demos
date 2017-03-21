using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyze.Business
{
    public struct ErrorMessage
    {
        public string Where { get; set; }
        public string LineNumber { get; set; }
        public string Message { get; set; }
        public RemoteFile File { get; set; }
    }

    public static class CodeValidator
    {

        public static async Task<List<ErrorMessage>> ValidateFile(RemoteFile file, CancellationToken cancelationToken)
        {
            var errorList = new List<ErrorMessage>();

            try
            {
                var csharpCode = file.Content;
                var codeTree = CSharpSyntaxTree.ParseText(csharpCode,
                                                          new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.None),
                                                          cancellationToken: cancelationToken);

                var codeRoot = await codeTree.GetRootAsync(cancelationToken);

                errorList.AddRange(codeRoot.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(x => !char.IsUpper(x.Identifier.ToString()[0]))
                    .Select(x => new ErrorMessage { File = file, LineNumber = x.SyntaxTree.GetLineSpan(x.Span).StartLinePosition.Line.ToString(), Where = x.Identifier.ToString(), Message = "Method İsimleri Büyük Harfle başlamalı" }));
            }
            catch (Exception ex)
            {
                errorList.Add(new ErrorMessage { File = file, Where = file.Name, Message = "Kod çözümlenemedi. " + ex.Message });
            }

            return errorList;
        }
    }
}
