#if false
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace TerbinLibrary.Serialize;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */

public interface IAutoSerializable
{

}

public class AutoStructSerializable : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structs = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is StructDeclarationSyntax,
                transform: static (ctx, _) => GetStruct(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(structs, static (spc, structSymbol) =>
        {
            Generate(spc, (INamedTypeSymbol)structSymbol!);
        });
    }

    private static INamedTypeSymbol? GetStruct(GeneratorSyntaxContext context)
    {
        var structDecl = (StructDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(structDecl);

        if (symbol == null)
            return null;

        foreach (var i in symbol.AllInterfaces)
        {
            if (i.Name == "IStructSerializable")
                return symbol;
        }

        return null;
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol symbol)
    {
        string namespaceName = symbol.ContainingNamespace.ToDisplayString();
        string structName = symbol.Name;

        var sb = new StringBuilder();

        sb.AppendLine($$"""
namespace {{namespaceName}}
{
    public partial struct {{structName}}
    {
        public int GetSize()
        {
            int size = 0;
""");

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            var name = field.Name;
            var type = field.Type;

            if (type is IArrayTypeSymbol arrayType)
            {
                sb.AppendLine($"size += sizeof(ushort) + ({name}?.Length ?? 0);");
            }
            else if (type.AllInterfaces.Any(i => i.Name == "IStructSerializable"))
            {
                sb.AppendLine($"size += {name}.GetSize();");
            }
            else
            {
                sb.AppendLine($"size += sizeof({type.ToDisplayString()});");
            }
        }

        sb.AppendLine("""
            return size;
        }

        public void WriteTo(Span<byte> pBuffer)
        {
            int offset = 0;
""");

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            var name = field.Name;
            var type = field.Type;

            if (type is IArrayTypeSymbol arrayType)
            {
                var element = arrayType.ElementType.ToDisplayString();
                sb.AppendLine($"pBuffer.WriteArray<{element}>(ref offset, {name});");
            }
            else if (type.AllInterfaces.Any(i => i.Name == "IStructSerializable"))
            {
                sb.AppendLine($"{name}.WriteTo(pBuffer[offset..]);");
                sb.AppendLine($"offset += {name}.GetSize();");
            }
            else
            {
                sb.AppendLine($"pBuffer.Write<{type.ToDisplayString()}>(ref offset, {name});");
            }
        }

        sb.AppendLine("""
        }

        public void ReadFrom(ReadOnlySpan<byte> pBuffer)
        {
            int offset = 0;
""");

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            var name = field.Name;
            var type = field.Type;

            if (type is IArrayTypeSymbol arrayType)
            {
                var element = arrayType.ElementType.ToDisplayString();
                sb.AppendLine($"{name} = pBuffer.ReadArray<{element}>(ref offset);");
            }
            else if (type.AllInterfaces.Any(i => i.Name == "IStructSerializable"))
            {
                sb.AppendLine($"{name}.ReadFrom(pBuffer[offset..]);");
                sb.AppendLine($"offset += {name}.GetSize();");
            }
            else
            {
                sb.AppendLine($"{name} = pBuffer.Read<{type.ToDisplayString()}>(ref offset);");
            }
        }

        sb.AppendLine("""
        }
    }
}
""");

        context.AddSource($"{structName}.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

#endif