using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Communication.Packets;

public struct ExceptionDTO() : IStructSerializable
{
    public string Site = "";
    public string Message = "";
    public string Source = "";
    public string Inner = "";
    public string Trace = "";
    public string String = "";

    public ExceptionDTO(Exception pE, string pSite) : this()
    {
        this.Site = pSite;
        this.Message = pE.Message;
        this.Source = pE.Source ?? "N/A";
        this.Inner = pE.InnerException?.Message ?? "N/A";
        this.Trace = pE.StackTrace ?? "N/A";
        this.String = pE.ToString();
    }

    public int GetSize()
    {
        return ((Site.Length + Message.Length + Source.Length + Inner.Length + Trace.Length + String.Length) * 2) +
            TerbinProtocol.LENGTH_ARRAY * 6;
    }

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Site = pBuffer.ReadArray<char>(ref offset).CrString();
        Message = pBuffer.ReadArray<char>(ref offset).CrString();
        Source = pBuffer.ReadArray<char>(ref offset).CrString();
        Inner = pBuffer.ReadArray<char>(ref offset).CrString();
        Trace = pBuffer.ReadArray<char>(ref offset).CrString();
        String = pBuffer.ReadArray<char>(ref offset).CrString();
    }

    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Site.ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Message.ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Source.ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Inner.ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Trace.ToCharArray());
        pBuffer.WriteArray<char>(ref offset, String.ToCharArray());
    }
}
