<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TiendasUpload.aspx.vb" MasterPageFile="~/Master/MasterPage.Master" Inherits="ICMTools.TiendasUpload" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<asp:Content ID="Content3" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="container-fluid">
        <div class="d-flex gap-1">
            <a href="../Pages/TiendasUpload.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-upload fa-2x"></i>
                <small>Carga</small>
            </a>
            <a href="../Pages/ExceptionsTiendasUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-upload fa-2x"></i>
                <small>Carga de Excepciones</small>
            </a><a href="../Pages/TiendasDocumentation.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-book fa-2x"></i>
                <small>Documentación</small>
            </a>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Importación de Tiendas con procesos de validación" />
    <meta name="author" content="Equipo SOINF" />
    <title>Tiendas</title>
    <script type="text/javascript">
        const userEmail = "<%= CType(Session.Item("User"), ICMTools.User).Email %>";
        const serverPath = "<%= Page.Server.MapPath("~").Replace("\", "\\") %>";
        const pageConfig = {
            maxFileSize: "<%= ConfigurationManager.AppSettings("maxFileSize")%>",
            fileUploadSelector: "#<%= AsyncFileUpload1.ClientID %>",
            columns: ["Sociedad", "Division", "CR Tienda", "Fecha Inicio", "Fecha Fin", "Monto Diario"],
            types: ["String", "String", "String", "Date", "Date", "Decimal"],
            LogPage: "Tiendas",
            LogType: "Validacion",
            FileType: "Tiendas",
            Extension: ".xlsx",
            LogBody: "Inicia validación para carga de Tiendas",
            Period: "",
            Society: $("#SelectSociety").val(),
            PersonnelDivision: "",
            LoadingValue: 20,
            File: "-1"
        };
        $(document).ready(function () {
            $.AyudaTooltip();
            $('#btnStartImport').on('click', function (e) {
                e.preventDefault();
                StartImportTiendas();
            });
            $('#btnStartImport').prop('disabled', true);
        });
    </script>
    <script src="../js/shared.js"></script>
    <script src="../js/Tiendas.js?v=3"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%: System.Web.Helpers.AntiForgery.GetHtml() %>
    <div class="container">
        <div class="row">
            <div class="col-12">
                <div class="card bg-light card-danger">
                    <div class="card-header lead">Seleccione archivo a cargar (Opcional)</div>
                    <div class="card-body">
                        <form id="myForm" runat="server">
                            <div class="row">
                                <div class="col-md-7">
                                    <div class="row form-group">
                                        <label class="control-label col-sm-5 text-right">Sociedad</label>
                                        <div class="col-sm-7">
                                            <select id="SelectSociety" onchange="$('#SelectPersonnelDivision').empty();loadPersonnelDivisionsBySociety();" class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                                        </div>
                                    </div>
                                    <div class="row form-group">
                                        <label class="control-label col-sm-5 text-right">División de Personal</label>
                                        <div class="col-sm-7">
                                            <select id="SelectPersonnelDivision" class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-5">
                                    <div class="form-group">
                                        <label class="control-label col-sm-12">Archivo de Tiendas</label>
                                        <div class="col">
                                            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                                            <div class="upload-drop-zone" id="drop-zone">
                                                <ajaxToolkit:AsyncFileUpload ID="AsyncFileUpload1" runat="server" ThrobberID="myThrobber" OnClientUploadComplete="uploadComplete" OnClientUploadError="uploadError" OnClientUploadStarted="beforeUploadStarts" Width="100%" ErrorBackColor="#FFCCFF" CompleteBackColor="#CCFFCC" ForeColor="Black" />
                                            </div>
                                            <asp:Label runat="server" ID="myThrobber" Style="display: none;"><i class="fas fa-sync-alt fa-spin fa-fw"></i>Cargando archivo...</asp:Label>
                                            <div class="table-responsive">
                                                <table id="statusUploadTable" class="table table-bordered table-sm"></table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="card-footer">
                        <div class="row">
                            <div class="col">
                                <div id="progressDiv" class="progress" style="display: none; height: 31px;">
                                    <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                                        <span class="sr-only">0%</span>
                                        </div>
                                </div>
                            </div>
                            <div class="col-5">
                                <button id="btnStartImport" class="btn btn-sm btn-primary float-right" data-toggle="tooltip" data-placement="top" title="Click aquí despues de elegir archivo de Tiendas"><i class="fas fa-upload fa-fw"></i>Iniciar Importación</button>
                                <a href="../TemplateFiles/ICMToolsPlantilla_Tiendas.xlsx" class="btn btn-sm btn-primary float-right mr-2" data-toggle="tooltip" data-placement="top" title="Descarga de plantilla para importación de Tiendas"><i class="fas fa-download fa-fw"></i>Descargar Plantilla</a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-12 mt-2">
                <div id="statusAlert" class="alert alert-warning fade" role="alert">
                    <i class="fas fa-exclamation-triangle fa-fw"></i><strong>Importante!</strong> Espere por favor, no actualice la página...
                </div>
            </div>
            <div class="col-12" style="margin-top: -8px;">
                <div id="errorPanel" class="card border-danger" style="display: none;">
                    <div class="card-header text-danger lead">Detalle de problemas<span class="badge badge-danger float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Problema</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameError"></span></h5>
                        <p class="card-text" id="MensajeError">Por favor ajuste el archivo de acuerdo a los problemas detectados y vuelva a intentar la carga.</p>
                        <div id="formatErrors" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <div id="successPanel" class="card border-success" style="display: none;">
                    <div class="card-header text-success lead">Confirmación<span class="badge badge-success float-right"><i class="fas fa-check-circle fa-fw"></i>Listo</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameSuccess"></span></h5>
                        <p class="card-text">La carga fue realizada con exito.</p>
                        <div id="formatSuccess" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <div id="WarningPanel" class="RespuestaPanel card border-warning" style="display: none;">
                    <div class="card-header text-warning lead">Confirmación de carga parcial de Tiendas<span class="badge badge-warning float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Advertencia</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameWarning"></span></h5>
                        <div id="formatWarning" class="pt-3 table-responsive text-default">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="../vendor/bootstrap-4.1.0/dist/js/bootstrap.min.js"></script>
    <script src="../vendor/bootstrap-filestyle-2.1.0/src/bootstrap-filestyle.min.js"></script>
</asp:Content>