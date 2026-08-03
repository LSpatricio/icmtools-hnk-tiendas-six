<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="CargaArchivo.aspx.vb" Inherits="ICMTools.CargarArchivo" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript" >
        $(document).ready(function () {

        });

        function fillCell(row, cellNumber, text) {
            var cell = row.insertCell(cellNumber);
            cell.innerHTML = text;
            cell.style.borderBottom = cell.style.borderRight = "solid 1px #aaaaff";
        }
        function addToClientTable(name, text) {
            var table = document.getElementById("<%= TableClientSide.ClientID%>");
            var row = table.insertRow(0);
            fillCell(row, 0, name);
            fillCell(row, 1, text);
        }

        function uploadError(sender, args) {
            addToClientTable(args.get_fileName(), "<span style='color:red;'>" + args.get_errorMessage() + "</span>");
        }

        function uploadComplete(sender, args) {
            var contentType = args.get_contentType();
            var text = args.get_length() + " bytes";
            if (contentType.length > 0) {
                text += ", '" + contentType + "'";
            }
            addToClientTable(args.get_fileName(), text);
        }
        
        function beforeUploadStarts(sender, args) {
            var filename = args.get_fileName();
            console.log(args);
            var ext = filename.substring(filename.lastIndexOf(".") + 1);
            if (ext != 'xlsx') {
                throw {
                    name: "Invalid File Type",
                    level: "Error",
                    message: "Tipo de archivo invalido (Solo .xlsx)",
                    htmlMessage: "Invalid File Type (Only .xlsx)"
                }
                return false;
            }
            return true;
        }

    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="container">
        <form id="FormUploadFile" runat="server">
            <asp:Label runat="server" ID="myThrobber" Style="display: none;"><i class="fas fa-sync-alt fa-spin fa-fw"></i>Cargando archivo...</asp:Label>

            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <ajaxToolkit:AsyncFileUpload ID="AsyncFileUpload1" runat="server" ThrobberID="myThrobber" OnClientUploadComplete="uploadComplete" OnClientUploadError="uploadError" OnClientUploadStarted="beforeUploadStarts" />
            
            <br />
            
            <strong>Ultimo evento Server-side:</strong><asp:Label runat="server" Text=" " ID="uploadResult" />
            
            <br />
            
            <strong>Eventos Client-side:</strong>            
            <table class="table table-condensed table-striped"  runat="server" id="TableClientSide"></table>

        </form>
    </div>
           
       
        

           
       
</asp:Content>
